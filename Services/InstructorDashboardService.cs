using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Models;
using SIMS_FPT.Models.ViewModels;
using SIMS_FPT.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SIMS_FPT.Services
{
    public class InstructorDashboardService : IInstructorDashboardService
    {
        private readonly ITeacherRepository _teacherRepo;
        private readonly IAssignmentRepository _assignmentRepo;
        private readonly ISubmissionRepository _submissionRepo;
        private readonly IStudentRepository _studentRepo;
        private readonly IClassRepository _classRepo;
        private readonly IStudentClassRepository _studentClassRepo;
        private readonly IClassSubjectRepository _classSubjectRepo;
        private readonly ISubjectRepository _subjectRepo;

        public InstructorDashboardService(
            ITeacherRepository teacherRepo,
            IAssignmentRepository assignmentRepo,
            ISubmissionRepository submissionRepo,
            IStudentRepository studentRepo,
            IClassRepository classRepo,
            IStudentClassRepository studentClassRepo,
            IClassSubjectRepository classSubjectRepo,
            ISubjectRepository subjectRepo)
        {
            _teacherRepo = teacherRepo;
            _assignmentRepo = assignmentRepo;
            _submissionRepo = submissionRepo;
            _studentRepo = studentRepo;
            _classRepo = classRepo;
            _studentClassRepo = studentClassRepo;
            _classSubjectRepo = classSubjectRepo;
            _subjectRepo = subjectRepo;
        }

        public InstructorDashboardViewModel GetDashboard(string teacherId, string? studentId = null)
        {
            var model = new InstructorDashboardViewModel();

            // Get teacher info for avatar
            var teacher = _teacherRepo.GetById(teacherId);

            // Get assignments owned by instructor
            var teacherAssignments = _assignmentRepo.GetAll()
                .Where(a => a.TeacherId.Equals(teacherId, StringComparison.OrdinalIgnoreCase))
                .OrderBy(a => a.DueDate)
                .ToList();

            // Get classes where teacher teaches
            var teacherClassIds = _classSubjectRepo.GetAll()
                .Where(cs => cs.TeacherId.Equals(teacherId, StringComparison.OrdinalIgnoreCase))
                .Select(cs => cs.ClassId)
                .Distinct()
                .ToList();

            var teacherClasses = _classRepo.GetAll()
                .Where(c => teacherClassIds.Contains(c.ClassId))
                .ToList();

            // Build teaching classes list
            var myTeachingClasses = new List<TeachingClassInfo>();
            foreach (var cls in teacherClasses)
            {
                var classSubjects = _classSubjectRepo.GetByClassId(cls.ClassId ?? string.Empty);
                var subjectNames = classSubjects
                    .Select(cs => _subjectRepo.GetById(cs.SubjectId ?? string.Empty)?.SubjectName ?? cs.SubjectId)
                    .ToList();

                myTeachingClasses.Add(new TeachingClassInfo
                {
                    ClassId = cls.ClassId ?? "N/A",
                    ClassName = cls.ClassName ?? "Unknown",
                    SubjectCode = classSubjects.FirstOrDefault()?.SubjectId ?? "N/A",
                    SubjectName = string.Join(", ", subjectNames),
                    StudentCount = cls.NumberOfStudents,
                    Semester = cls.Semester ?? "N/A"
                });
            }

            // Get students enrolled in teacher's classes
            var students = new List<StudentCSVModel>();
            var enrolledIds = new HashSet<string>();

            foreach (var cls in teacherClasses)
            {
                var enrollments = _studentClassRepo.GetByClassId(cls.ClassId ?? string.Empty);
                foreach (var enr in enrollments)
                {
                    if (!enrolledIds.Contains(enr.StudentId ?? string.Empty))
                    {
                        var student = _studentRepo.GetById(enr.StudentId ?? string.Empty);
                        if (student != null)
                        {
                            students.Add(student);
                            enrolledIds.Add(enr.StudentId ?? string.Empty);
                        }
                    }
                }
            }

            // Select student
            var selectedStudent = !string.IsNullOrEmpty(studentId)
                ? students.FirstOrDefault(s => s.StudentId.Equals(studentId, StringComparison.OrdinalIgnoreCase))
                : students.FirstOrDefault();

            if (selectedStudent == null && students.Any())
            {
                selectedStudent = students.First();
            }

            // Build model
            model.TodayClasses = new List<ClassScheduleItem>(); // Placeholder - can be populated from real schedule data
            model.TeachingClasses = myTeachingClasses;
            model.LeaveDaysRemaining = 12;
            model.LastSalaryMonth = "November 2025";
            model.StudentOptions = students.Select(s => new StudentOption
            {
                StudentId = s.StudentId ?? "",
                StudentName = s.FullName ?? "Unknown"
            }).ToList();
            model.SelectedStudentId = selectedStudent?.StudentId ?? "";
            model.SelectedStudentName = selectedStudent?.FullName ?? "No students";

            // Collect recent activities
            var allActivities = new List<RecentActivityItem>();
            foreach (var assn in teacherAssignments)
            {
                var submissions = _submissionRepo.GetByAssignmentId(assn.AssignmentId ?? string.Empty);
                foreach (var sub in submissions)
                {
                    var student = _studentRepo.GetById(sub.StudentId ?? string.Empty);
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

            model.RecentActivities = allActivities
                .OrderByDescending(x => x.SubmissionDate)
                .Take(5)
                .ToList();

            // Populate chart data
            PopulateChartData(model, teacherAssignments, selectedStudent);

            // Identify at-risk students
            model.AtRiskStudents = IdentifyAtRiskStudents(students, teacherAssignments);

            return model;
        }

        private List<AtRiskStudent> IdentifyAtRiskStudents(List<StudentCSVModel> students, List<AssignmentModel> assignments)
        {
            var atRiskList = new List<AtRiskStudent>();

            foreach (var student in students)
            {
                var studentSubmissions = new List<SubmissionModel>();

                // Get all submissions for this student across all teacher's assignments
                foreach (var assn in assignments)
                {
                    var subs = _submissionRepo.GetByAssignmentId(assn.AssignmentId ?? string.Empty);
                    var studentSub = subs.FirstOrDefault(s => s.StudentId.Equals(student.StudentId, StringComparison.OrdinalIgnoreCase));
                    if (studentSub != null)
                    {
                        studentSubmissions.Add(studentSub);
                    }
                }

                // Calculate metrics
                int totalAssignments = assignments.Count;
                int submittedCount = studentSubmissions.Count;
                int gradedCount = studentSubmissions.Count(s => s.Grade.HasValue);

                // Calculate average grade
                double avgGrade = 0;
                if (gradedCount > 0)
                {
                    avgGrade = studentSubmissions.Where(s => s.Grade.HasValue).Average(s => s.Grade ?? 0);
                }

                // Identify risk factors
                string riskReason = "";
                string riskLevel = "";

                // High Risk: Missing more than 30% of assignments
                if (totalAssignments > 0 && submittedCount < totalAssignments * 0.7)
                {
                    riskReason = $"Missing {totalAssignments - submittedCount}/{totalAssignments} assignments";
                    riskLevel = "High";
                }
                // High Risk: Average grade below 50
                else if (gradedCount > 0 && avgGrade < 50)
                {
                    riskReason = $"Low average grade ({avgGrade:F1}/100)";
                    riskLevel = "High";
                }
                // Medium Risk: Average grade between 50-60
                else if (gradedCount > 0 && avgGrade < 60)
                {
                    riskReason = $"Below average performance ({avgGrade:F1}/100)";
                    riskLevel = "Medium";
                }

                // Add to at-risk list if any risk detected
                if (!string.IsNullOrEmpty(riskReason))
                {
                    atRiskList.Add(new AtRiskStudent
                    {
                        StudentId = student.StudentId ?? "",
                        StudentName = student.FullName ?? "Unknown",
                        Reason = riskReason,
                        RiskLevel = riskLevel
                    });
                }
            }

            // Return top 5 highest risk students
            return atRiskList
                .OrderBy(s => s.RiskLevel == "High" ? 0 : 1)
                .Take(5)
                .ToList();
        }

        private void PopulateChartData(InstructorDashboardViewModel model, List<AssignmentModel> assignments, StudentCSVModel? selectedStudent)
        {
            // 1. Performance Chart Data
            if (selectedStudent != null && assignments.Any())
            {
                var recentAssignments = assignments
                    .OrderByDescending(a => a.DueDate)
                    .Take(5)
                    .OrderBy(a => a.DueDate)
                    .ToList();

                foreach (var assn in recentAssignments)
                {
                    model.PerformanceLabels.Add(assn.Title.Length > 15 ? assn.Title.Substring(0, 15) + "..." : assn.Title);

                    // Get all submissions for this assignment
                    var allSubs = _submissionRepo.GetByAssignmentId(assn.AssignmentId ?? string.Empty);

                    // Calculate class average
                    var gradedSubs = allSubs.Where(s => s.Grade.HasValue).ToList();
                    double classAvg = gradedSubs.Any() ? gradedSubs.Average(s => s.Grade ?? 0) : 0;
                    model.ClassAverageSeries.Add(Math.Round(classAvg, 1));

                    // Get selected student's grade
                    var studentSub = allSubs.FirstOrDefault(s => s.StudentId.Equals(selectedStudent.StudentId, StringComparison.OrdinalIgnoreCase));
                    double studentGrade = studentSub?.Grade ?? 0;
                    model.StudentSeries.Add(Math.Round(studentGrade, 1));
                }
            }
            else
            {
                // Default empty data if no student or assignments
                model.PerformanceLabels = new List<string> { "No Data" };
                model.ClassAverageSeries = new List<double> { 0 };
                model.StudentSeries = new List<double> { 0 };
            }

            // 2. Submission Chart Data
            var totalSubmissions = 0;
            var totalGraded = 0;

            foreach (var assn in assignments)
            {
                var subs = _submissionRepo.GetByAssignmentId(assn.AssignmentId ?? string.Empty);
                totalSubmissions += subs.Count;
                totalGraded += subs.Count(s => s.Grade.HasValue);
            }

            model.TotalSubmissions = totalSubmissions;
            model.TotalGraded = totalGraded;
        }

        private string CalculateTimeAgo(DateTime? date)
        {
            if (!date.HasValue) return "Unknown";

            var now = DateTime.Now;
            var diff = now - date.Value;

            if (diff.TotalSeconds < 60) return "just now";
            if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes}m ago";
            if (diff.TotalHours < 24) return $"{(int)diff.TotalHours}h ago";
            if (diff.TotalDays < 7) return $"{(int)diff.TotalDays}d ago";

            return date.Value.ToString("MMM d, yyyy");
        }
    }
}
