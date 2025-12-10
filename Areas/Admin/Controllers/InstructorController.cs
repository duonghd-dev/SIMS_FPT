using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // Cần thêm cái này cho SelectList
using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Models;
using SIMS_FPT.Services;
using System.Linq;
using System.Threading.Tasks;

namespace SIMS_FPT.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class InstructorController : Controller
    {
        private readonly ITeacherRepository _repo;
        private readonly IDepartmentRepository _deptRepo; // Thêm Department Repo
        private readonly TeacherService _service;

        // Cập nhật Constructor
        public InstructorController(ITeacherRepository repo,
                                    IDepartmentRepository deptRepo,
                                    TeacherService service)
        {
            _repo = repo;
            _deptRepo = deptRepo;
            _service = service;
        }

        public IActionResult List()
        {
            return View(_repo.GetAll());
        }

        [HttpGet]
        public IActionResult Add()
        {
            // Lấy danh sách khoa để hiển thị Dropdown
            var departments = _deptRepo.GetAll();
            ViewBag.Departments = new SelectList(departments, "DepartmentId", "DepartmentName");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(TeacherCSVModel model)
        {
            // Kiểm tra trùng ID
            if (_repo.GetById(model.TeacherId) != null)
            {
                ModelState.AddModelError("TeacherId", "Teacher ID already exists!");

                // Load lại dropdown nếu lỗi
                var departments = _deptRepo.GetAll();
                ViewBag.Departments = new SelectList(departments, "DepartmentId", "DepartmentName");
                return View(model);
            }

            if (ModelState.IsValid)
            {
                await _service.Add(model);
                return RedirectToAction("List");
            }

            // Load lại dropdown nếu model invalid
            ViewBag.Departments = new SelectList(_deptRepo.GetAll(), "DepartmentId", "DepartmentName");
            return View(model);
        }

        [HttpGet]
        public IActionResult Edit(string id)
        {
            var teacher = _repo.GetById(id);
            if (teacher == null) return NotFound();

            // Load danh sách khoa cho Dropdown
            var departments = _deptRepo.GetAll();
            ViewBag.Departments = new SelectList(departments, "DepartmentId", "DepartmentName", teacher.DepartmentId);

            return View(teacher);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(TeacherCSVModel model)
        {
            if (ModelState.IsValid)
            {
                await _service.Update(model);
                return RedirectToAction("List");
            }

            ViewBag.Departments = new SelectList(_deptRepo.GetAll(), "DepartmentId", "DepartmentName", model.DepartmentId);
            return View(model);
        }

        public IActionResult DeleteTeacher(string id)
        {
            _service.Delete(id);
            return RedirectToAction("List");
        }

        public IActionResult Detail(string id)
        {
            var teacher = _repo.GetById(id);
            if (teacher == null) return NotFound();

            // Lấy tên khoa để hiển thị (Optional)
            var dept = _deptRepo.GetById(teacher.DepartmentId ?? "");
            ViewBag.DepartmentName = dept?.DepartmentName ?? "N/A";

            return View(teacher);
        }
    }
}