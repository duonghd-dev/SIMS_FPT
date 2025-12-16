using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Helpers;
using SIMS_FPT.Models;
using System.Linq;

namespace SIMS_FPT.Areas.Admin.Controllers
{
    [Area("Admin")]
    // [Authorize(Roles = "Admin")]
    public class SubjectController : Controller
    {
        private readonly ISubjectRepository _repo;
        private readonly IDepartmentRepository _deptRepo;
        private readonly ITeacherRepository _teacherRepo;

        public SubjectController(ISubjectRepository repo, IDepartmentRepository deptRepo, ITeacherRepository teacherRepo)
        {
            _repo = repo;
            _deptRepo = deptRepo;
            _teacherRepo = teacherRepo;
        }

        public IActionResult List()
        {
            var subjects = _repo.GetAll();
            var allTeachers = _teacherRepo.GetAll().ToDictionary(t => t.TeacherId, t => t.Name);
            ViewBag.TeacherNames = allTeachers;
            return View(subjects);
        }

        [HttpGet]
        public IActionResult Add()
        {
            // Load danh sách Khoa vào Dropdown
            ViewBag.Departments = new SelectList(_deptRepo.GetAll(), "DepartmentId", "DepartmentName");

            // Load tất cả teachers với DepartmentId
            LoadAllTeachersWithDepartment();

            return View();
        }

        [HttpPost]
        public IActionResult Add(SubjectModel model)
        {
            // Validate Subject ID format
            if (!ValidationHelper.IsValidId(model.SubjectId))
            {
                ModelState.AddModelError("SubjectId", "Subject ID must be 3-20 alphanumeric characters!");
                ViewBag.Departments = new SelectList(_deptRepo.GetAll(), "DepartmentId", "DepartmentName");
                LoadAllTeachersWithDepartment(model.SelectedTeacherIds);
                return View(model);
            }

            // Check for duplicate Subject ID
            if (_repo.GetById(model.SubjectId) != null)
            {
                ModelState.AddModelError("SubjectId", "Subject ID already exists in the system!");
                ViewBag.Departments = new SelectList(_deptRepo.GetAll(), "DepartmentId", "DepartmentName");
                LoadAllTeachersWithDepartment(model.SelectedTeacherIds);
                return View(model);
            }

            // Validate credits
            if (!ValidationHelper.IsValidCredits(model.Credits))
            {
                ModelState.AddModelError("Credits", "Credits must be between 1 and 10!");
                ViewBag.Departments = new SelectList(_deptRepo.GetAll(), "DepartmentId", "DepartmentName");
                LoadAllTeachersWithDepartment(model.SelectedTeacherIds);
                return View(model);
            }

            if (ModelState.IsValid)
            {
                // Convert selected teacher IDs to comma-separated string
                model.TeacherIds = model.SelectedTeacherIds != null && model.SelectedTeacherIds.Any()
                    ? string.Join(",", model.SelectedTeacherIds)
                    : "";

                _repo.Add(model);
                return RedirectToAction("List");
            }

            ViewBag.Departments = new SelectList(_deptRepo.GetAll(), "DepartmentId", "DepartmentName");
            LoadAllTeachersWithDepartment(model.SelectedTeacherIds);
            return View(model);
        }

        [HttpGet]
        public IActionResult Edit(string id)
        {
            var item = _repo.GetById(id);
            if (item == null) return NotFound();

            // Parse teacher IDs from comma-separated string
            item.SelectedTeacherIds = !string.IsNullOrEmpty(item.TeacherIds)
                ? item.TeacherIds.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                : new List<string>();

            // Load danh sách Khoa, chọn sẵn khoa hiện tại của môn học
            ViewBag.Departments = new SelectList(_deptRepo.GetAll(), "DepartmentId", "DepartmentName", item.DepartmentId);

            // Load tất cả teachers với DepartmentId
            LoadAllTeachersWithDepartment(item.SelectedTeacherIds);

            return View(item);
        }

        [HttpPost]
        public IActionResult Edit(SubjectModel model)
        {
            // Validate credits
            if (!ValidationHelper.IsValidCredits(model.Credits))
            {
                ModelState.AddModelError("Credits", "Credits must be between 1 and 10!");
                ViewBag.Departments = new SelectList(_deptRepo.GetAll(), "DepartmentId", "DepartmentName", model.DepartmentId);
                LoadAllTeachersWithDepartment(model.SelectedTeacherIds);
                return View(model);
            }

            if (ModelState.IsValid)
            {
                // Convert selected teacher IDs to comma-separated string
                model.TeacherIds = model.SelectedTeacherIds != null && model.SelectedTeacherIds.Any()
                    ? string.Join(",", model.SelectedTeacherIds)
                    : "";

                _repo.Update(model);
                return RedirectToAction("List");
            }

            ViewBag.Departments = new SelectList(_deptRepo.GetAll(), "DepartmentId", "DepartmentName", model.DepartmentId);
            LoadAllTeachersWithDepartment(model.SelectedTeacherIds);
            return View(model);
        }

        public IActionResult Delete(string id)
        {
            _repo.Delete(id);
            return RedirectToAction("List");
        }

        private void LoadTeachersViewBag(List<string>? selectedTeacherIds = null)
        {
            var teachers = _teacherRepo.GetAll()
                .Select(t => new { Value = t.TeacherId, Text = $"{t.TeacherId} - {t.Name}" })
                .ToList();
            ViewBag.Teachers = new MultiSelectList(teachers, "Value", "Text", selectedTeacherIds);
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
