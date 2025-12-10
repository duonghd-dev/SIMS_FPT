using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIMS_FPT.Models;
using System;
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
        // Đường dẫn file
        private readonly string _studentPath = Path.Combine(Directory.GetCurrentDirectory(), "CSV_DATA", "students.csv");
        private readonly string _deptPath = Path.Combine(Directory.GetCurrentDirectory(), "CSV_DATA", "departments.csv");
        private readonly string _feesPath = Path.Combine(Directory.GetCurrentDirectory(), "CSV_DATA", "fees.csv");
        private readonly string _expensePath = Path.Combine(Directory.GetCurrentDirectory(), "CSV_DATA", "expenses.csv");
        private readonly string _teacherPath = Path.Combine(Directory.GetCurrentDirectory(), "CSV_DATA", "teachers.csv");

        public IActionResult Dashboard()
        {
            var model = new DashboardViewModel();

            // 1. Đọc dữ liệu (Sử dụng Helper chung)
            var students = ReadCsv<StudentCSVModel>(_studentPath);
            var departments = ReadCsv<DepartmentModel>(_deptPath);
            var fees = ReadCsv<FeeModel>(_feesPath);
            var expenses = ReadCsv<ExpenseModel>(_expensePath);
            var teachers = ReadCsv<TeacherCSVModel>(_teacherPath);

            // 2. Thống kê cơ bản
            model.StudentCount = students.Count;
            model.DepartmentCount = departments.Count;
            model.TeacherCount = teachers.Count;

            // Tính doanh thu giả định (vì file fees chưa có cột amount chuẩn)
            model.TotalRevenue = fees.Count(f => f.Status == "Paid") * 500;

            // 3. Biểu đồ Doanh thu
            var revenueByYear = fees
                .Where(x => x.PaidDate.HasValue && x.Status == "Paid")
                .GroupBy(x => x.PaidDate.Value.Year)
                .OrderBy(g => g.Key)
                .Select(g => new { Year = g.Key.ToString(), Total = g.Count() * 500m })
                .ToList();

            model.RevenueLabels = revenueByYear.Select(x => x.Year).ToList();
            model.RevenueData = revenueByYear.Select(x => x.Total).ToList();

            // 4. Biểu đồ Sinh viên theo Giới tính
            var studentsByGender = students
                .GroupBy(x => x.Gender ?? "Unknown")
                .Select(g => new { Gender = g.Key, Count = g.Count() })
                .ToList();

            model.StudentClassLabels = studentsByGender.Select(x => x.Gender).ToList();
            model.StudentClassData = studentsByGender.Select(x => x.Count).ToList();

            // 5. Sinh viên mới nhất (Dùng AdmissionDate vừa sửa trong Model)
            model.NewestStudents = students
                .OrderByDescending(s => s.AdmissionDate) // Bây giờ lệnh này sẽ chạy OK
                .Take(5)
                .ToList();

            // 6. Chi tiêu gần đây
            model.RecentExpenses = expenses
                .OrderByDescending(e => e.PurchaseDate)
                .Take(5)
                .ToList();

            return View(model);
        }

        // Helper đọc CSV an toàn
        private List<T> ReadCsv<T>(string filePath)
        {
            if (!System.IO.File.Exists(filePath)) return new List<T>();
            try
            {
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
            catch
            {
                return new List<T>();
            }
        }

        private decimal GetFeeAmount(string feeTypeId)
        {
            return 500;
        }
    }
}