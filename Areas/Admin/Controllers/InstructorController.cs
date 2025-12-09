using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Models;
using SIMS_FPT.Services;

namespace SIMS_FPT.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class InstructorController : Controller
    {
        private readonly ITeacherRepository _repo;
        private readonly TeacherService _service;

        public InstructorController(ITeacherRepository repo, TeacherService service)
        {
            _repo = repo;
            _service = service;
        }

        public IActionResult List()
        {
            return View(_repo.GetAll());
        }

        [HttpGet]
        public IActionResult Add() => View();

        [HttpPost]
        public async Task<IActionResult> Add(TeacherCSVModel model)
        {
            await _service.Add(model);
            return RedirectToAction("List");
        }

        [HttpGet]
        public IActionResult Edit(string id)
        {
            var teacher = _repo.GetById(id);
            if (teacher == null) return NotFound();
            return View(teacher);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(TeacherCSVModel model, string originalUsername)
        {
            await _service.Update(model, originalUsername);
            return RedirectToAction("List");
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
            return View(teacher);
        }
    }
}
