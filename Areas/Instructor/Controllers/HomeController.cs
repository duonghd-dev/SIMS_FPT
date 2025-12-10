using Microsoft.AspNetCore.Mvc;
using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Models.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace SIMS_FPT.Areas.Instructor.Controllers
{
    [Area("Instructor")]
    public class HomeController : Controller
    {
        private readonly ISubjectRepository _subjectRepo;

        public HomeController(ISubjectRepository subjectRepo)
        {
            _subjectRepo = subjectRepo;
        }

        // RENAMED FROM Index TO Dashboard
        public IActionResult Dashboard()
        {
            // 1. Fetch Data
            var subjects = _subjectRepo.GetAll().Take(3).ToList();

            // 2. Populate ViewModel
            var model = new InstructorDashboardViewModel
            {
                TodayClasses = subjects.Select((s, index) => new ClassScheduleItem
                {
                    SubjectName = s.SubjectName,
                    ClassName = s.Class,
                    Time = $"{8 + (index * 2)}:00 AM - {10 + (index * 2)}:00 AM",
                    Room = $"Room A-{101 + index}"
                }).ToList(),

                AtRiskStudents = new List<AtRiskStudent>
                {
                    new AtRiskStudent { StudentName = "John Doe", Reason = "Attendance < 80%", RiskLevel = "High" },
                    new AtRiskStudent { StudentName = "Jane Smith", Reason = "Failed Mid-term", RiskLevel = "Medium" }
                },

                PerformanceLabels = new List<string> { "Week 1", "Week 2", "Week 3", "Week 4" },
                PerformanceData = new List<int> { 65, 70, 68, 85 },

                LeaveDaysRemaining = 12,
                LastSalaryMonth = "October 2023"
            };

            // 3. Return the View
            return View(model);
        }

        public IActionResult MyPayslips()
        {
            return View();
        }
    }
}