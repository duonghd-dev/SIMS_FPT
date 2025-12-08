using Microsoft.AspNetCore.Mvc;

namespace SIMS_FPT.Areas.Instructor.Controllers
{
    [Area("Instructor")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
