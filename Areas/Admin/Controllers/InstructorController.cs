using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Models;
using SIMS_FPT.Services.Interfaces;
using System.Threading.Tasks;

namespace SIMS_FPT.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class InstructorController : Controller
    {
        private readonly IAdminInstructorService _instructorService;
        private readonly IDepartmentRepository _deptRepo;

        public InstructorController(IAdminInstructorService instructorService, IDepartmentRepository deptRepo)
        {
            _instructorService = instructorService;
            _deptRepo = deptRepo;
        }

        public IActionResult List()
        {
            return View(_instructorService.GetAllInstructors());
        }

        [HttpGet]
        public IActionResult Add()
        {
            ViewBag.Departments = new SelectList(_deptRepo.GetAll(), "DepartmentId", "DepartmentName");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(TeacherCSVModel model)
        {
            if (ModelState.IsValid)
            {
                var (success, message) = await _instructorService.AddInstructor(model);
                if (success)
                    return RedirectToAction("List");
                else
                    ModelState.AddModelError("", message);
            }

            ViewBag.Departments = new SelectList(_deptRepo.GetAll(), "DepartmentId", "DepartmentName");
            return View(model);
        }

        [HttpGet]
        public IActionResult Edit(string id)
        {
            var teacher = _instructorService.GetInstructorById(id);
            if (teacher == null) return NotFound();

            ViewBag.Departments = new SelectList(_deptRepo.GetAll(), "DepartmentId", "DepartmentName", teacher.DepartmentId);
            return View(teacher);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(TeacherCSVModel model)
        {
            if (ModelState.IsValid)
            {
                var (success, message) = await _instructorService.UpdateInstructor(model);
                if (success)
                    return RedirectToAction("List");
                else
                    ModelState.AddModelError("", message);
            }

            ViewBag.Departments = new SelectList(_deptRepo.GetAll(), "DepartmentId", "DepartmentName", model.DepartmentId);
            return View(model);
        }

        public async Task<IActionResult> DeleteTeacher(string id)
        {
            await _instructorService.DeleteInstructor(id);
            return RedirectToAction("List");
        }

        public IActionResult Detail(string id)
        {
            var teacher = _instructorService.GetInstructorById(id);
            if (teacher == null) return NotFound();

            var dept = _deptRepo.GetById(teacher.DepartmentId ?? "");
            ViewBag.DepartmentName = dept?.DepartmentName ?? "N/A";

            return View(teacher);
        }
    }
}
