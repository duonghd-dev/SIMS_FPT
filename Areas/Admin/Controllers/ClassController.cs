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
    [Area("Admin")]
    public class ClassController : Controller
    {
        private readonly IAdminClassService _classService;
        private readonly IClassRepository _classRepo;
        private readonly ISubjectRepository _subjectRepo;
        private readonly ITeacherRepository _teacherRepo;

        public ClassController(
            IAdminClassService classService,
            IClassRepository classRepo,
            ISubjectRepository subjectRepo,
            ITeacherRepository teacherRepo)
        {
            _classService = classService;
            _classRepo = classRepo;
            _subjectRepo = subjectRepo;
            _teacherRepo = teacherRepo;
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
            ViewBag.AllSubjects = _subjectRepo.GetAll();
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
                ViewBag.AllSubjects = _subjectRepo.GetAll();
                LoadAllTeachersWithSubjects();
                return View(new ClassDetailViewModel { Class = model });
            }

            // Check for duplicate Class ID
            if (!string.IsNullOrWhiteSpace(model.ClassId) && _classRepo.GetById(model.ClassId) != null)
            {
                ModelState.AddModelError("ClassId", "Class ID already exists in the system!");
                ViewBag.AllSubjects = _subjectRepo.GetAll();
                LoadAllTeachersWithSubjects();
                return View(new ClassDetailViewModel { Class = model });
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

            ViewBag.AllSubjects = _subjectRepo.GetAll();
            LoadAllTeachersWithSubjects();
            return View(new ClassDetailViewModel { Class = model });
        }

        // 3. Edit class (GET)
        [HttpGet]
        public IActionResult Edit(string id)
        {
            var viewModel = _classService.GetClassWithDetails(id);
            if (viewModel == null) return NotFound();

            ViewBag.AllSubjects = _subjectRepo.GetAll();
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

            ViewBag.AllSubjects = _subjectRepo.GetAll();
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