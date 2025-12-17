using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIMS_FPT.Models;
using SIMS_FPT.Services.Interfaces;
using System.Threading.Tasks;

namespace SIMS_FPT.Areas.Admin.Controllers
{
    [Area("Admin")]
    // [Authorize(Roles = "Admin")]
    public class StudentController : Controller
    {
        private readonly IAdminStudentService _studentService;

        public StudentController(IAdminStudentService studentService)
        {
            _studentService = studentService;
        }

        public IActionResult List(string className)
        {
            var data = _studentService.GetAllStudents(className);
            ViewBag.SearchTerm = className;
            return View(data);
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(StudentCSVModel model)
        {
            if (ModelState.IsValid)
            {
                var (success, message) = await _studentService.AddStudent(model);
                if (success)
                    return RedirectToAction("List");
                else
                    ModelState.AddModelError("", message);
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Edit(string id)
        {
            var student = _studentService.GetStudentById(id);
            if (student == null) return NotFound();
            return View(student);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(StudentCSVModel model)
        {
            if (ModelState.IsValid)
            {
                var (success, message) = await _studentService.UpdateStudent(model);
                if (success)
                    return RedirectToAction("List");
                else
                    ModelState.AddModelError("", message);
            }
            return View(model);
        }

        public IActionResult Detail(string id)
        {
            var student = _studentService.GetStudentById(id);
            if (student == null) return NotFound();
            return View(student);
        }

        public async Task<IActionResult> DeleteStudent(string id)
        {
            await _studentService.DeleteStudent(id);
            return RedirectToAction("List");
        }
    }
}
