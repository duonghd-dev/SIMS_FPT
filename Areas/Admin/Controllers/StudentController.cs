using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Models;
using SIMS_FPT.Services;
using System;
using System.Linq; // Cần thiết để dùng Where
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

        // SỬA: Thêm tham số className để lọc
        public IActionResult List(string className)
        {
            var data = _repo.GetAll();

            // Logic lọc theo lớp (hoặc tên)
            if (!string.IsNullOrEmpty(className))
            {
                // Chuyển về chữ thường để tìm không phân biệt hoa/thường
                className = className.Trim().ToLower();

                data = data.Where(s =>
                    (s.ClassName != null && s.ClassName.ToLower().Contains(className)) ||
                    (s.StudentId != null && s.StudentId.ToLower().Contains(className)) ||
                    (s.FullName != null && s.FullName.ToLower().Contains(className))
                ).ToList();

                // Lưu lại từ khóa để hiển thị lại trên ô tìm kiếm
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
            var existing = _repo.GetById(model.StudentId);
            if (existing != null)
            {
                ModelState.AddModelError("StudentId", "Student ID already exists!");
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