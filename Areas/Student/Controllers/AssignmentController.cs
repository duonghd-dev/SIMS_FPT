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
using System.Collections.Generic;

namespace SIMS_FPT.Areas.Student.Controllers
{
    [Area("Student")]
    // [Authorize(Roles = "Student")]
    public class AssignmentController : Controller
    {
        private readonly IAssignmentRepository _assignmentRepo;
        private readonly ISubmissionRepository _submissionRepo;
        private readonly IClassRepository _classRepo;
        private readonly IStudentClassRepository _studentClassRepo;
        private readonly IWebHostEnvironment _env;

        public AssignmentController(IAssignmentRepository assignmentRepo,
                                    ISubmissionRepository submissionRepo,
                                    IClassRepository classRepo,
                                    IStudentClassRepository studentClassRepo,
                                    IWebHostEnvironment env)
        {
            _assignmentRepo = assignmentRepo;
            _submissionRepo = submissionRepo;
            _classRepo = classRepo;
            _studentClassRepo = studentClassRepo;
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

        // Helper: Check if student is allowed to access this assignment
        private bool IsStudentEligibleForAssignment(string studentId, AssignmentModel assignment)
        {
            if (!string.IsNullOrEmpty(assignment.ClassId))
            {
                return _studentClassRepo.IsEnrolled(assignment.ClassId, studentId);
            }

            // Fallback for old assignments
            var enrollments = _studentClassRepo.GetByStudentId(studentId);
            var enrolledClassIds = enrollments.Select(e => e.ClassId).ToList();

            var studentClasses = _classRepo.GetAll()
                .Where(c => enrolledClassIds.Contains(c.ClassId))
                .ToList();

            return studentClasses.Any(c => c.SubjectName == assignment.SubjectId && c.TeacherName == assignment.TeacherId);
        }

        public IActionResult Index()
        {
            var studentId = CurrentStudentId;

            // 1. Get all assignments
            var allAssignments = _assignmentRepo.GetAll();

            // 2. Filter: Only show assignments for classes the student is enrolled in
            var visibleAssignments = allAssignments
                .Where(a => IsStudentEligibleForAssignment(studentId, a))
                .ToList();

            // 3. ViewModel
            var viewModel = visibleAssignments
                .Select(a => {
                    // Corrected Logic: Use a code block to get ClassName before creating the ViewModel
                    var classInfo = _classRepo.GetById(a.ClassId);
                    var submission = _submissionRepo.GetByStudentAndAssignment(studentId, a.AssignmentId);

                    if (submission != null && !a.AreGradesPublished)
                    {
                        submission.Grade = null;        // Hide the grade
                        submission.TeacherComments = null; // Hide the feedback
                    }

                    return new StudentAssignmentViewModel
                    {
                        Assignment = a,
                        Submission = submission,
                        ClassName = classInfo?.ClassName ?? "Unknown Class"
                    };
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

            if (!IsStudentEligibleForAssignment(CurrentStudentId, assignment))
            {
                return RedirectToAction("AccessDenied", "Login", new { area = "" });
            }

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

            // [SECURITY] Check eligibility again on POST
            if (!IsStudentEligibleForAssignment(CurrentStudentId, assignment))
            {
                return RedirectToAction("AccessDenied", "Login", new { area = "" });
            }

            if (uploadFile == null || uploadFile.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "Please select a file to upload.");
            }
            else
            {
                var ext = Path.GetExtension(uploadFile.FileName)?.ToLowerInvariant();
                var allowed = assignment.AllowedFileTypes;
                if (!string.IsNullOrWhiteSpace(allowed) && allowed.Trim() != "*")
                {
                    var allowedList = allowed
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Trim().ToLowerInvariant())
                        .ToList();

                    // FIX 2: Remove the dot from the extension before comparing it to the allowed list.
                    var cleanExt = ext?.TrimStart('.');
                    if (ext == null || !allowedList.Contains(cleanExt))
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

            // Create structure: submissions/{ClassId}/{AssignmentId}
            var uploadsFolder = Path.Combine(_env.WebRootPath, "submissions", assignment.ClassId, assignment.AssignmentId);
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            var extension = Path.GetExtension(uploadFile.FileName);
            var fileName = $"{studentId}_{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            // Clean up old file if exists
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

            // Save relative path using the new structure
            submission.FilePath = Path.Combine("submissions", assignment.ClassId, assignment.AssignmentId, fileName).Replace("\\", "/");
            submission.SubmissionDate = DateTime.Now;

            // Reset grade on re-submission if desired
            if (existing != null)
            {
                submission.Grade = null;
                submission.TeacherComments = null;
            }

            _submissionRepo.SaveSubmission(submission);

            TempData["Success"] = "Submission uploaded successfully.";
            return RedirectToAction("Index");
        }
    }
}