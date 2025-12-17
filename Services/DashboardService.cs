using SIMS_FPT.Models;
using SIMS_FPT.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SIMS_FPT.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ICsvService _csvService;

        // CSV paths
        private readonly string _studentPath;
        private readonly string _deptPath;
        private readonly string _teacherPath;
        private readonly string _subjectPath;
        private readonly string _classSubjectPath;
        private readonly string _studentClassPath;

        public DashboardService(ICsvService csvService)
        {
            _csvService = csvService;
            var csvDir = Path.Combine(Directory.GetCurrentDirectory(), "CSV_DATA");
            _studentPath = Path.Combine(csvDir, "students.csv");
            _deptPath = Path.Combine(csvDir, "departments.csv");
            _teacherPath = Path.Combine(csvDir, "teachers.csv");
            _subjectPath = Path.Combine(csvDir, "subjects.csv");
            _classSubjectPath = Path.Combine(csvDir, "class_subjects.csv");
            _studentClassPath = Path.Combine(csvDir, "student_classes.csv");
        }

        public DashboardViewModel GetAdminDashboardData()
        {
            var model = new DashboardViewModel();

            // Read CSV
            var students = _csvService.ReadCsv<StudentCSVModel>(_studentPath);
            var departments = _csvService.ReadCsv<DepartmentModel>(_deptPath);
            var teachers = _csvService.ReadCsv<TeacherCSVModel>(_teacherPath);
            var subjects = _csvService.ReadCsv<SubjectModel>(_subjectPath);
            var classSubjects = _csvService.ReadCsv<ClassSubjectModel>(_classSubjectPath);
            var studentClasses = _csvService.ReadCsv<StudentClassModel>(_studentClassPath);

            // Stats
            model.StudentCount = students.Count;
            model.DepartmentCount = departments.Count;
            model.TeacherCount = teachers.Count;

            // No fees → revenue = 0
            model.TotalRevenue = 0;

            // Fake Revenue Chart (optional)
            model.RevenueLabels = new List<string> { "2022", "2023", "2024" };
            model.RevenueData = new List<decimal> { 0, 0, 0 };

            // Students per department (dynamic, synced with enrollments)
            var deptStudentCounts = CalculateDepartmentStudentCounts(subjects, classSubjects, studentClasses);

            // Sync department model counts to calculated totals
            foreach (var dept in departments)
            {
                var key = dept.DepartmentId ?? string.Empty;
                dept.NumberOfStudents = deptStudentCounts.TryGetValue(key, out var count) ? count : 0;
            }

            var departmentStats = deptStudentCounts
                .Select(kvp => new
                {
                    Label = departments
                        .FirstOrDefault(d => string.Equals(d.DepartmentId, kvp.Key, StringComparison.OrdinalIgnoreCase))?
                        .DepartmentName ?? "Unknown",
                    Count = kvp.Value
                })
                .OrderByDescending(d => d.Count)
                .ToList();

            if (departmentStats.Any())
            {
                model.DepartmentChartLabels = departmentStats.Select(d => string.IsNullOrWhiteSpace(d.Label) ? "Unknown" : d.Label).ToList();
                model.DepartmentChartData = departmentStats.Select(d => d.Count).ToList();
            }
            else
            {
                model.DepartmentChartLabels = new List<string> { "No Data" };
                model.DepartmentChartData = new List<int> { 0 };
            }

            // Student Gender Chart
            var genderStat = students
                .GroupBy(s => s.Gender ?? "Unknown")
                .Select(g => new { g.Key, Count = g.Count() })
                .ToList();

            model.StudentClassLabels = genderStat.Select(x => x.Key).ToList();
            model.StudentClassData = genderStat.Select(x => x.Count).ToList();

            // Newest Students
            model.NewestStudents = students
                .OrderByDescending(s => s.StudentId)
                .Take(5)
                .ToList();

            return model;
        }

        /// <summary>
        /// Calculate how many students belong to each department (via class enrollments → class subjects → subjects)
        /// </summary>
        private Dictionary<string, int> CalculateDepartmentStudentCounts(
            List<SubjectModel> subjects,
            List<ClassSubjectModel> classSubjects,
            List<StudentClassModel> studentClasses)
        {
            var subjectDept = subjects
                .Where(s => !string.IsNullOrWhiteSpace(s.SubjectId))
                .GroupBy(s => s.SubjectId!)
                .ToDictionary(
                    g => g.Key,
                    g => g.First().DepartmentId ?? string.Empty,
                    StringComparer.OrdinalIgnoreCase);

            var classDept = classSubjects
                .Where(cs => !string.IsNullOrWhiteSpace(cs.ClassId) && !string.IsNullOrWhiteSpace(cs.SubjectId))
                .GroupBy(cs => cs.ClassId!)
                .ToDictionary(
                    g => g.Key,
                    g => g
                        .Select(cs => cs.SubjectId != null && subjectDept.TryGetValue(cs.SubjectId, out var deptId) ? deptId : string.Empty)
                        .FirstOrDefault(d => !string.IsNullOrWhiteSpace(d)) ?? string.Empty,
                    StringComparer.OrdinalIgnoreCase);

            var enrollmentCounts = studentClasses
                .Where(sc => !string.IsNullOrWhiteSpace(sc.ClassId))
                .GroupBy(sc => sc.ClassId!)
                .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);

            var result = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var kvp in enrollmentCounts)
            {
                var deptId = classDept.TryGetValue(kvp.Key, out var dept) ? dept : string.Empty;
                var targetDept = string.IsNullOrWhiteSpace(deptId) ? "Unknown" : deptId;

                result[targetDept] = result.TryGetValue(targetDept, out var existing)
                    ? existing + kvp.Value
                    : kvp.Value;
            }

            return result;
        }
    }
}
