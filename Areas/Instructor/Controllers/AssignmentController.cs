using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SIMS_FPT.Business.Interfaces;
using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Models;
using SIMS_FPT.Models.ViewModels;
using SIMS_FPT.Services.Interfaces;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;

namespace SIMS_FPT.Areas.Instructor.Controllers
{
    [Area("Instructor")]
    public class AssignmentController : Controller
    {
        private readonly IInstructorAssignmentService _assignmentService;
        private readonly IClassRepository _classRepo;
        private readonly IClassSubjectRepository _classSubjectRepo;
        private readonly IGradingService _gradingService;
        private readonly ISubmissionRepository _submissionRepo;
        private readonly IWebHostEnvironment _env;

        public AssignmentController(
            IInstructorAssignmentService assignmentService,
            IClassRepository classRepo,
            IClassSubjectRepository classSubjectRepo,
            ISubmissionRepository submissionRepo,
            IGradingService gradingService,
            IWebHostEnvironment env)
        {
            _assignmentService = assignmentService;
            _classRepo = classRepo;
            _classSubjectRepo = classSubjectRepo;
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

        public IActionResult Index()
        {
            return View(_assignmentService.GetTeacherAssignments(CurrentTeacherId));
        }

        public IActionResult Create()
        {
            LoadTeacherClasses();
            return View();
        }

        [HttpPost]
        public IActionResult Create(AssignmentModel model)
        {
            ModelState.Remove("TeacherId");
            ModelState.Remove("AssignmentId");
            ModelState.Remove("SubjectId");

            if (ModelState.IsValid)
            {
                var (success, message) = _assignmentService.CreateAssignment(model, CurrentTeacherId);
                if (success)
                    return RedirectToAction("Index");
                else
                    ModelState.AddModelError("ClassId", message);
            }

            LoadTeacherClasses(model.ClassId ?? "");
            return View(model);
        }

        public IActionResult Edit(string id)
        {
            var assignment = _assignmentService.GetAssignmentById(id, CurrentTeacherId);
            if (assignment == null)
                return RedirectToAction("AccessDenied", "Login", new { area = "" });

            LoadTeacherClasses(assignment.ClassId ?? "");
            return View(assignment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(AssignmentModel model)
        {
            ModelState.Remove("TeacherId");
            ModelState.Remove("SubjectId");

            if (ModelState.IsValid)
            {
                var (success, message) = _assignmentService.UpdateAssignment(model, CurrentTeacherId);
                if (success)
                    return RedirectToAction("Index");
                else
                {
                    ModelState.AddModelError("ClassId", message);
                    if (message.Contains("denied"))
                        return RedirectToAction("AccessDenied", "Login", new { area = "" });
                }
            }

            LoadTeacherClasses(model.ClassId ?? "");
            return View(model);
        }

        [HttpGet]
        public IActionResult Grade(string id)
        {
            var model = _gradingService.PrepareGradingView(id, CurrentTeacherId);
            if (model == null)
                return RedirectToAction("AccessDenied", "Login", new { area = "" });

            return View(model);
        }

        [HttpPost]
        public IActionResult Grade(BulkGradeViewModel model)
        {
            var assignment = _assignmentService.GetAssignmentById(model.AssignmentId, CurrentTeacherId);
            if (assignment == null)
                return RedirectToAction("AccessDenied", "Login", new { area = "" });

            _gradingService.ProcessGrades(model);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult DownloadSubmission(string assignmentId, string studentId)
        {
            var assignment = _assignmentService.GetAssignmentById(assignmentId, CurrentTeacherId);
            if (assignment == null)
                return RedirectToAction("AccessDenied", "Login", new { area = "" });

            var submission = _submissionRepo.GetByStudentAndAssignment(studentId, assignmentId);
            if (submission == null || string.IsNullOrEmpty(submission.FilePath))
                return NotFound("Submission file not found.");

            var relativePath = submission.FilePath.TrimStart('/', '\\');
            var filePath = Path.Combine(_env.WebRootPath, relativePath);

            if (!System.IO.File.Exists(filePath))
                return NotFound($"File not found on server.");

            var stream = System.IO.File.OpenRead(filePath);
            var fileName = Path.GetFileName(filePath);

            return File(stream, "application/octet-stream", fileName);
        }

        [HttpGet]
        public IActionResult PreviewSubmission(string assignmentId, string studentId)
        {
            var assignment = _assignmentService.GetAssignmentById(assignmentId, CurrentTeacherId);
            if (assignment == null)
                return RedirectToAction("AccessDenied", "Login", new { area = "" });

            var submission = _submissionRepo.GetByStudentAndAssignment(studentId, assignmentId);
            if (submission == null || string.IsNullOrEmpty(submission.FilePath))
                return NotFound("Submission file not found.");

            var relativePath = submission.FilePath.TrimStart('/', '\\');
            var filePath = Path.Combine(_env.WebRootPath, relativePath);
            if (!System.IO.File.Exists(filePath))
                return NotFound("File not found on server.");

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
            var assignment = _assignmentService.GetAssignmentById(id, CurrentTeacherId);
            if (assignment == null)
                return RedirectToAction("AccessDenied", "Login", new { area = "" });

            // Actually delete the assignment
            var (success, message) = _assignmentService.DeleteAssignment(id, CurrentTeacherId);
            if (success)
                TempData["Success"] = message;
            else
                TempData["Error"] = message;

            return RedirectToAction("Index");
        }
    }
}
