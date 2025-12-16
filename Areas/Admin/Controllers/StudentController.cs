using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Helpers;
using SIMS_FPT.Models;
using SIMS_FPT.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SIMS_FPT.Areas.Admin.Controllers
{
    [Area("Admin")]
    // [Authorize(Roles = "Admin")]
    public class StudentController : Controller
    {
        private readonly IStudentRepository _repo;
        private readonly StudentService _service;

        public StudentController(IStudentRepository repo, StudentService service)
        {
            _repo = repo;
            _service = service;
        }

        public IActionResult List(string className)
        {
            var data = _repo.GetAll();

            if (!string.IsNullOrEmpty(className))
            {
                var keyword = className.Trim().ToLower();

                data = data.Where(s =>
                    (s.StudentId != null && s.StudentId.ToLower().Contains(keyword)) ||
                    (s.FullName != null && s.FullName.ToLower().Contains(keyword))
                ).ToList();

                ViewBag.SearchTerm = className;
            }

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
            // Validate Student ID format
            if (!ValidationHelper.IsValidId(model.StudentId))
            {
                ModelState.AddModelError("StudentId", "Student ID must be 3-20 alphanumeric characters!");
                return View(model);
            }

            // Check for duplicate Student ID
            var existingStudent = _repo.GetById(model.StudentId);
            if (existingStudent != null)
            {
                ModelState.AddModelError("StudentId", "Student ID already exists in the system!");
                return View(model);
            }

            // Validate Email format
            if (!ValidationHelper.IsValidEmail(model.Email))
            {
                ModelState.AddModelError("Email", "Please enter a valid email address (e.g., example@gmail.com)!");
                return View(model);
            }

            // Check for duplicate Email
            var existingEmail = _repo.GetAll().FirstOrDefault(s => s.Email?.ToLower() == model.Email?.ToLower());
            if (existingEmail != null)
            {
                ModelState.AddModelError("Email", "This email address is already registered in the system!");
                return View(model);
            }

            // Validate Date of Birth
            if (!ValidationHelper.IsValidDateOfBirth(model.DateOfBirth))
            {
                var today = DateTime.Now;
                if (model.DateOfBirth > today)
                {
                    ModelState.AddModelError("DateOfBirth", "Date of birth cannot be in the future!");
                }
                else
                {
                    ModelState.AddModelError("DateOfBirth", "Student must be at least 5 years old!");
                }
                return View(model);
            }

            if (ModelState.IsValid)
            {
                await _service.Add(model);
                return RedirectToAction("List");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Edit(string id)
        {
            var student = _repo.GetById(id);
            if (student == null) return NotFound();
            return View(student);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(StudentCSVModel model)
        {
            // Validate Email format
            if (!ValidationHelper.IsValidEmail(model.Email))
            {
                ModelState.AddModelError("Email", "Please enter a valid email address (e.g., example@gmail.com)!");
                return View(model);
            }

            // Check for duplicate Email (excluding current student)
            var existingEmail = _repo.GetAll()
                .FirstOrDefault(s => s.Email?.ToLower() == model.Email?.ToLower() && s.StudentId != model.StudentId);
            if (existingEmail != null)
            {
                ModelState.AddModelError("Email", "This email address is already registered in the system!");
                return View(model);
            }

            // Validate Date of Birth
            if (!ValidationHelper.IsValidDateOfBirth(model.DateOfBirth))
            {
                var today = DateTime.Now;
                if (model.DateOfBirth > today)
                {
                    ModelState.AddModelError("DateOfBirth", "Date of birth cannot be in the future!");
                }
                else
                {
                    ModelState.AddModelError("DateOfBirth", "Student must be at least 5 years old!");
                }
                return View(model);
            }

            if (ModelState.IsValid)
            {
                await _service.Update(model);
                return RedirectToAction("List");
            }
            return View(model);
        }

        public IActionResult Detail(string id)
        {
            var student = _repo.GetById(id);
            if (student == null) return NotFound();
            return View(student);
        }

        public IActionResult DeleteStudent(string id)
        {
            _repo.Delete(id);
            return RedirectToAction("List");
        }
    }
}