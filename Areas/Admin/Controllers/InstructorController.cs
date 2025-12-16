using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // Cần thêm cái này cho SelectList
using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Helpers;
using SIMS_FPT.Models;
using SIMS_FPT.Services;
using System;
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
            var departments = _deptRepo.GetAll();
            ViewBag.Departments = new SelectList(departments, "DepartmentId", "DepartmentName");

            // Validate Teacher ID format
            if (!ValidationHelper.IsValidId(model.TeacherId))
            {
                ModelState.AddModelError("TeacherId", "Teacher ID must be 3-20 alphanumeric characters!");
                return View(model);
            }

            // Check for duplicate Teacher ID
            if (_repo.GetById(model.TeacherId) != null)
            {
                ModelState.AddModelError("TeacherId", "Teacher ID already exists in the system!");
                return View(model);
            }

            // Validate Email format
            if (!ValidationHelper.IsValidEmail(model.Email))
            {
                ModelState.AddModelError("Email", "Please enter a valid email address (e.g., example@gmail.com)!");
                return View(model);
            }

            // Check for duplicate Email
            var existingEmail = _repo.GetAll()
                .FirstOrDefault(t => t.Email?.ToLower() == model.Email?.ToLower());
            if (existingEmail != null)
            {
                ModelState.AddModelError("Email", "This email address is already registered in the system!");
                return View(model);
            }

            // Validate Mobile format (if provided)
            if (!string.IsNullOrWhiteSpace(model.Mobile) && !ValidationHelper.IsValidPhoneNumber(model.Mobile))
            {
                ModelState.AddModelError("Mobile", "Mobile number must be 10-11 digits (e.g., 0123456789)!");
                return View(model);
            }

            // Validate Date of Birth
            if (model.DateOfBirth.HasValue && !ValidationHelper.IsValidDateOfBirth(model.DateOfBirth))
            {
                var today = DateTime.Now;
                if (model.DateOfBirth > today)
                {
                    ModelState.AddModelError("DateOfBirth", "Date of birth cannot be in the future!");
                }
                else
                {
                    ModelState.AddModelError("DateOfBirth", "Teacher must be at least 5 years old!");
                }
                return View(model);
            }

            // Validate Joining Date relative to Date of Birth (must be >= DOB and age >= 21 at join)
            if (model.JoiningDate.HasValue && model.DateOfBirth.HasValue)
            {
                var dob = model.DateOfBirth.Value.Date;
                var join = model.JoiningDate.Value.Date;
                if (join < dob)
                {
                    ModelState.AddModelError("JoiningDate", "Joining Date cannot be earlier than Date of Birth!");
                    return View(model);
                }

                var minJoinDate = dob.AddYears(21);
                if (join < minJoinDate)
                {
                    ModelState.AddModelError("JoiningDate", "Teacher must be at least 21 years old at joining!");
                    return View(model);
                }
            }

            if (ModelState.IsValid)
            {
                // If password is not provided, use Teacher ID as default password
                if (string.IsNullOrWhiteSpace(model.Password))
                {
                    model.Password = model.TeacherId;
                }

                await _service.Add(model);
                return RedirectToAction("List");
            }

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
            var departments = _deptRepo.GetAll();
            ViewBag.Departments = new SelectList(departments, "DepartmentId", "DepartmentName", model.DepartmentId);

            // Validate Email format
            if (!ValidationHelper.IsValidEmail(model.Email))
            {
                ModelState.AddModelError("Email", "Please enter a valid email address (e.g., example@gmail.com)!");
                return View(model);
            }

            // Check for duplicate Email (excluding current teacher)
            var existingEmail = _repo.GetAll()
                .FirstOrDefault(t => t.Email?.ToLower() == model.Email?.ToLower() && t.TeacherId != model.TeacherId);
            if (existingEmail != null)
            {
                ModelState.AddModelError("Email", "This email address is already registered in the system!");
                return View(model);
            }

            // Validate Mobile format (if provided)
            if (!string.IsNullOrWhiteSpace(model.Mobile) && !ValidationHelper.IsValidPhoneNumber(model.Mobile))
            {
                ModelState.AddModelError("Mobile", "Mobile number must be 10-11 digits (e.g., 0123456789)!");
                return View(model);
            }

            // Validate Date of Birth
            if (model.DateOfBirth.HasValue && !ValidationHelper.IsValidDateOfBirth(model.DateOfBirth))
            {
                var today = DateTime.Now;
                if (model.DateOfBirth > today)
                {
                    ModelState.AddModelError("DateOfBirth", "Date of birth cannot be in the future!");
                }
                else
                {
                    ModelState.AddModelError("DateOfBirth", "Teacher must be at least 5 years old!");
                }
                return View(model);
            }

            // Validate Joining Date relative to Date of Birth (must be >= DOB and age >= 21 at join)
            if (model.JoiningDate.HasValue && model.DateOfBirth.HasValue)
            {
                var dob = model.DateOfBirth.Value.Date;
                var join = model.JoiningDate.Value.Date;
                if (join < dob)
                {
                    ModelState.AddModelError("JoiningDate", "Joining Date cannot be earlier than Date of Birth!");
                    return View(model);
                }

                var minJoinDate = dob.AddYears(21);
                if (join < minJoinDate)
                {
                    ModelState.AddModelError("JoiningDate", "Teacher must be at least 21 years old at joining!");
                    return View(model);
                }
            }

            if (ModelState.IsValid)
            {
                // If password is not provided, use Teacher ID as default password
                if (string.IsNullOrWhiteSpace(model.Password))
                {
                    model.Password = model.TeacherId;
                }

                await _service.Update(model);
                return RedirectToAction("List");
            }

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