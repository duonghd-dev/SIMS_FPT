using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        private readonly IWebHostEnvironment _env;

        public CourseMaterialController(ICourseMaterialRepository materialRepo,
                                        ISubjectRepository subjectRepo,
                                        IClassRepository classRepo,
                                        IWebHostEnvironment env)
        {
            _materialRepo = materialRepo;
            _subjectRepo = subjectRepo;
            _classRepo = classRepo;
            _env = env;
        }

        // Helper to get the logged-in Teacher's ID
        private string CurrentTeacherId
        {
            get
            {
                var linkedId = User.FindFirst("LinkedId")?.Value;
                if (!string.IsNullOrEmpty(linkedId)) return linkedId;
                return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;
            }
        }

        // Helper to get subjects that this teacher is assigned to (via Classes)
        private List<SubjectModel> GetAllowedSubjects()
        {
            var teacherId = CurrentTeacherId;
            // Find all classes taught by this teacher
            var teacherClasses = _classRepo.GetAll()
                .Where(c => c.TeacherName == teacherId)
                .ToList();

            // Extract unique Subject IDs from those classes
            var subjectIds = teacherClasses
                .Select(c => c.SubjectName)
                .Distinct()
                .ToList();

            // Return the full Subject objects
            var allSubjects = _subjectRepo.GetAll();
            return allSubjects.Where(s => subjectIds.Contains(s.SubjectId)).ToList();
        }

        public IActionResult Index()
        {
            // 1. Get the list of allowed subjects for the current instructor
            var allowedSubjects = GetAllowedSubjects();
            var allowedSubjectIds = allowedSubjects.Select(s => s.SubjectId).ToList();

            // 2. Filter materials: Only show materials linked to subjects this instructor teaches
            var materials = _materialRepo.GetAll()
                .Where(m => allowedSubjectIds.Contains(m.SubjectId))
                .OrderByDescending(m => m.UploadDate)
                .ToList();

            ViewBag.Subjects = allowedSubjects;
            return View(materials);
        }

        public IActionResult Create()
        {
            // Only populate the dropdown with subjects the instructor teaches
            ViewBag.Subjects = GetAllowedSubjects();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CourseMaterialModel model, IFormFile uploadFile)
        {
            ModelState.Remove("MaterialId"); // generated server-side
            ModelState.Remove("UploadDate"); // set server-side

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

            // Security Check: Ensure the instructor teaches the selected Subject
            var allowedSubjects = GetAllowedSubjects();
            if (!allowedSubjects.Any(s => s.SubjectId == model.SubjectId))
            {
                ModelState.AddModelError("SubjectId", "You are not authorized to add materials for this subject.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Subjects = allowedSubjects;
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
                ViewBag.Subjects = allowedSubjects;
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

            // Security Check: Ensure the instructor teaches the subject of the material they are trying to delete
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