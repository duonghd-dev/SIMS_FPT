using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Helpers;
using SIMS_FPT.Models;
using SIMS_FPT.Models.ViewModels;
using SIMS_FPT.Services.Interfaces;
using System.Linq;

namespace SIMS_FPT.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class ClassController : Controller
    {
        private readonly IAdminClassService _classService;
        private readonly IClassRepository _classRepo;
        private readonly ISubjectRepository _subjectRepo;
        private readonly ITeacherRepository _teacherRepo;
        private readonly IDepartmentRepository _deptRepo;

        public ClassController(
            IAdminClassService classService,
            IClassRepository classRepo,
            ISubjectRepository subjectRepo,
            ITeacherRepository teacherRepo,
            IDepartmentRepository deptRepo)
        {
            _classService = classService;
            _classRepo = classRepo;
            _subjectRepo = subjectRepo;
            _teacherRepo = teacherRepo;
            _deptRepo = deptRepo;
        }

        // 1. List all classes
        public IActionResult List()
        {
            var viewModel = _classService.GetAllClassesWithDetails();
            return View(viewModel);
        }

        // 2. Add class (GET)
        [HttpGet]
        public IActionResult Add()
        {
            var departments = _deptRepo.GetAll();
            ViewBag.Departments = new SelectList(departments, "DepartmentId", "DepartmentName");

            // Pass departments with capacity info for validation
            var deptList = departments.Select(d => new { value = d.DepartmentId, text = $"{d.NumberOfStudents} - {d.DepartmentName}" }).ToList();
            ViewBag.DepartmentCapacity = deptList;

            var subjects = _subjectRepo.GetAll().Select(s => new
            {
                subjectId = s.SubjectId,
                subjectName = s.SubjectName,
                departmentId = s.DepartmentId ?? ""
            }).ToList();
            ViewBag.AllSubjects = subjects;

            // Load all classes for capacity calculation
            var allClasses = _classRepo.GetAll().Select(c => new
            {
                classId = c.ClassId,
                numberOfStudents = c.NumberOfStudents,
                departmentId = c.DepartmentId ?? ""
            }).ToList();
            ViewBag.AllClasses = allClasses;

            LoadAllTeachersWithSubjects();
            return View(new ClassDetailViewModel { Class = new ClassModel() });
        }

        // 2. Add class (POST)
        [HttpPost]
        public IActionResult Add(ClassDetailViewModel viewModel, List<string> SubjectIds, List<string> TeacherIds)
        {
            var model = viewModel.Class;

            // Validate Class ID format
            if (string.IsNullOrWhiteSpace(model.ClassId) || !ValidationHelper.IsValidId(model.ClassId))
            {
                ModelState.AddModelError("ClassId", "Class ID must be 3-20 alphanumeric characters!");
                ViewBag.Departments = new SelectList(_deptRepo.GetAll(), "DepartmentId", "DepartmentName", model.DepartmentId);
                ViewBag.AllSubjects = _subjectRepo.GetAll();
                LoadAllTeachersWithSubjects();
                return View(new ClassDetailViewModel { Class = model });
            }

            // Check for duplicate Class ID
            if (!string.IsNullOrWhiteSpace(model.ClassId) && _classRepo.GetById(model.ClassId) != null)
            {
                ModelState.AddModelError("ClassId", "Class ID already exists in the system!");
                ViewBag.Departments = new SelectList(_deptRepo.GetAll(), "DepartmentId", "DepartmentName", model.DepartmentId);
                ViewBag.AllSubjects = _subjectRepo.GetAll();
                LoadAllTeachersWithSubjects();
                return View(new ClassDetailViewModel { Class = model });
            }

            if (ModelState.IsValid)
            {
                // Validate that total students in department doesn't exceed capacity
                var dept = _deptRepo.GetById(model.DepartmentId);
                if (dept != null)
                {
                    var existingClasses = _classRepo.GetAll()
                        .Where(c => c.DepartmentId == model.DepartmentId)
                        .ToList();

                    var totalStudents = existingClasses.Sum(c => c.NumberOfStudents) + model.NumberOfStudents;

                    if (totalStudents > dept.NumberOfStudents)
                    {
                        var currentUsed = existingClasses.Sum(c => c.NumberOfStudents);
                        var available = dept.NumberOfStudents - currentUsed;
                        ModelState.AddModelError("NumberOfStudents",
                            $"Total students in department cannot exceed {dept.NumberOfStudents}. " +
                            $"Currently used: {currentUsed}, Available: {available}");
                    }
                }

                if (ModelState.IsValid)
                {
                    var (success, message) = _classService.AddClass(model, SubjectIds ?? new List<string>(), TeacherIds ?? new List<string>());
                    if (success)
                    {
                        return RedirectToAction("List");
                    }
                    else
                    {
                        ModelState.AddModelError("", message);
                    }
                }
            }

            var depts = _deptRepo.GetAll();
            ViewBag.Departments = new SelectList(depts, "DepartmentId", "DepartmentName", model.DepartmentId);
            var deptList = depts.Select(d => new { value = d.DepartmentId, text = $"{d.NumberOfStudents} - {d.DepartmentName}" }).ToList();
            ViewBag.DepartmentCapacity = deptList;
            ViewBag.AllSubjects = _subjectRepo.GetAll();
            var allClasses = _classRepo.GetAll().Select(c => new
            {
                classId = c.ClassId,
                numberOfStudents = c.NumberOfStudents,
                departmentId = c.DepartmentId ?? ""
            }).ToList();
            ViewBag.AllClasses = allClasses;
            LoadAllTeachersWithSubjects();
            return View(new ClassDetailViewModel { Class = model });
        }

        // 3. Edit class (GET)
        [HttpGet]
        public IActionResult Edit(string id)
        {
            var viewModel = _classService.GetClassWithDetails(id);
            if (viewModel == null) return NotFound();

            var departments = _deptRepo.GetAll();
            ViewBag.Departments = new SelectList(departments, "DepartmentId", "DepartmentName", viewModel.Class?.DepartmentId);

            // Pass departments with capacity info for validation
            var deptList = departments.Select(d => new { value = d.DepartmentId, text = $"{d.NumberOfStudents} - {d.DepartmentName}" }).ToList();
            ViewBag.DepartmentCapacity = deptList;

            var subjects = _subjectRepo.GetAll().Select(s => new
            {
                subjectId = s.SubjectId,
                subjectName = s.SubjectName,
                departmentId = s.DepartmentId ?? ""
            }).ToList();
            ViewBag.AllSubjects = subjects;

            // Load all classes for capacity calculation (excluding current class)
            var allClasses = _classRepo.GetAll()
                .Where(c => c.ClassId != id)
                .Select(c => new
                {
                    classId = c.ClassId,
                    numberOfStudents = c.NumberOfStudents,
                    departmentId = c.DepartmentId ?? ""
                }).ToList();
            ViewBag.AllClasses = allClasses;

            LoadAllTeachersWithSubjects();
            return View(viewModel);
        }

        // 3. Edit class (POST)
        [HttpPost]
        public IActionResult Edit(ClassDetailViewModel viewModel, List<string> SubjectIds, List<string> TeacherIds)
        {
            var model = viewModel.Class;

            if (ModelState.IsValid)
            {
                // Validate that total students in department doesn't exceed capacity
                var dept = _deptRepo.GetById(model.DepartmentId);
                if (dept != null)
                {
                    var existingClasses = _classRepo.GetAll()
                        .Where(c => c.DepartmentId == model.DepartmentId && c.ClassId != model.ClassId)
                        .ToList();

                    var totalStudents = existingClasses.Sum(c => c.NumberOfStudents) + model.NumberOfStudents;

                    if (totalStudents > dept.NumberOfStudents)
                    {
                        var currentUsed = existingClasses.Sum(c => c.NumberOfStudents);
                        var available = dept.NumberOfStudents - currentUsed;
                        ModelState.AddModelError("NumberOfStudents",
                            $"Total students in department cannot exceed {dept.NumberOfStudents}. " +
                            $"Currently used: {currentUsed}, Available: {available}");
                    }
                }

                if (ModelState.IsValid)
                {
                    var (success, message) = _classService.UpdateClass(model, SubjectIds ?? new List<string>(), TeacherIds ?? new List<string>());
                    if (success)
                    {
                        return RedirectToAction("List");
                    }
                    else
                    {
                        ModelState.AddModelError("", message);
                    }
                }
            }

            var depts = _deptRepo.GetAll();
            ViewBag.Departments = new SelectList(depts, "DepartmentId", "DepartmentName", model?.DepartmentId);
            var deptList = depts.Select(d => new { value = d.DepartmentId, text = $"{d.NumberOfStudents} - {d.DepartmentName}" }).ToList();
            ViewBag.DepartmentCapacity = deptList;
            ViewBag.AllSubjects = _subjectRepo.GetAll();
            var allClasses = _classRepo.GetAll()
                .Where(c => c.ClassId != model?.ClassId)
                .Select(c => new
                {
                    classId = c.ClassId,
                    numberOfStudents = c.NumberOfStudents,
                    departmentId = c.DepartmentId ?? ""
                }).ToList();
            ViewBag.AllClasses = allClasses;
            LoadAllTeachersWithSubjects();
            return View(viewModel);
        }

        // 4. Delete class
        public IActionResult Delete(string id)
        {
            var (success, message) = _classService.DeleteClass(id);
            return RedirectToAction("List");
        }

        // 5. Manage students in class (GET)
        [HttpGet]
        public IActionResult ManageStudents(string id)
        {
            var viewModel = _classService.GetClassEnrollment(id);
            if (viewModel == null) return NotFound();
            return View(viewModel);
        }

        // 6. Add students to class (POST)
        [HttpPost]
        public IActionResult AddStudentsToClass(string classId, List<string> selectedStudents)
        {
            var (success, message) = _classService.AddStudentsToClass(classId, selectedStudents ?? new List<string>());
            return RedirectToAction("ManageStudents", new { id = classId });
        }

        // 7. Remove student from class
        public IActionResult RemoveStudentFromClass(string classId, string studentId)
        {
            var (success, message) = _classService.RemoveStudentFromClass(classId, studentId);
            return RedirectToAction("ManageStudents", new { id = classId });
        }

        private void LoadAllTeachersWithSubjects()
        {
            var allSubjects = _subjectRepo.GetAll();
            var allTeachers = _teacherRepo.GetAll();

            var teachersWithSubjects = allTeachers.Select(t => new
            {
                Id = t.TeacherId,
                Name = $"{t.TeacherId} - {t.Name}",
                DeptId = t.DepartmentId ?? "",
                SubjectIds = allSubjects
                    .Where(s => !string.IsNullOrEmpty(s.TeacherIds) &&
                                s.TeacherIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(id => id.Trim())
                                .Contains(t.TeacherId))
                    .Select(s => s.SubjectId)
                    .ToList()
            }).ToList();

            ViewBag.AllTeachers = teachersWithSubjects;
        }
    }
}