using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Models;
using SIMS_FPT.Services.Interfaces;
using System.Linq;

namespace SIMS_FPT.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class DepartmentController : Controller
    {
        private readonly IAdminDepartmentService _deptService;
        private readonly ITeacherRepository _teacherRepo;
        private readonly ISubjectRepository _subjectRepo;

        public DepartmentController(IAdminDepartmentService deptService, ITeacherRepository teacherRepo, ISubjectRepository subjectRepo)
        {
            _deptService = deptService;
            _teacherRepo = teacherRepo;
            _subjectRepo = subjectRepo;
        }

        public IActionResult List()
        {
            return View(_deptService.GetAllDepartments());
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Add(DepartmentModel model)
        {
            if (ModelState.IsValid)
            {
                var (success, message) = _deptService.AddDepartment(model);
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
            var item = _deptService.GetDepartmentById(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost]
        public IActionResult Edit(DepartmentModel model)
        {
            if (ModelState.IsValid)
            {
                var (success, message) = _deptService.UpdateDepartment(model);
                if (success)
                    return RedirectToAction("List");
                else
                    ModelState.AddModelError("", message);
            }
            return View(model);
        }

        public IActionResult Delete(string id)
        {
            var (success, message) = _deptService.DeleteDepartment(id);
            return RedirectToAction("List");
        }

        public IActionResult Detail(string id)
        {
            var dept = _deptService.GetDepartmentById(id);
            if (dept == null) return NotFound();

            var deptTeachers = _teacherRepo.GetAll().Where(t => t.DepartmentId == id).ToList();
            var deptSubjects = _subjectRepo.GetAll().Where(s => s.DepartmentId == id).ToList();
            var viewModel = new DepartmentDetailViewModel
            {
                Department = dept,
                Teachers = deptTeachers,
                Subjects = deptSubjects
            };
            return View(viewModel);
        }
    }
}