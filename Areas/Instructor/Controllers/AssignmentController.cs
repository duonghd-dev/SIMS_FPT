using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using SIMS_FPT.Business.Interfaces;
using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Models;
using SIMS_FPT.Models.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;

namespace SIMS_FPT.Areas.Instructor.Controllers
{
    [Area("Instructor")]
    public class AssignmentController : Controller
    {
        private readonly IAssignmentRepository _assignmentRepo;
        private readonly ISubjectRepository _subjectRepo;
        // Student and Submission repos removed as they are now handled by GradingService
        private readonly IGradingService _gradingService;
        private readonly ISubmissionRepository _submissionRepo; // Kept for Download
        private readonly IWebHostEnvironment _env;

        public AssignmentController(IAssignmentRepository assignmentRepo,
                                    ISubjectRepository subjectRepo,
                                    ISubmissionRepository submissionRepo,
                                    IGradingService gradingService,
                                    IWebHostEnvironment env)
        {
            _assignmentRepo = assignmentRepo;
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
                return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity.Name;
            }
        }

        public IActionResult Index()
        {
            var teacherId = CurrentTeacherId;
            var assignments = _assignmentRepo.GetAll()
                                .Where(a => a.TeacherId == teacherId)
                                .ToList();
            return View(assignments);
        }

        public IActionResult Create()
        {
            ViewBag.Subjects = _subjectRepo.GetAll();
            return View();
        }

        [HttpPost]
        public IActionResult Create(AssignmentModel model)
        {
            ModelState.Remove("TeacherId");

            // Remove AssignmentId from validation so we can generate it ourselves
            ModelState.Remove("AssignmentId");

            if (ModelState.IsValid)
            {
                // --- NEW ID GENERATION LOGIC START ---

                // 1. Get all existing assignments
                var allAssignments = _assignmentRepo.GetAll();

                // 2. Find the highest existing "ASM-XXX" number
                int nextIdNumber = 1;
                foreach (var assign in allAssignments)
                {
                    // Check if ID starts with "ASM-"
                    if (!string.IsNullOrEmpty(assign.AssignmentId) && assign.AssignmentId.StartsWith("ASM-"))
                    {
                        // Extract the number part (e.g., "ASM-005" -> "005")
                        string numberPart = assign.AssignmentId.Substring(4);
                        if (int.TryParse(numberPart, out int currentNum))
                        {
                            if (currentNum >= nextIdNumber)
                            {
                                nextIdNumber = currentNum + 1;
                            }
                        }
                    }
                }

                // 3. Create the new ID (e.g., ASM-001, ASM-002, etc.)
                model.AssignmentId = $"ASM-{nextIdNumber:D3}";

                // --- NEW ID GENERATION LOGIC END ---

                model.TeacherId = CurrentTeacherId;
                _assignmentRepo.Add(model);
                return RedirectToAction("Index");
            }

            ViewBag.Subjects = _subjectRepo.GetAll();
            return View(model);
        }

        public IActionResult Edit(string id)
        {
            var assignment = _assignmentRepo.GetById(id);
            if (assignment == null || assignment.TeacherId != CurrentTeacherId)
            {
                return RedirectToAction("AccessDenied", "Login", new { area = "" });
            }

            ViewBag.Subjects = _subjectRepo.GetAll();
            return View(assignment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(AssignmentModel model)
        {
            ModelState.Remove("TeacherId");
            var existing = _assignmentRepo.GetById(model.AssignmentId);
            if (existing == null || existing.TeacherId != CurrentTeacherId)
            {
                return RedirectToAction("AccessDenied", "Login", new { area = "" });
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Subjects = _subjectRepo.GetAll();
                return View(model);
            }

            model.TeacherId = existing.TeacherId; // preserve ownership
            model.AreGradesPublished = existing.AreGradesPublished; // do not overwrite publish status from edit form
            _assignmentRepo.Update(model);
            return RedirectToAction("Index");
        }

        // [REFACTORED] Much thinner Grade action
        [HttpGet]
        public IActionResult Grade(string id)
        {
            // Logic moved to GradingService.PrepareGradingView
            var model = _gradingService.PrepareGradingView(id, CurrentTeacherId);

            if (model == null)
            {
                // If null, it means assignment not found OR unauthorized
                return RedirectToAction("AccessDenied", "Login", new { area = "" });
            }

            return View(model);
        }

        [HttpPost]
        public IActionResult Grade(BulkGradeViewModel model)
        {
            // Verify ownership again before saving
            var assignment = _assignmentRepo.GetById(model.AssignmentId);
            if (assignment == null || assignment.TeacherId != CurrentTeacherId)
            {
                return RedirectToAction("AccessDenied", "Login", new { area = "" });
            }

            if (!ModelState.IsValid) return View(model);

            _gradingService.ProcessGrades(model);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult DownloadSubmission(string assignmentId, string studentId)
        {
            var assignment = _assignmentRepo.GetById(assignmentId);
            if (assignment == null || assignment.TeacherId != CurrentTeacherId)
            {
                return RedirectToAction("AccessDenied", "Login", new { area = "" });
            }

            var submission = _submissionRepo.GetByStudentAndAssignment(studentId, assignmentId);
            if (submission == null || string.IsNullOrEmpty(submission.FilePath))
            {
                return NotFound("Submission file not found.");
            }

            var relativePath = submission.FilePath.TrimStart('/', '\\');
            var filePath = Path.Combine(_env.WebRootPath, relativePath);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound($"File not found on server: {submission.FilePath}");
            }

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            var fileName = Path.GetFileName(filePath);
            return File(fileBytes, "application/octet-stream", fileName);
        }

        [HttpGet]
        public IActionResult PreviewSubmission(string assignmentId, string studentId)
        {
            var assignment = _assignmentRepo.GetById(assignmentId);
            if (assignment == null || assignment.TeacherId != CurrentTeacherId)
            {
                return RedirectToAction("AccessDenied", "Login", new { area = "" });
            }

            var submission = _submissionRepo.GetByStudentAndAssignment(studentId, assignmentId);
            if (submission == null || string.IsNullOrEmpty(submission.FilePath))
            {
                return NotFound("Submission file not found.");
            }

            var relativePath = submission.FilePath.TrimStart('/', '\\');
            var filePath = Path.Combine(_env.WebRootPath, relativePath);
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("File not found on server.");
            }

            var ext = Path.GetExtension(filePath).ToLowerInvariant();
            var contentType = ext switch
            {
                ".pdf" => "application/pdf",
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".txt" => "text/plain",
                _ => "application/octet-stream"
            };

            var stream = System.IO.File.OpenRead(filePath);
            return File(stream, contentType);
        }
    }
}