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

        public IActionResult Index(string? classId, string? search)
        {
            var vm = _studentService.GetManagedStudents(CurrentTeacherId, classId, search);
            return View(vm);
        }

        public IActionResult Profile(string id)
        {
            var vm = _studentService.GetStudentProfile(id, CurrentTeacherId);
            if (vm == null)
                return RedirectToAction("AccessDenied", "Login", new { area = "" });

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(string classId, string studentId)
        {
            if (string.IsNullOrEmpty(classId) || string.IsNullOrEmpty(studentId))
            {
                TempData["Error"] = "Invalid data.";
                return RedirectToAction(nameof(Index));
            }

            var success = _studentService.RemoveStudentFromClass(CurrentTeacherId, classId, studentId);
            if (success)
            {
                TempData["Success"] = "Student removed from class successfully.";
            }
            else
            {
                TempData["Error"] = "Failed to remove student. You may not have permission or the student was not found.";
            }

            // Redirect back to Index, preserving the current filter
            return RedirectToAction(nameof(Index), new { classId });
        }
    }
}