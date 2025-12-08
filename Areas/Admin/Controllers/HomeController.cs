using Microsoft.AspNetCore.Mvc;
using SIMS_FPT.Models;
using System.IO;
using Microsoft.AspNetCore.Authorization;


namespace SIMS_FPT.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class HomeController : Controller
    {
        public IActionResult Dashboard()
        {
            var model = new DashboardViewModel
            {
                StudentCount = GetCsvCount("students.csv"),
                DepartmentCount = GetCsvCount("departments.csv"),
                TeacherCount = GetCsvCount("teachers.csv") 
            };

            return View(model);
        }

        private int GetCsvCount(string fileName)
        {
            try
            {
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "CSV_DATA", fileName);
                if (!System.IO.File.Exists(filePath)) return 0;

                var lines = System.IO.File.ReadAllLines(filePath);
                
                return lines.Length > 1 ? lines.Length - 1 : 0;
            }
            catch
            {
                return 0;
            }
        }
    }
}