using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Models;
using SIMS_FPT.Services;
using System.Threading.Tasks;

namespace SIMS_FPT.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class StudentController : Controller
    {
        private readonly IStudentRepository _repo;
        private readonly StudentService _service;

        public StudentController(IStudentRepository repo, StudentService service)
        {
            _repo = repo;
            _service = service;
        }

        public IActionResult List()
        {
            return View(_repo.GetAll());
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(StudentCSVModel model)
        {
            await _service.Add(model);
            return RedirectToAction("List");
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
            await _service.Update(model);
            return RedirectToAction("List");
        }

        public IActionResult DeleteStudent(string id)
        {
            _repo.Delete(id);
            return RedirectToAction("List");
        }

        public IActionResult Detail(string id)
        {
            var student = _repo.GetById(id);
            if (student == null) return NotFound();

            return View(student);
        }
    }
}
