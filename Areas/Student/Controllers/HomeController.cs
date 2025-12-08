using Microsoft.AspNetCore.Mvc;

namespace SIMS_Project.Areas.Student.Controllers
{
    [Area("Student")]
    public class HomeController : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}