using Microsoft.AspNetCore.Mvc;
using SIMS_FPT.Models;
using Microsoft.AspNetCore.Authorization;
using SIMS_FPT.Services.Interfaces;

namespace SIMS_FPT.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class HomeController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public HomeController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public IActionResult Dashboard()
        {
            var model = _dashboardService.GetAdminDashboardData();
            return View(model);
        }
    }
}
