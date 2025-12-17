using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SIMS_FPT.Models;
using SIMS_FPT.Services.Interfaces;
using System.Linq;
using System.Security.Claims;

namespace SIMS_FPT.Areas.Instructor.Controllers
{
    [Area("Instructor")]
    [Authorize]
    public class CourseMaterialController : Controller
    {
        private readonly IInstructorCourseMaterialService _materialService;

        public CourseMaterialController(IInstructorCourseMaterialService materialService)
        {
            _materialService = materialService;
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

        public IActionResult Index()
        {
            var materials = _materialService.GetTeacherMaterials(CurrentTeacherId);
            return View(materials);
        }

        public IActionResult Create()
        {
            var list = _materialService.GetTeacherClassSubjectList(CurrentTeacherId);
            ViewBag.SubjectList = list.Select(x => new SelectListItem { Value = x.SubjectId, Text = x.DisplayText }).ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CourseMaterialModel model, IFormFile uploadFile)
        {
            ModelState.Remove("MaterialId");
            ModelState.Remove("UploadDate");

            if (ModelState.IsValid)
            {
                var (success, message, filePath) = _materialService.CreateMaterial(model, uploadFile, CurrentTeacherId);
                if (success)
                {
                    TempData["Success"] = message;
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, message);
                }
            }

            var list = _materialService.GetTeacherClassSubjectList(CurrentTeacherId);
            ViewBag.SubjectList = list.Select(x => new SelectListItem { Value = x.SubjectId, Text = x.DisplayText }).ToList();
            TempData["Error"] = "Please fix the errors below and try again.";
            return View(model);
        }

        [HttpPost]
        public IActionResult Delete(string id)
        {
            var (success, message) = _materialService.DeleteMaterial(id, CurrentTeacherId);
            if (!success)
                TempData["Error"] = message;
            else
                TempData["Success"] = message;

            return RedirectToAction("Index");
        }
    }
}