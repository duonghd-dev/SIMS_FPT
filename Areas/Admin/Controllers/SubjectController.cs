using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Models;
using SIMS_FPT.Services.Interfaces;
using System.Linq;

namespace SIMS_FPT.Areas.Admin.Controllers
{
    [Area("Admin")]
    // [Authorize(Roles = "Admin")]
    public class SubjectController : Controller
    {
        private readonly IAdminSubjectService _subjectService;
        private readonly IDepartmentRepository _deptRepo;
        private readonly ITeacherRepository _teacherRepo;

        public SubjectController(IAdminSubjectService subjectService, IDepartmentRepository deptRepo, ITeacherRepository teacherRepo)
        {
            _subjectService = subjectService;
            _deptRepo = deptRepo;
            _teacherRepo = teacherRepo;
        }

        public IActionResult List()
        {
            var subjects = _subjectService.GetAllSubjects();
            var allTeachers = _subjectService.GetTeacherNamesByIds();
            ViewBag.TeacherNames = allTeachers;
            return View(subjects);
        }

        [HttpGet]
        public IActionResult Add()
        {
            ViewBag.Departments = new SelectList(_deptRepo.GetAll(), "DepartmentId", "DepartmentName");
            LoadAllTeachersWithDepartment();
            return View();
        }

        [HttpPost]
        public IActionResult Add(SubjectModel model)
        {
            if (ModelState.IsValid)
            {
                model.TeacherIds = model.SelectedTeacherIds != null && model.SelectedTeacherIds.Any()
                    ? string.Join(",", model.SelectedTeacherIds)
                    : "";

                var (success, message) = _subjectService.AddSubject(model);
                if (success)
                    return RedirectToAction("List");
                else
                    ModelState.AddModelError("", message);
            }

            ViewBag.Departments = new SelectList(_deptRepo.GetAll(), "DepartmentId", "DepartmentName");
            LoadAllTeachersWithDepartment(model.SelectedTeacherIds);
            return View(model);
        }

        [HttpGet]
        public IActionResult Edit(string id)
        {
            var item = _subjectService.GetSubjectById(id);
            if (item == null) return NotFound();

            item.SelectedTeacherIds = !string.IsNullOrEmpty(item.TeacherIds)
                ? item.TeacherIds.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                : new List<string>();

            ViewBag.Departments = new SelectList(_deptRepo.GetAll(), "DepartmentId", "DepartmentName", item.DepartmentId);
            LoadAllTeachersWithDepartment(item.SelectedTeacherIds);
            return View(item);
        }

        [HttpPost]
        public IActionResult Edit(SubjectModel model)
        {
            if (ModelState.IsValid)
            {
                model.TeacherIds = model.SelectedTeacherIds != null && model.SelectedTeacherIds.Any()
                    ? string.Join(",", model.SelectedTeacherIds)
                    : "";

                var (success, message) = _subjectService.UpdateSubject(model);
                if (success)
                    return RedirectToAction("List");
                else
                    ModelState.AddModelError("", message);
            }

            ViewBag.Departments = new SelectList(_deptRepo.GetAll(), "DepartmentId", "DepartmentName", model.DepartmentId);
            LoadAllTeachersWithDepartment(model.SelectedTeacherIds);
            return View(model);
        }

        public IActionResult Delete(string id)
        {
            var (success, message) = _subjectService.DeleteSubject(id);
            return RedirectToAction("List");
        }

        private void LoadAllTeachersWithDepartment(List<string>? selectedTeacherIds = null)
        {
            var teachers = _teacherRepo.GetAll()
                .Select(t => new
                {
                    Value = t.TeacherId,
                    Text = $"{t.TeacherId} - {t.Name}",
                    DepartmentId = t.DepartmentId ?? ""
                })
                .ToList();
            ViewBag.AllTeachers = teachers;
            ViewBag.SelectedTeacherIds = selectedTeacherIds ?? new List<string>();
        }
    }
}
