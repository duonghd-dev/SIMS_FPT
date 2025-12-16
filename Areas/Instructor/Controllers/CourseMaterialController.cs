using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // Required for SelectListItem
using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;

namespace SIMS_FPT.Areas.Instructor.Controllers
{
    [Area("Instructor")]
    [Authorize]
    public class CourseMaterialController : Controller
    {
        private readonly ICourseMaterialRepository _materialRepo;
        private readonly ISubjectRepository _subjectRepo;
        private readonly IClassRepository _classRepo;
        private readonly IClassSubjectRepository _classSubjectRepo;
        private readonly IWebHostEnvironment _env;

        public CourseMaterialController(ICourseMaterialRepository materialRepo,
                                        ISubjectRepository subjectRepo,
                                        IClassRepository classRepo,
                                        IClassSubjectRepository classSubjectRepo,
                                        IWebHostEnvironment env)
        {
            _materialRepo = materialRepo;
            _subjectRepo = subjectRepo;
            _classRepo = classRepo;
            _classSubjectRepo = classSubjectRepo;
            _env = env;
        }

        // Helper to get the logged-in Teacher's ID
        private string CurrentTeacherId
        {
            get
            {
                var linkedId = User.FindFirst("LinkedId")?.Value;
                if (!string.IsNullOrEmpty(linkedId)) return linkedId;
                return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity?.Name ?? "UNKNOWN";
            }
        }

        // Helper to get subjects that this teacher is assigned to (via ClassSubject)
        private List<SubjectModel> GetAllowedSubjects()
        {
            var teacherId = CurrentTeacherId;
            var teacherSubjectIds = _classSubjectRepo.GetAll()
                .Where(cs => cs.TeacherId == teacherId)
                .Select(cs => cs.SubjectId)
                .Distinct()
                .ToList();

            var allSubjects = _subjectRepo.GetAll();
            return allSubjects.Where(s => teacherSubjectIds.Contains(s.SubjectId)).ToList();
        }

        // [NEW] Helper to generate the "Class Name - Subject Name" dropdown list
        private List<SelectListItem> GetClassSelectionList()
        {
            var teacherId = CurrentTeacherId;
            var teacherClassIds = _classSubjectRepo.GetAll()
                .Where(cs => cs.TeacherId == teacherId)
                .Select(cs => cs.ClassId)
                .Distinct()
                .ToList();

            var teacherClasses = _classRepo.GetAll()
                .Where(c => teacherClassIds.Contains(c.ClassId))
                .ToList();

            var result = new List<SelectListItem>();

            foreach (var c in teacherClasses)
            {
                // Get subjects taught in this class
                var classSubjects = _classSubjectRepo.GetByClassId(c.ClassId!);
                foreach (var cs in classSubjects)
                {
                    var subject = _subjectRepo.GetById(cs.SubjectId!);
                    if (subject != null)
                    {
                        result.Add(new SelectListItem
                        {
                            Value = cs.SubjectId,
                            Text = $"{c.ClassName ?? c.ClassId ?? "Unknown"} - {subject.SubjectName ?? subject.SubjectId ?? "Unknown"}"
                        });
                    }
                }
            }

            return result;
        }

        public IActionResult Index()
        {
            var allowedSubjects = GetAllowedSubjects();
            var allowedSubjectIds = allowedSubjects.Select(s => s.SubjectId).ToList();

            var materials = _materialRepo.GetAll()
                .Where(m => allowedSubjectIds.Contains(m.SubjectId))
                .OrderByDescending(m => m.UploadDate)
                .ToList();

            ViewBag.Subjects = allowedSubjects;
            return View(materials);
        }

        public IActionResult Create()
        {
            // Use the new helper to populate ViewBag.SubjectList
            ViewBag.SubjectList = GetClassSelectionList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CourseMaterialModel model, IFormFile uploadFile)
        {
            ModelState.Remove("MaterialId");
            ModelState.Remove("UploadDate");

            if (uploadFile == null && string.IsNullOrWhiteSpace(model.VideoUrl))
            {
                ModelState.AddModelError(string.Empty, "Please upload a file or provide a video link.");
            }

            if (uploadFile != null)
            {
                var ext = Path.GetExtension(uploadFile.FileName).ToLowerInvariant();
                var allowed = new[] { ".pdf", ".ppt", ".pptx" };
                if (!allowed.Contains(ext))
                {
                    ModelState.AddModelError(string.Empty, "Only PDF, PPT, or PPTX files are allowed.");
                }
            }

            // Security Check
            var allowedSubjects = GetAllowedSubjects();
            if (!allowedSubjects.Any(s => s.SubjectId == model.SubjectId))
            {
                ModelState.AddModelError("SubjectId", "You are not authorized to add materials for this subject.");
            }

            if (!ModelState.IsValid)
            {
                // Repopulate list on error
                ViewBag.SubjectList = GetClassSelectionList();
                TempData["Error"] = "Please fix the errors below and try again.";
                return View(model);
            }

            if (uploadFile != null)
            {
                var dir = Path.Combine(_env.WebRootPath, "materials", model.SubjectId ?? "general");
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(uploadFile.FileName)}";
                var filePath = Path.Combine(dir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    uploadFile.CopyTo(stream);
                }

                model.FilePath = Path.Combine("materials", model.SubjectId ?? "general", fileName).Replace("\\", "/");
            }

            model.UploadDate = DateTime.Now;
            if (string.IsNullOrEmpty(model.MaterialId)) model.MaterialId = Guid.NewGuid().ToString();
            try
            {
                _materialRepo.Add(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Save failed: {ex.Message}";
                // Repopulate list on exception
                ViewBag.SubjectList = GetClassSelectionList();
                return View(model);
            }
            TempData["Success"] = "Material saved successfully.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Delete(string id)
        {
            var mat = _materialRepo.GetById(id);
            if (mat == null) return RedirectToAction("Index");

            var allowedSubjects = GetAllowedSubjects();
            if (!allowedSubjects.Any(s => s.SubjectId == mat.SubjectId))
            {
                TempData["Error"] = "You are not authorized to delete this material.";
                return RedirectToAction("Index");
            }

            if (!string.IsNullOrEmpty(mat.FilePath))
            {
                var phys = Path.Combine(_env.WebRootPath, mat.FilePath.TrimStart('/', '\\'));
                if (System.IO.File.Exists(phys))
                {
                    System.IO.File.Delete(phys);
                }
            }

            _materialRepo.Delete(id);
            return RedirectToAction("Index");
        }
    }
}