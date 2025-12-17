using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIMS_FPT.Services.Interfaces;
using System.Security.Claims;

namespace SIMS_FPT.Areas.Instructor.Controllers
{
    [Area("Instructor")]
    [Authorize]
    public class StudentController : Controller
    {
        private readonly IInstructorStudentService _studentService;

        public StudentController(IInstructorStudentService studentService)
        {
            _studentService = studentService;
        }

        private string CurrentTeacherId
        {
            get
            {
                var linkedId = User.FindFirst("LinkedId")?.Value;
                if (!string.IsNullOrEmpty(linkedId)) return linkedId;
                return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity?.Name ?? "UNKNOWN";
            }
        }

        public IActionResult Profile(string id)
        {
            var vm = _studentService.GetStudentProfile(id, CurrentTeacherId);
            if (vm == null)
                return RedirectToAction("AccessDenied", "Login", new { area = "" });

            return View(vm);
        }
    }
}


