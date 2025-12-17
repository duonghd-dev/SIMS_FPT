using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SIMS_FPT.Models;
using SIMS_FPT.Services.Interfaces;
using SIMS_FPT.Data.Interfaces; // [NEW] Added for repositories
using System.Linq;
using System.Security.Claims;
using System.Collections.Generic;
using System;

namespace SIMS_FPT.Areas.Instructor.Controllers
{
    [Area("Instructor")]
    [Authorize]
    public class CourseMaterialController : Controller
    {
        private readonly IInstructorCourseMaterialService _materialService;
        private readonly IClassRepository _classRepo; // [NEW]
        private readonly IClassSubjectRepository _classSubjectRepo; // [NEW]
        private readonly ISubjectRepository _subjectRepo; // [NEW]

        public CourseMaterialController(
            IInstructorCourseMaterialService materialService,
            IClassRepository classRepo,
            IClassSubjectRepository classSubjectRepo,
            ISubjectRepository subjectRepo)
        {
            _materialService = materialService;
            _classRepo = classRepo;
            _classSubjectRepo = classSubjectRepo;
            _subjectRepo = subjectRepo;
        }

        private string CurrentTeacherId
        {
            get
            {
                var linkedId = User.FindFirst("LinkedId")?.Value;
                if (!string.IsNullOrEmpty(linkedId)) return linkedId;
                return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity?.Name ?? "UNKNOWN";
            }
        }

        // [HELPER] Load Subjects
        private void LoadTeacherSubjects(string? selectedSubjectId = null)
        {
            var teacherId = CurrentTeacherId;
            var teacherSubjectIds = _classSubjectRepo.GetByTeacherId(teacherId)
                .Select(cs => cs.SubjectId)
                .Distinct()
                .ToList();

            var subjects = _subjectRepo.GetAll()
                .Where(s => teacherSubjectIds.Contains(s.SubjectId))
                .Select(s => new
                {
                    SubjectId = s.SubjectId,
                    DisplayText = $"{s.SubjectId} - {s.SubjectName}"
                })
                .ToList();

            ViewBag.Subjects = new SelectList(subjects, "SubjectId", "DisplayText", selectedSubjectId);
        }

        // [HELPER] Load Classes for Subject
        private void LoadClassesForSubject(string subjectId, string? selectedClassId = null)
        {
            if (string.IsNullOrEmpty(subjectId))
            {
                ViewBag.Classes = new SelectList(new List<object>(), "ClassId", "DisplayText");
                return;
            }

            var teacherId = CurrentTeacherId;
            var classIds = _classSubjectRepo.GetAll()
                .Where(cs => cs.TeacherId == teacherId && cs.SubjectId == subjectId)
                .Select(cs => cs.ClassId)
                .Distinct()
                .ToList();

            var classes = _classRepo.GetAll()
                .Where(c => classIds.Contains(c.ClassId))
                .Select(c => new
                {
                    ClassId = c.ClassId,
                    DisplayText = $"{c.ClassId} - {c.ClassName}"
                })
                .ToList();

            ViewBag.Classes = new SelectList(classes, "ClassId", "DisplayText", selectedClassId);
        }

        // [AJAX]
        [HttpGet]
        public IActionResult GetClassesBySubject(string subjectId)
        {
            var teacherId = CurrentTeacherId;
            var classIds = _classSubjectRepo.GetAll()
                .Where(cs => cs.TeacherId == teacherId && cs.SubjectId == subjectId)
                .Select(cs => cs.ClassId)
                .Distinct()
                .ToList();

            var classes = _classRepo.GetAll()
                .Where(c => classIds.Contains(c.ClassId))
                .Select(c => new
                {
                    id = c.ClassId,
                    name = $"{c.ClassId} - {c.ClassName}"
                })
                .ToList();

            return Json(classes);
        }

        // [UPDATED] Index with Filter
        public IActionResult Index(string? searchString, string? subjectId, string? classId)
        {
            var materials = _materialService.GetTeacherMaterials(CurrentTeacherId);

            // Filter logic
            if (!string.IsNullOrEmpty(searchString))
                materials = materials.Where(m => m.Title.Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();

            if (!string.IsNullOrEmpty(subjectId))
                materials = materials.Where(m => m.SubjectId == subjectId).ToList();

            if (!string.IsNullOrEmpty(classId))
                materials = materials.Where(m => m.ClassId == classId).ToList();

            // Load Filter Dropdowns
            LoadTeacherSubjects(subjectId);
            LoadClassesForSubject(subjectId, classId);
            ViewData["SearchString"] = searchString;

            return View(materials);
        }

        // [UPDATED] Create GET
        public IActionResult Create()
        {
            LoadTeacherSubjects();
            ViewBag.Classes = new SelectList(new List<object>(), "ClassId", "DisplayText");
            return View();
        }

        // [UPDATED] Create POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CourseMaterialModel model, IFormFile uploadFile)
        {
            ModelState.Remove("MaterialId");
            ModelState.Remove("UploadDate");

            if (ModelState.IsValid)
            {
                var (success, message, filePath) = _materialService.CreateMaterial(model, uploadFile, CurrentTeacherId);
                if (success)
                {
                    TempData["Success"] = message;
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, message);
                }
            }

            // Reload dropdowns on error
            LoadTeacherSubjects(model.SubjectId);
            LoadClassesForSubject(model.SubjectId, model.ClassId);
            TempData["Error"] = "Please fix the errors below and try again.";
            return View(model);
        }

        [HttpPost]
        public IActionResult Delete(string id)
        {
            var (success, message) = _materialService.DeleteMaterial(id, CurrentTeacherId);
            if (!success)
                TempData["Error"] = message;
            else
                TempData["Success"] = message;

            return RedirectToAction("Index");
        }
        public IActionResult Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var material = _materialService.GetMaterialById(id);
            if (material == null) return NotFound();

            // Load dropdowns with the current material's selections
            LoadTeacherSubjects(material.SubjectId);
            LoadClassesForSubject(material.SubjectId, material.ClassId);

            return View(material);
        }

        // [NEW] Edit POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CourseMaterialModel model, IFormFile? uploadFile)
        {
            // Remove validations for fields that might not be re-submitted or are read-only
            ModelState.Remove("UploadDate");
            // If the user doesn't upload a new file, FilePath might be null in the form, 
            // but we keep the old one in the service. 
            // If your model requires FilePath, remove that validation here if uploadFile is null.

            if (ModelState.IsValid)
            {
                var (success, message) = _materialService.UpdateMaterial(model, uploadFile, CurrentTeacherId);
                if (success)
                {
                    TempData["Success"] = message;
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, message);
                }
            }

            // Reload dropdowns on error
            LoadTeacherSubjects(model.SubjectId);
            LoadClassesForSubject(model.SubjectId, model.ClassId);
            TempData["Error"] = "Failed to update material. Please check inputs.";
            return View(model);
        }
    }
}