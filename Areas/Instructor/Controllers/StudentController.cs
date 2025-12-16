using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Models.ViewModels;
using System.Linq;
using System.Security.Claims;

namespace SIMS_FPT.Areas.Instructor.Controllers
{
    [Area("Instructor")]
    [Authorize]
    public class StudentController : Controller
    {
        private readonly IStudentRepository _studentRepo;
        private readonly IAssignmentRepository _assignmentRepo;
        private readonly ISubmissionRepository _submissionRepo;

        public StudentController(IStudentRepository studentRepo,
                                 IAssignmentRepository assignmentRepo,
                                 ISubmissionRepository submissionRepo)
        {
            _studentRepo = studentRepo;
            _assignmentRepo = assignmentRepo;
            _submissionRepo = submissionRepo;
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

        public IActionResult Profile(string id)
        {
            var student = _studentRepo.GetById(id);
            if (student == null) return NotFound();

            var teacherId = CurrentTeacherId;
            var teacherAssignments = _assignmentRepo.GetAll().Where(a => a.TeacherId == teacherId).ToList();
            if (!teacherAssignments.Any())
            {
                return RedirectToAction("AccessDenied", "Login", new { area = "" });
            }

            var history = teacherAssignments.Select(a =>
            {
                var sub = _submissionRepo.GetByStudentAndAssignment(student.StudentId ?? "", a.AssignmentId ?? "");
                return new AssignmentHistoryItem
                {
                    AssignmentId = a.AssignmentId ?? "",
                    AssignmentTitle = a.Title ?? "Untitled",
                    SubjectId = a.SubjectId ?? "",
                    Grade = sub?.Grade ?? null,
                    MaxPoints = a.MaxPoints,
                    TeacherComments = sub?.TeacherComments ?? ""
                };
            }).ToList();

            double totalScore = history.Where(h => h.Grade.HasValue).Sum(h => h.Grade!.Value);
            double totalMax = history.Sum(h => h.MaxPoints);
            double avgPercent = totalMax > 0 ? (totalScore / totalMax) * 100 : 0;

            var vm = new StudentProfileViewModel
            {
                Student = student,
                AssignmentHistory = history,
                AverageScorePercent = avgPercent
            };

            return View(vm);
        }
    }
}


