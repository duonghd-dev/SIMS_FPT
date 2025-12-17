using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SIMS_FPT.Services.Interfaces;
using System.Security.Claims;

namespace SIMS_FPT.Areas.Student.Controllers
{
    [Area("Student")]
    public class AssignmentController : Controller
    {
        private readonly IStudentAssignmentService _assignmentService;

        public AssignmentController(IStudentAssignmentService assignmentService)
        {
            _assignmentService = assignmentService;
        }

        private string CurrentStudentId
        {
            get
            {
                var linkedId = User.FindFirst("LinkedId")?.Value;
                if (!string.IsNullOrEmpty(linkedId)) return linkedId;
                return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity?.Name ?? string.Empty;
            }
        }

        public IActionResult Index()
        {
            var viewModel = _assignmentService.GetStudentAssignments(CurrentStudentId);
            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Submit(string id)
        {
            var vm = _assignmentService.GetAssignmentForSubmission(id, CurrentStudentId);
            if (vm == null)
                return RedirectToAction("AccessDenied", "Login", new { area = "" });

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Submit(string id, IFormFile uploadFile)
        {
            if (uploadFile == null || uploadFile.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "Please select a file to upload.");
                var fallbackVm = _assignmentService.GetAssignmentForSubmission(id, CurrentStudentId);
                return View(fallbackVm);
            }

            var (success, message, filePath) = _assignmentService.SubmitAssignment(id, CurrentStudentId, uploadFile);
            if (success)
            {
                TempData["Success"] = message;
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError(string.Empty, message);
                var fallbackVm = _assignmentService.GetAssignmentForSubmission(id, CurrentStudentId);
                return View(fallbackVm);
            }
        }
    }
}