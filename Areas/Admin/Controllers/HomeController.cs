using Microsoft.AspNetCore.Mvc;
using SIMS_FPT.Models;
using System;
using System.Collections.Generic; // Cần thêm
using System.IO;
using System.Linq; // Cần thêm
using System.Text.RegularExpressions;

namespace SIMS_FPT.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController : Controller
    {
        private readonly string _studentPath = Path.Combine(Directory.GetCurrentDirectory(), "CSV_DATA", "students.csv");
        private readonly string _deptPath = Path.Combine(Directory.GetCurrentDirectory(), "CSV_DATA", "departments.csv");
        private readonly string _feesPath = Path.Combine(Directory.GetCurrentDirectory(), "CSV_DATA", "fees.csv");

        public IActionResult Dashboard()
        {
            var model = new DashboardViewModel();

            // 1. Load dữ liệu thô từ CSV
            var allStudents = GetAllStudents();
            var allFees = GetAllFees();
            var allDepts = GetCsvCount(_deptPath); // Chỉ cần đếm số lượng

            // 2. Gán số liệu tổng quan
            model.StudentCount = allStudents.Count;
            model.DepartmentCount = allDepts;
            model.TotalRevenue = allFees.Sum(x => x.Amount);

            // 3. Xử lý dữ liệu cho BIỂU ĐỒ REVENUE (Gom theo Năm)
            var revenueByYear = allFees
                .GroupBy(x => x.PaidDate.Year)
                .OrderBy(g => g.Key)
                .Select(g => new { Year = g.Key.ToString(), Total = g.Sum(x => x.Amount) })
                .ToList();

            model.RevenueLabels = revenueByYear.Select(x => x.Year).ToList();
            model.RevenueData = revenueByYear.Select(x => x.Total).ToList();

            // 4. Xử lý dữ liệu cho BIỂU ĐỒ HỌC SINH (Gom theo Lớp)
            var studentsByClass = allStudents
                .GroupBy(x => x.ClassName)
                .OrderBy(g => g.Key)
                .Select(g => new { ClassName = g.Key, Count = g.Count() })
                .ToList();

            model.StudentClassLabels = studentsByClass.Select(x => x.ClassName).ToList();
            model.StudentClassData = studentsByClass.Select(x => x.Count).ToList();

            return View(model);
        }

        // --- CÁC HÀM HỖ TRỢ ĐỌC CSV ---

        private int GetCsvCount(string filePath)
        {
            if (!System.IO.File.Exists(filePath)) return 0;
            var lines = System.IO.File.ReadAllLines(filePath);
            return lines.Length > 1 ? lines.Length - 1 : 0;
        }

        // Đọc danh sách học sinh để gom nhóm
        private List<StudentCSVModel> GetAllStudents()
        {
            var list = new List<StudentCSVModel>();
            if (!System.IO.File.Exists(_studentPath)) return list;
            var lines = System.IO.File.ReadAllLines(_studentPath);
            for (int i = 1; i < lines.Length; i++)
            {
                var val = Regex.Split(lines[i], ",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
                // Cần lấy cột Class (index 5)
                if (val.Length > 5) 
                {
                    list.Add(new StudentCSVModel { ClassName = val[5].Trim('"') });
                }
            }
            return list;
        }

        // Đọc danh sách phí để gom nhóm
        private List<FeeModel> GetAllFees()
        {
            var list = new List<FeeModel>();
            if (!System.IO.File.Exists(_feesPath)) return list;
            var lines = System.IO.File.ReadAllLines(_feesPath);
            // fees.csv: id,student_name,gender,fees_type,amount,paid_date,status
            for (int i = 1; i < lines.Length; i++)
            {
                var val = Regex.Split(lines[i], ",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
                if (val.Length >= 6)
                {
                    if (decimal.TryParse(val[4], out decimal amount) && 
                        DateTime.TryParse(val[5], out DateTime date))
                    {
                        list.Add(new FeeModel { Amount = amount, PaidDate = date });
                    }
                }
            }
            return list;
        }
    }
}