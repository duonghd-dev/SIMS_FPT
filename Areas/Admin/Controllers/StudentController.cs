using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIMS_FPT.Data.Interfaces;
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
            // Kiểm tra trùng ID
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