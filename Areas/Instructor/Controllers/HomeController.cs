using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Helpers;
using SIMS_FPT.Models;
using SIMS_FPT.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
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
        private readonly IClassRepository _classRepo;
        private readonly IStudentClassRepository _studentClassRepo;

        // [NEW] Repositories for Profile
        private readonly ITeacherRepository _teacherRepo;
        private readonly IUserRepository _userRepo;
        private readonly IWebHostEnvironment _env;

        public HomeController(ISubjectRepository subjectRepo,
                              IAssignmentRepository assignmentRepo,
                              ISubmissionRepository submissionRepo,
                              IStudentRepository studentRepo,
                              IClassRepository classRepo,
                              IStudentClassRepository studentClassRepo,
                              ITeacherRepository teacherRepo, // Inject
                              IUserRepository userRepo,       // Inject
                              IWebHostEnvironment env)
        {
            _subjectRepo = subjectRepo;
            _assignmentRepo = assignmentRepo;
            _submissionRepo = submissionRepo;
            _studentRepo = studentRepo;
            _classRepo = classRepo;
            _studentClassRepo = studentClassRepo;
            _teacherRepo = teacherRepo;
            _userRepo = userRepo;
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

        public IActionResult Dashboard(string studentId = null)
        {
            // ... (Existing Dashboard Logic - kept same as your previous requests) ...
            var teacherId = CurrentTeacherId;

            // Pull assignments owned by this instructor
            var teacherAssignments = _assignmentRepo.GetAll()
                .Where(a => a.TeacherId == teacherId)
                .OrderBy(a => a.DueDate)
                .ToList();

            // 2. Build student list based on classes taught by this instructor
            var students = new List<Models.StudentCSVModel>();

            // Get all classes where the teacher_id matches the current instructor
            var teacherClasses = _classRepo.GetAll()
                .Where(c => c.TeacherName == teacherId)
                .ToList();

            // Get students enrolled in these specific classes
            var enrolledIds = new HashSet<string>();

            foreach (var cls in teacherClasses)
            {
                var enrollments = _studentClassRepo.GetByClassId(cls.ClassId);
                foreach (var enr in enrollments)
                {
                    if (!enrolledIds.Contains(enr.StudentId))
                    {
                        var student = _studentRepo.GetById(enr.StudentId);
                        if (student != null)
                        {
                            students.Add(student);
                            enrolledIds.Add(enr.StudentId);
                        }
                    }
                }
            }

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

            // [NEW] Variables to hold total counts for the Doughnut Chart
            int totalSubmissions = 0;
            int totalGraded = 0;

            if (teacherAssignments.Any())
            {
                foreach (var assn in teacherAssignments)
                {
                    model.PerformanceLabels.Add(assn.Title);

                    var submissions = _submissionRepo.GetByAssignmentId(assn.AssignmentId);

                    // [NEW] Update total counts
                    totalSubmissions += submissions.Count;
                    totalGraded += submissions.Count(s => s.Grade.HasValue);

                    model.SubmissionCounts.Add(submissions.Count);
                    model.GradedCounts.Add(submissions.Count(s => s.Grade.HasValue));

                    // --- FIX 1: Calculate Percentage Scores for Performance Chart ---
                    // Use 1 to prevent division by zero, though this implies a max point assignment is 1.
                    double maxPoints = assn.MaxPoints > 0 ? assn.MaxPoints : 1;

                    var graded = submissions.Where(s => s.Grade.HasValue).ToList();
                    double classAvg = graded.Any()
                        ? graded.Average(s => s.Grade.GetValueOrDefault(0))
                        : 0;
                    // Store as percentage
                    double classAvgPercentage = (classAvg / maxPoints) * 100;
                    model.ClassAverageSeries.Add(Math.Round(classAvgPercentage, 2));

                    if (selectedStudent != null)
                    {
                        var studentSub = submissions.FirstOrDefault(s => s.StudentId == selectedStudent.StudentId);
                        double studentScore = studentSub?.Grade ?? 0;
                        // Store as percentage
                        double studentPercentage = (studentScore / maxPoints) * 100;
                        model.StudentSeries.Add(Math.Round(studentPercentage, 2));
                    }
                    else
                    {
                        model.StudentSeries.Add(0);
                    }
                }
            }
            else
            {
                model.PerformanceLabels.Add("No assignments yet");
                model.ClassAverageSeries.Add(0);
                model.StudentSeries.Add(0);
                model.SubmissionCounts.Add(0);
                model.GradedCounts.Add(0);
            }

            // [NEW] Assign total counts to model
            model.TotalSubmissions = totalSubmissions;
            model.TotalGraded = totalGraded;

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

        // [NEW] Profile Action (GET)
        [HttpGet]
        public IActionResult Profile()
        {
            var teacherId = CurrentTeacherId;
            var teacher = _teacherRepo.GetById(teacherId);
            if (teacher == null) return NotFound("Teacher profile not found.");

            // Find linked user for login info
            var user = _userRepo.GetByLinkedId(teacherId);

            var model = new InstructorProfileViewModel
            {
                TeacherId = teacher.TeacherId,
                FullName = teacher.Name,
                Email = teacher.Email,
                Mobile = teacher.Mobile,
                Address = teacher.Address,
                City = teacher.City,
                Country = teacher.Country,
                ExistingImagePath = teacher.ImagePath
            };

            return View(model);
        }

        // [NEW] Profile Action (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Profile(InstructorProfileViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var teacherId = CurrentTeacherId;
            if (model.TeacherId != teacherId) return Forbid();

            var teacher = _teacherRepo.GetById(teacherId);
            var user = _userRepo.GetByLinkedId(teacherId);

            if (teacher == null) return NotFound();

            // 1. Update Teacher Info
            teacher.Name = model.FullName;
            teacher.Email = model.Email;
            teacher.Mobile = model.Mobile;
            teacher.Address = model.Address;
            teacher.City = model.City;
            teacher.Country = model.Country;

            // 2. Handle Image Upload
            if (model.ProfileImage != null)
            {
                var folder = Path.Combine(_env.WebRootPath, "assets/img/profiles");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                var fileName = $"{teacherId}_{Guid.NewGuid()}{Path.GetExtension(model.ProfileImage.FileName)}";
                var filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    model.ProfileImage.CopyTo(stream);
                }
                teacher.ImagePath = $"~/assets/img/profiles/{fileName}";
            }

            // 3. Handle Password Change (Sync to Teacher and User CSVs)
            bool passwordChanged = !string.IsNullOrEmpty(model.NewPassword);
            if (passwordChanged)
            {
                // Note: Teachers.csv usually stores plain/simple pass or none, while Users.csv stores Hash.
                // We update both for consistency, though Users.csv is what matters for Login.
                teacher.Password = model.NewPassword;
            }

            _teacherRepo.Update(teacher);

            // 4. Sync to Users.csv
            if (user != null)
            {
                user.FullName = teacher.Name;
                user.Email = teacher.Email;

                if (passwordChanged)
                {
                    user.Password = PasswordHasherHelper.Hash(model.NewPassword);
                    user.HashAlgorithm = "PBKDF2";
                }
                _userRepo.Update(user);
            }

            TempData["Success"] = "Profile updated successfully.";

            // Reload model to show new image/data
            model.ExistingImagePath = teacher.ImagePath;
            return View(model);
        }
    }
}