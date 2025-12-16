using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SIMS_FPT.Models;
using System.Security.Claims; // Required for ClaimTypes

namespace SIMS_FPT.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // FIX: Check if the user is already logged in
            if (User.Identity?.IsAuthenticated == true)
            {
                // Get the user's role
                var role = User.FindFirst(ClaimTypes.Role)?.Value;

                // Redirect to the correct dashboard based on role
                // (This logic is copied from LoginController)
                return role switch
                {
                    "Admin" => RedirectToAction("Dashboard", "Home", new { area = "Admin" }),
                    "Instructor" => RedirectToAction("Dashboard", "Home", new { area = "Instructor" }),
                    "Student" => RedirectToAction("Dashboard", "Home", new { area = "Student" }),
                    _ => RedirectToAction("Login", "Login")
                };
            }

            // If not logged in, show the landing page
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}