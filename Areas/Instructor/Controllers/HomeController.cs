using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace SIMS_FPT.Areas.Instructor.Controllers
{
    [Area("Instructor")]
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ISubjectRepository _subjectRepo;
        private readonly IAssignmentRepository _assignmentRepo;
        private readonly ISubmissionRepository _submissionRepo;
        private readonly IStudentRepository _studentRepo;

        public HomeController(ISubjectRepository subjectRepo,
                              IAssignmentRepository assignmentRepo,
                              ISubmissionRepository submissionRepo,
                              IStudentRepository studentRepo)
        {
            _subjectRepo = subjectRepo;
            _assignmentRepo = assignmentRepo;
            _submissionRepo = submissionRepo;
            _studentRepo = studentRepo;
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

        // RENAMED FROM Index TO Dashboard
        public IActionResult Dashboard(string studentId = null)
        {
            var teacherId = CurrentTeacherId;

            // Pull assignments owned by this instructor
            var teacherAssignments = _assignmentRepo.GetAll()
                .Where(a => a.TeacherId == teacherId)
                .OrderBy(a => a.DueDate)
                .ToList();

            // Build student list across subjects taught by this instructor
            var students = new List<Models.StudentCSVModel>();
            foreach (var assn in teacherAssignments)
            {
                var subjectStudents = _studentRepo.GetBySubject(assn.SubjectId);
                if (subjectStudents != null)
                {
                    foreach (var s in subjectStudents)
                    {
                        if (students.All(x => x.StudentId != s.StudentId))
                        {
                            students.Add(s);
                        }
                    }
                }
            }

            // Default selected student
            var selectedStudent = !string.IsNullOrEmpty(studentId)
                ? students.FirstOrDefault(s => s.StudentId == studentId)
                : students.FirstOrDefault();

            if (selectedStudent == null && students.Any())
            {
                selectedStudent = students.First();
            }

            var model = new InstructorDashboardViewModel
            {
                TodayClasses = BuildTodayClasses(),
                LeaveDaysRemaining = 12,
                LastSalaryMonth = "November 2025",
                StudentOptions = students.Select(s => new StudentOption { StudentId = s.StudentId, StudentName = s.FullName }).ToList(),
                SelectedStudentId = selectedStudent?.StudentId,
                SelectedStudentName = selectedStudent?.FullName ?? "No students"
            };

            // Build performance + submissions chart data
            if (teacherAssignments.Any())
            {
                foreach (var assn in teacherAssignments)
                {
                    model.PerformanceLabels.Add(assn.Title);

                    var submissions = _submissionRepo.GetByAssignmentId(assn.AssignmentId);
                    model.SubmissionCounts.Add(submissions.Count);
                    model.GradedCounts.Add(submissions.Count(s => s.Grade.HasValue));

                    var graded = submissions.Where(s => s.Grade.HasValue).ToList();
                    double classAvg = graded.Any()
                        ? graded.Average(s => s.Grade.GetValueOrDefault(0))
                        : 0;
                    model.ClassAverageSeries.Add(Math.Round(classAvg, 2));

                    if (selectedStudent != null)
                    {
                        var studentSub = submissions.FirstOrDefault(s => s.StudentId == selectedStudent.StudentId);
                        model.StudentSeries.Add(Math.Round(studentSub?.Grade ?? 0, 2));
                    }
                    else
                    {
                        model.StudentSeries.Add(0);
                    }
                }
            }
            else
            {
                // Provide safe defaults so charts don't break
                model.PerformanceLabels.Add("No assignments yet");
                model.ClassAverageSeries.Add(0);
                model.StudentSeries.Add(0);
                model.SubmissionCounts.Add(0);
                model.GradedCounts.Add(0);
            }

            // Build at-risk list (avg grade < 50% or missing submissions on more than half assignments)
            model.AtRiskStudents = students.Select(s =>
            {
                double totalScore = 0;
                double totalMax = 0;
                int missingCount = 0;

                foreach (var assn in teacherAssignments)
                {
                    totalMax += assn.MaxPoints;
                    var sub = _submissionRepo.GetByStudentAndAssignment(s.StudentId, assn.AssignmentId);
                    if (sub?.Grade != null)
                    {
                        totalScore += sub.Grade.Value;
                    }
                    else
                    {
                        missingCount++;
                    }
                }

                double percent = totalMax > 0 ? (totalScore / totalMax) * 100 : 0;
                var atRisk = percent < 50 || missingCount > Math.Max(1, teacherAssignments.Count / 2);

                return new AtRiskStudent
                {
                    StudentId = s.StudentId,
                    StudentName = s.FullName,
                    RiskLevel = percent < 35 ? "High" : "Medium",
                    Reason = atRisk
                        ? $"Avg {(int)percent}% | Missing {missingCount} of {teacherAssignments.Count}"
                        : null
                };
            })
            .Where(x => x.Reason != null)
            .OrderByDescending(x => x.RiskLevel)
            .ToList();

            // 1. Fetch Data
            return View(model);
        }

        private List<ClassScheduleItem> BuildTodayClasses()
        {
            var subjects = _subjectRepo.GetAll().Take(3).ToList();
            return subjects.Select((s, index) => new ClassScheduleItem
            {
                SubjectName = s.SubjectName,
                Time = $"{8 + (index * 2)}:00 AM - {10 + (index * 2)}:00 AM",
                Room = $"Room A-{101 + index}",
                ClassName = s.SubjectId
            }).ToList();
        }

        public IActionResult MyPayslips()
        {
            return View();
        }
    }
}