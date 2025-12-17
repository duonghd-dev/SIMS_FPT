// SIMS_FPT/Areas/Instructor/Controllers/AssignmentController.cs
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SIMS_FPT.Business.Interfaces;
using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Models;
using SIMS_FPT.Models.ViewModels;
using SIMS_FPT.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;

namespace SIMS_FPT.Areas.Instructor.Controllers
{
    [Area("Instructor")]
    public class AssignmentController : Controller
    {
        // ... (Keep existing constructor and dependencies) ...
        private readonly IInstructorAssignmentService _assignmentService;
        private readonly IClassRepository _classRepo;
        private readonly IClassSubjectRepository _classSubjectRepo;
        private readonly ISubjectRepository _subjectRepo;
        private readonly IGradingService _gradingService;
        private readonly ISubmissionRepository _submissionRepo;
        private readonly IWebHostEnvironment _env;

        public AssignmentController(
            IInstructorAssignmentService assignmentService,
            IClassRepository classRepo,
            IClassSubjectRepository classSubjectRepo,
            ISubjectRepository subjectRepo,
            ISubmissionRepository submissionRepo,
            IGradingService gradingService,
            IWebHostEnvironment env)
        {
            _assignmentService = assignmentService;
            _classRepo = classRepo;
            _classSubjectRepo = classSubjectRepo;
            _subjectRepo = subjectRepo;
            _submissionRepo = submissionRepo;
            _gradingService = gradingService;
            _env = env;
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

        // [HELPER] Load Subjects for Dropdown
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

        // [HELPER] Load Classes for a specific Subject
        private void LoadClassesForSubject(string subjectId, string? selectedClassId = null)
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
                    ClassId = c.ClassId,
                    DisplayText = $"{c.ClassId} - {c.ClassName}"
                })
                .ToList();

            ViewBag.Classes = new SelectList(classes, "ClassId", "DisplayText", selectedClassId);
        }

        // [HELPER] Standard Class Loader (Backward compatibility for Edit)
        private void LoadTeacherClasses(string? selectedClassId = null)
        {
            var teacherId = CurrentTeacherId;
            var teacherClassIds = _classSubjectRepo.GetAll()
                .Where(cs => cs.TeacherId == teacherId)
                .Select(cs => cs.ClassId)
                .Distinct()
                .ToList();

            var myClasses = _classRepo.GetAll()
                .Where(c => teacherClassIds.Contains(c.ClassId))
                .Select(c => new
                {
                    ClassId = c.ClassId,
                    DisplayText = $"{c.ClassId} - {c.ClassName}"
                })
                .ToList();

            ViewBag.Classes = new SelectList(myClasses, "ClassId", "DisplayText", selectedClassId);
        }

        // [AJAX] Endpoint to get classes when subject changes
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

        // [UPDATED] Index Action with Search and Filter
        public IActionResult Index(string? searchString, string? subjectId, string? classId)
        {
            var teacherId = CurrentTeacherId;
            var assignments = _assignmentService.GetTeacherAssignments(teacherId);

            // 1. Filter by Search String (Title)
            if (!string.IsNullOrEmpty(searchString))
            {
                assignments = assignments.Where(a => a.Title.Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            // 2. Filter by Subject
            if (!string.IsNullOrEmpty(subjectId))
            {
                assignments = assignments.Where(a => a.SubjectId == subjectId).ToList();
            }

            // 3. Filter by Class
            if (!string.IsNullOrEmpty(classId))
            {
                assignments = assignments.Where(a => a.ClassId == classId).ToList();
            }

            // Populate Dropdowns
            LoadTeacherSubjects(subjectId);

            if (!string.IsNullOrEmpty(subjectId))
            {
                // If subject selected, load specific classes
                LoadClassesForSubject(subjectId, classId);
            }
            else
            {
                // Initialize empty or default
                ViewBag.Classes = new SelectList(new List<object>(), "ClassId", "DisplayText");
            }

            ViewData["SearchString"] = searchString;
            return View(assignments);
        }

        // ... (Keep the rest of the existing methods: Create, Edit, Grade, Delete, etc.) ...

        public IActionResult Create()
        {
            LoadTeacherSubjects();
            ViewBag.Classes = new SelectList(new List<object>(), "ClassId", "DisplayText");
            return View();
        }

        [HttpPost]
        public IActionResult Create(AssignmentModel model)
        {
            ModelState.Remove("TeacherId");
            ModelState.Remove("AssignmentId");

            if (ModelState.IsValid)
            {
                var (success, message) = _assignmentService.CreateAssignment(model, CurrentTeacherId);
                if (success)
                    return RedirectToAction("Index");
                else
                    ModelState.AddModelError("", message);
            }

            LoadTeacherSubjects(model.SubjectId);
            LoadClassesForSubject(model.SubjectId, model.ClassId);
            return View(model);
        }

        public IActionResult Edit(string id)
        {
            var assignment = _assignmentService.GetAssignmentById(id, CurrentTeacherId);
            if (assignment == null)
                return RedirectToAction("AccessDenied", "Login", new { area = "" });

            // Ensure SubjectId is valid for the dropdown
            LoadTeacherSubjects(assignment.SubjectId);
            // Load classes specifically for this subject so the current class is selected
            LoadClassesForSubject(assignment.SubjectId, assignment.ClassId);

            return View(assignment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(AssignmentModel model)
        {
            ModelState.Remove("TeacherId");
            // SubjectId might be disabled/readonly in view, ensure it binds or is retrieved

            if (ModelState.IsValid)
            {
                var (success, message) = _assignmentService.UpdateAssignment(model, CurrentTeacherId);
                if (success)
                    return RedirectToAction("Index");
                else
                    ModelState.AddModelError("", message);
            }

            LoadTeacherSubjects(model.SubjectId);
            LoadClassesForSubject(model.SubjectId, model.ClassId);
            return View(model);
        }

        [HttpGet]
        public IActionResult Grade(string id)
        {
            var model = _gradingService.PrepareGradingView(id, CurrentTeacherId);
            if (model == null) return RedirectToAction("AccessDenied", "Login", new { area = "" });
            return View(model);
        }

        [HttpPost]
        public IActionResult Grade(BulkGradeViewModel model)
        {
            var assignment = _assignmentService.GetAssignmentById(model.AssignmentId, CurrentTeacherId);
            if (assignment == null) return RedirectToAction("AccessDenied", "Login", new { area = "" });

            _gradingService.ProcessGrades(model);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult DownloadSubmission(string assignmentId, string studentId)
        {
            var assignment = _assignmentService.GetAssignmentById(assignmentId, CurrentTeacherId);
            if (assignment == null) return RedirectToAction("AccessDenied", "Login", new { area = "" });

            var submission = _submissionRepo.GetByStudentAndAssignment(studentId, assignmentId);
            if (submission == null || string.IsNullOrEmpty(submission.FilePath)) return NotFound("File not found.");

            var relativePath = submission.FilePath.TrimStart('/', '\\');
            var filePath = Path.Combine(_env.WebRootPath, relativePath);

            if (!System.IO.File.Exists(filePath)) return NotFound($"File not found.");

            var stream = System.IO.File.OpenRead(filePath);
            return File(stream, "application/octet-stream", Path.GetFileName(filePath));
        }

        [HttpGet]
        public IActionResult PreviewSubmission(string assignmentId, string studentId)
        {
            var assignment = _assignmentService.GetAssignmentById(assignmentId, CurrentTeacherId);
            if (assignment == null) return RedirectToAction("AccessDenied", "Login", new { area = "" });

            var submission = _submissionRepo.GetByStudentAndAssignment(studentId, assignmentId);
            if (submission == null || string.IsNullOrEmpty(submission.FilePath)) return NotFound("File not found.");

            var relativePath = submission.FilePath.TrimStart('/', '\\');
            var filePath = Path.Combine(_env.WebRootPath, relativePath);

            if (!System.IO.File.Exists(filePath)) return NotFound($"File not found.");

            var ext = Path.GetExtension(filePath).ToLowerInvariant();
            var contentType = ext switch
            {
                ".pdf" => "application/pdf",
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".txt" => "text/plain",
                ".html" => "text/html",
                _ => "application/octet-stream"
            };

            var stream = System.IO.File.OpenRead(filePath);
            return File(stream, contentType);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(string id)
        {
            var (success, message) = _assignmentService.DeleteAssignment(id, CurrentTeacherId);
            if (success) TempData["Success"] = message;
            else TempData["Error"] = message;
            return RedirectToAction("Index");
        }
    }
}