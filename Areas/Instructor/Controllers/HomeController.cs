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
            var teacherId = CurrentTeacherId;

            // Set ViewBag for header avatar
            var teacher = _teacherRepo.GetById(teacherId);
            if (teacher != null)
            {
                ViewBag.ProfileImage = teacher.ImagePath;
            }

            // 1. Pull assignments owned by this instructor
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

            // --- NEW CODE: Populate Teaching Classes List ---
            var myTeachingClasses = new List<TeachingClassInfo>();
            foreach (var cls in teacherClasses)
            {
                // Assuming cls.SubjectName holds the Subject ID (e.g., SUB001)
                var subject = _subjectRepo.GetById(cls.SubjectName);

                myTeachingClasses.Add(new TeachingClassInfo
                {
                    ClassId = cls.ClassId,
                    ClassName = cls.ClassName,
                    SubjectCode = subject?.SubjectId ?? cls.SubjectName,
                    SubjectName = subject?.SubjectName ?? "Unknown Subject",
                    StudentCount = cls.NumberOfStudents,
                    Semester = cls.Semester
                });
            }
            // ------------------------------------------------

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

            // Init Model
            var model = new InstructorDashboardViewModel
            {
                TodayClasses = BuildTodayClasses(),
                TeachingClasses = myTeachingClasses, // <--- Assign the list here
                LeaveDaysRemaining = 12,
                LastSalaryMonth = "November 2025",
                StudentOptions = students.Select(s => new StudentOption { StudentId = s.StudentId, StudentName = s.FullName }).ToList(),
                SelectedStudentId = selectedStudent?.StudentId,
                SelectedStudentName = selectedStudent?.FullName ?? "No students"
            };
            var allActivities = new List<RecentActivityItem>();

            if (teacherAssignments.Any())
            {
                foreach (var assn in teacherAssignments)
                {
                    // ... (Existing Chart Logic: PerformanceLabels, Counts, etc.) ...

                    // Fetch submissions for this assignment
                    var submissions = _submissionRepo.GetByAssignmentId(assn.AssignmentId);

                    // ... (Existing Total counts logic) ...

                    // [NEW] Collect submissions for the feed
                    foreach (var sub in submissions)
                    {
                        var student = _studentRepo.GetById(sub.StudentId);
                        if (student != null)
                        {
                            allActivities.Add(new RecentActivityItem
                            {
                                StudentName = student.FullName,
                                StudentId = sub.StudentId,
                                AssignmentTitle = assn.Title,
                                SubmissionDate = sub.SubmissionDate,
                                TimeAgo = CalculateTimeAgo(sub.SubmissionDate)
                            });
                        }
                    }
                }
            }
            model.RecentActivities = allActivities
           .OrderByDescending(x => x.SubmissionDate)
           .Take(5)
           .ToList();
            int totalSubmissions = 0;
            int totalGraded = 0;

            // ... (Rest of the existing logic for Charts, Activity, AtRisk remains the same) ...

            if (teacherAssignments.Any())
            {
                foreach (var assn in teacherAssignments)
                {
                    var submissions = _submissionRepo.GetByAssignmentId(assn.AssignmentId);
                    foreach (var sub in submissions)
                    {
                        var student = _studentRepo.GetById(sub.StudentId);
                        if (student != null)
                        {
                            allActivities.Add(new RecentActivityItem
                            {
                                StudentName = student.FullName,
                                StudentId = sub.StudentId,
                                AssignmentTitle = assn.Title,
                                SubmissionDate = sub.SubmissionDate,
                                TimeAgo = CalculateTimeAgo(sub.SubmissionDate)
                            });
                        }
                    }
                }
            }
            model.RecentActivities = allActivities.OrderByDescending(x => x.SubmissionDate).Take(5).ToList();



            if (teacherAssignments.Any())
            {
                foreach (var assn in teacherAssignments)
                {
                    model.PerformanceLabels.Add(assn.Title);
                    var submissions = _submissionRepo.GetByAssignmentId(assn.AssignmentId);

                    totalSubmissions += submissions.Count;
                    totalGraded += submissions.Count(s => s.Grade.HasValue);
                    model.SubmissionCounts.Add(submissions.Count);
                    model.GradedCounts.Add(submissions.Count(s => s.Grade.HasValue));

                    double maxPoints = assn.MaxPoints > 0 ? assn.MaxPoints : 1;
                    var graded = submissions.Where(s => s.Grade.HasValue).ToList();
                    double classAvg = graded.Any() ? graded.Average(s => s.Grade.GetValueOrDefault(0)) : 0;

                    model.ClassAverageSeries.Add(Math.Round((classAvg / maxPoints) * 100, 2));

                    if (selectedStudent != null)
                    {
                        var studentSub = submissions.FirstOrDefault(s => s.StudentId == selectedStudent.StudentId);
                        double studentScore = studentSub?.Grade ?? 0;
                        model.StudentSeries.Add(Math.Round((studentScore / maxPoints) * 100, 2));
                    }
                    else
                    {
                        model.StudentSeries.Add(0);
                    }
                }
            }
            else
            {
                model.PerformanceLabels.Add("No assignments");
                model.ClassAverageSeries.Add(0);
                model.StudentSeries.Add(0);
            }

            model.TotalSubmissions = totalSubmissions;
            model.TotalGraded = totalGraded;

            // At Risk Logic
            model.AtRiskStudents = students.Select(s =>
            {
                double totalScore = 0;
                double totalMax = 0;
                int missingCount = 0;
                foreach (var assn in teacherAssignments)
                {
                    totalMax += assn.MaxPoints;
                    var sub = _submissionRepo.GetByStudentAndAssignment(s.StudentId, assn.AssignmentId);
                    if (sub?.Grade != null) totalScore += sub.Grade.Value;
                    else missingCount++;
                }
                double percent = totalMax > 0 ? (totalScore / totalMax) * 100 : 0;
                var atRisk = percent < 50 || missingCount > Math.Max(1, teacherAssignments.Count / 2);
                return new AtRiskStudent
                {
                    StudentId = s.StudentId,
                    StudentName = s.FullName,
                    RiskLevel = percent < 35 ? "High" : "Medium",
                    Reason = atRisk ? $"Avg {(int)percent}% | Missing {missingCount}" : null
                };
            }).Where(x => x.Reason != null).OrderByDescending(x => x.RiskLevel).ToList();

            return View(model);
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

            // Set ViewBag for header avatar
            ViewBag.ProfileImage = teacher.ImagePath;

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

            // Set ViewBag for header avatar
            ViewBag.ProfileImage = teacher.ImagePath;

            return View(model);
        }
        private string CalculateTimeAgo(DateTime date)
        {
            var span = DateTime.Now - date;
            if (span.TotalMinutes < 60)
                return $"{(int)span.TotalMinutes} mins ago";
            if (span.TotalHours < 24)
                return $"{(int)span.TotalHours} hours ago";
            return $"{(int)span.TotalDays} days ago";
        }
        private List<ClassScheduleItem> BuildTodayClasses()
        {
            // Fetch a few subjects to simulate a schedule
            var subjects = _subjectRepo.GetAll().Take(3).ToList();

            return subjects.Select((s, index) => new ClassScheduleItem
            {
                SubjectName = s.SubjectName,
                Time = $"{8 + (index * 2)}:00 AM - {10 + (index * 2)}:00 AM",
                Room = $"Room A-{101 + index}",
                ClassName = s.SubjectId
            }).ToList();
        }
    }
}
