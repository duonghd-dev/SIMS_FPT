using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        private readonly IClassRepository _classRepo;
        private readonly IClassSubjectRepository _classSubjectRepo;
        private readonly IGradingService _gradingService;
        private readonly ISubmissionRepository _submissionRepo;
        private readonly IWebHostEnvironment _env;

        public AssignmentController(IAssignmentRepository assignmentRepo,
                                    ISubjectRepository subjectRepo,
                                    IClassRepository classRepo,
                                    IClassSubjectRepository classSubjectRepo,
                                    ISubmissionRepository submissionRepo,
                                    IGradingService gradingService,
                                    IWebHostEnvironment env)
        {
            _assignmentRepo = assignmentRepo;
            _subjectRepo = subjectRepo;
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
            // Get classes where the teacher is assigned via ClassSubject table
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
            var teacherId = CurrentTeacherId;
            var assignments = _assignmentRepo.GetAll()
                                .Where(a => a.TeacherId == teacherId)
                                .ToList();
            return View(assignments);
        }

        public IActionResult Create()
        {
            LoadTeacherClasses();
            return View();
        }

        [HttpPost]
        public IActionResult Create(AssignmentModel model)
        {
            // Remove fields we auto-generate or don't need from user input
            ModelState.Remove("TeacherId");
            ModelState.Remove("AssignmentId");
            ModelState.Remove("SubjectId");

            // 1. Validate that the teacher owns this class
            var teacherId = CurrentTeacherId;
            var targetClass = model.ClassId != null ? _classRepo.GetById(model.ClassId) : null;
            var teachesThisClass = model.ClassId != null && _classSubjectRepo.GetAll()
                .Any(cs => cs.ClassId == model.ClassId && cs.TeacherId == teacherId);

            if (targetClass == null || !teachesThisClass)
            {
                ModelState.AddModelError("ClassId", "Invalid class selected or you are not the teacher.");
            }

            if (ModelState.IsValid)
            {
                // 2. Generate ID Logic
                var allAssignments = _assignmentRepo.GetAll();
                int nextIdNumber = 1;
                foreach (var assign in allAssignments)
                {
                    if (!string.IsNullOrEmpty(assign.AssignmentId) && assign.AssignmentId.StartsWith("ASM-"))
                    {
                        string numberPart = assign.AssignmentId.Substring(4);
                        if (int.TryParse(numberPart, out int currentNum))
                        {
                            if (currentNum >= nextIdNumber) nextIdNumber = currentNum + 1;
                        }
                    }
                }
                model.AssignmentId = $"ASM-{nextIdNumber:D3}";

                // 3. Auto-fill Data
                model.TeacherId = teacherId;
                // Get first subject from this class for the assignment
                var firstSubject = _classSubjectRepo.GetByClassId(model.ClassId!).FirstOrDefault();
                model.SubjectId = firstSubject?.SubjectId ?? "";

                _assignmentRepo.Add(model);
                return RedirectToAction("Index");
            }

            LoadTeacherClasses(model.ClassId ?? ""); // Reload dropdown if validation fails
            return View(model);
        }

        public IActionResult Edit(string id)
        {
            var assignment = _assignmentRepo.GetById(id);
            if (assignment == null || assignment.TeacherId != CurrentTeacherId)
            {
                return RedirectToAction("AccessDenied", "Login", new { area = "" });
            }
            LoadTeacherClasses(assignment.ClassId ?? "");
            return View(assignment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(AssignmentModel model)
        {
            ModelState.Remove("TeacherId");
            ModelState.Remove("SubjectId");

            var existing = _assignmentRepo.GetById(model.AssignmentId);
            if (existing == null)
            {
                return RedirectToAction("AccessDenied", "Login", new { area = "" });
            }
            if (existing.TeacherId != null && existing.TeacherId != CurrentTeacherId)
            {
                return RedirectToAction("AccessDenied", "Login", new { area = "" });
            }

            // Verify class ownership again
            var targetClass = model.ClassId != null ? _classRepo.GetById(model.ClassId) : null;
            var teachesClass = model.ClassId != null && _classSubjectRepo.GetAll()
                .Any(cs => cs.ClassId == model.ClassId && cs.TeacherId == CurrentTeacherId);

            if (targetClass == null || !teachesClass)
            {
                ModelState.AddModelError("ClassId", "Invalid class selected.");
            }

            if (!ModelState.IsValid)
            {
                LoadTeacherClasses(model.ClassId ?? "");
                return View(model);
            }

            // Preserve and Update
            model.TeacherId = existing.TeacherId ?? CurrentTeacherId;
            model.AreGradesPublished = existing.AreGradesPublished;
            var firstSubject = _classSubjectRepo.GetByClassId(model.ClassId!).FirstOrDefault();
            model.SubjectId = firstSubject?.SubjectId ?? existing.SubjectId ?? "";

            _assignmentRepo.Update(model);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Grade(string id)
        {
            var model = _gradingService.PrepareGradingView(id, CurrentTeacherId);
            if (model == null)
            {
                return RedirectToAction("AccessDenied", "Login", new { area = "" });
            }
            return View(model);
        }

        [HttpPost]
        public IActionResult Grade(BulkGradeViewModel model)
        {
            var assignment = _assignmentRepo.GetById(model.AssignmentId);
            if (assignment == null || assignment.TeacherId != CurrentTeacherId)
            {
                return RedirectToAction("AccessDenied", "Login", new { area = "" });
            }


            _gradingService.ProcessGrades(model);
            return RedirectToAction("Index");
        }

        [HttpGet]
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
                return NotFound($"File not found on server.");
            }

            // Use FileStream for efficiency with potentially large files
            var stream = System.IO.File.OpenRead(filePath);
            var fileName = Path.GetFileName(filePath);

            // Return as a download attachment
            return File(stream, "application/octet-stream", fileName);
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
            var assignment = _assignmentRepo.GetById(id);
            if (assignment == null || assignment.TeacherId != CurrentTeacherId)
            {
                return RedirectToAction("AccessDenied", "Login", new { area = "" });
            }

            _assignmentRepo.Delete(id);
            TempData["Success"] = "Assignment deleted successfully.";
            return RedirectToAction("Index");
        }
    }
}