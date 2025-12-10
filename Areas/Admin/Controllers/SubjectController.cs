using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Models;
using System.Linq;

namespace SIMS_FPT.Areas.Admin.Controllers
{
    [Area("Admin")]
    // [Authorize(Roles = "Admin")]
    public class SubjectController : Controller
    {
        private readonly ISubjectRepository _repo;
        private readonly IDepartmentRepository _deptRepo; // Thêm DeptRepo

        public SubjectController(ISubjectRepository repo, IDepartmentRepository deptRepo)
        {
            _repo = repo;
            _deptRepo = deptRepo;
        }

        public IActionResult List()
        {
            return View(_repo.GetAll());
        }

        [HttpGet]
        public IActionResult Add()
        {
            // Load danh sách Khoa vào Dropdown
            ViewBag.Departments = new SelectList(_deptRepo.GetAll(), "DepartmentId", "DepartmentName");
            return View();
        }

        [HttpPost]
        public IActionResult Add(SubjectModel model)
        {
            // Kiểm tra trùng ID
            if (_repo.GetById(model.SubjectId) != null)
            {
                ModelState.AddModelError("SubjectId", "Subject ID already exists!");
                ViewBag.Departments = new SelectList(_deptRepo.GetAll(), "DepartmentId", "DepartmentName");
                return View(model);
            }

            if (ModelState.IsValid)
            {
                _repo.Add(model);
                return RedirectToAction("List");
            }

            ViewBag.Departments = new SelectList(_deptRepo.GetAll(), "DepartmentId", "DepartmentName");
            return View(model);
        }

        [HttpGet]
        public IActionResult Edit(string id)
        {
            var item = _repo.GetById(id);
            if (item == null) return NotFound();

            // Load danh sách Khoa, chọn sẵn khoa hiện tại của môn học
            ViewBag.Departments = new SelectList(_deptRepo.GetAll(), "DepartmentId", "DepartmentName", item.DepartmentId);
            return View(item);
        }

        [HttpPost]
        public IActionResult Edit(SubjectModel model)
        {
            if (ModelState.IsValid)
            {
                _repo.Update(model);
                return RedirectToAction("List");
            }

            ViewBag.Departments = new SelectList(_deptRepo.GetAll(), "DepartmentId", "DepartmentName", model.DepartmentId);
            return View(model);
        }

        public IActionResult Delete(string id)
        {
            _repo.Delete(id);
            return RedirectToAction("List");
        }
    }
}