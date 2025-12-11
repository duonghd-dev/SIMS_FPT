using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Mvc;
using SIMS_FPT.Models;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace SIMS_FPT.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class HomeController : Controller
    {
        // CSV paths
        private readonly string _studentPath = Path.Combine(Directory.GetCurrentDirectory(), "CSV_DATA", "students.csv");
        private readonly string _deptPath = Path.Combine(Directory.GetCurrentDirectory(), "CSV_DATA", "departments.csv");
        private readonly string _teacherPath = Path.Combine(Directory.GetCurrentDirectory(), "CSV_DATA", "teachers.csv");

        public IActionResult Dashboard()
        {
            var model = new DashboardViewModel();

            // Read CSV
            var students = ReadCsv<StudentCSVModel>(_studentPath);
            var departments = ReadCsv<DepartmentModel>(_deptPath);
            var teachers = ReadCsv<TeacherCSVModel>(_teacherPath);

            // Stats
            model.StudentCount = students.Count;
            model.DepartmentCount = departments.Count;
            model.TeacherCount = teachers.Count;

            // No fees → revenue = 0
            model.TotalRevenue = 0;

            // Fake Revenue Chart (optional)
            model.RevenueLabels = new List<string> { "2022", "2023", "2024" };
            model.RevenueData = new List<decimal> { 0, 0, 0 };

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

            return View(model);
        }

        // CSV Helper
        private List<T> ReadCsv<T>(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
                return new List<T>();

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                HeaderValidated = null,
                BadDataFound = null
            };

            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, config);

            return csv.GetRecords<T>().ToList();
        }
    }
}
