using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Models;
using SIMS_FPT.Models.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;

namespace SIMS_FPT.Areas.Student.Controllers
{
    [Area("Student")]
    public class AssignmentController : Controller
    {
        private readonly IAssignmentRepository _assignmentRepo;
        private readonly ISubmissionRepository _submissionRepo;
        private readonly IWebHostEnvironment _env;

        public AssignmentController(IAssignmentRepository assignmentRepo,
                                    ISubmissionRepository submissionRepo,
                                    IWebHostEnvironment env)
        {
            _assignmentRepo = assignmentRepo;
            _submissionRepo = submissionRepo;
            _env = env;
        }

        private string CurrentStudentId
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
            var studentId = CurrentStudentId;
            var assignments = _assignmentRepo.GetAll();
            var viewModel = assignments
                .Select(a => new StudentAssignmentViewModel
                {
                    Assignment = a,
                    Submission = _submissionRepo.GetByStudentAndAssignment(studentId, a.AssignmentId)
                })
                .OrderByDescending(x => x.Assignment.DueDate)
                .ToList();

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Submit(string id)
        {
            var assignment = _assignmentRepo.GetById(id);
            if (assignment == null) return NotFound();

            var studentId = CurrentStudentId;
            var submission = _submissionRepo.GetByStudentAndAssignment(studentId, id);

            var vm = new StudentAssignmentViewModel
            {
                Assignment = assignment,
                Submission = submission
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Submit(string id, IFormFile uploadFile)
        {
            var assignment = _assignmentRepo.GetById(id);
            if (assignment == null) return NotFound();

            if (uploadFile == null || uploadFile.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "Please select a file to upload.");
            }
            else
            {
                // Validate allowed file types based on assignment settings
                var ext = Path.GetExtension(uploadFile.FileName)?.ToLowerInvariant();
                var allowed = assignment.AllowedFileTypes;
                if (!string.IsNullOrWhiteSpace(allowed) && allowed.Trim() != "*")
                {
                    var allowedList = allowed
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Trim().ToLowerInvariant())
                        .ToList();

                    if (ext == null || !allowedList.Contains(ext))
                    {
                        ModelState.AddModelError(string.Empty, $"File type {ext} is not allowed for this assignment.");
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                var fallbackVm = new StudentAssignmentViewModel
                {
                    Assignment = assignment,
                    Submission = _submissionRepo.GetByStudentAndAssignment(CurrentStudentId, id)
                };
                return View(fallbackVm);
            }

            var studentId = CurrentStudentId;
            var uploadsFolder = Path.Combine(_env.WebRootPath, "submissions", assignment.AssignmentId);
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            var extension = Path.GetExtension(uploadFile.FileName);
            var fileName = $"{studentId}_{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            // Remove previous submission file if it exists
            var existing = _submissionRepo.GetByStudentAndAssignment(studentId, id);
            if (existing != null && !string.IsNullOrEmpty(existing.FilePath))
            {
                var existingPhysical = Path.Combine(_env.WebRootPath, existing.FilePath.TrimStart('/', '\\'));
                if (System.IO.File.Exists(existingPhysical))
                {
                    System.IO.File.Delete(existingPhysical);
                }
            }

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                uploadFile.CopyTo(stream);
            }

            var submission = existing ?? new SubmissionModel { AssignmentId = id, StudentId = studentId };
            submission.FilePath = Path.Combine("submissions", assignment.AssignmentId, fileName).Replace("\\", "/");
            submission.SubmissionDate = DateTime.Now;

            _submissionRepo.SaveSubmission(submission);

            TempData["Success"] = "Submission uploaded successfully.";
            return RedirectToAction("Index");
        }
    }
}

