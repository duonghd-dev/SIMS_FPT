using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Models;
using System.Collections.Generic;
using System.Linq;

namespace SIMS_FPT.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class DepartmentController : Controller
    {
        private readonly IDepartmentRepository _deptRepo;
        private readonly ITeacherRepository _teacherRepo;
        private readonly ISubjectRepository _subjectRepo;

        // Cập nhật Constructor để nhận thêm TeacherRepo
        public DepartmentController(IDepartmentRepository deptRepo, ITeacherRepository teacherRepo, ISubjectRepository subjectRepo)
        {
            _deptRepo = deptRepo;
            _teacherRepo = teacherRepo;
            _subjectRepo = subjectRepo;
        }




        public IActionResult List()
        {
            return View(_deptRepo.GetAll());
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Add(DepartmentModel model)
        {
            // Check trùng ID
            if (_deptRepo.GetById(model.DepartmentId) != null)
            {
                ModelState.AddModelError("DepartmentId", "Department ID already exists!");
                return View(model);
            }

            if (ModelState.IsValid)
            {
                _deptRepo.Add(model);
                return RedirectToAction("List");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Edit(string id)
        {
            var item = _deptRepo.GetById(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost]
        public IActionResult Edit(DepartmentModel model)
        {
            if (ModelState.IsValid)
            {
                _deptRepo.Update(model);
                return RedirectToAction("List");
            }
            return View(model);
        }

        public IActionResult Delete(string id)
        {
            _deptRepo.Delete(id);
            return RedirectToAction("List");
        }

        // --- TÍNH NĂNG MỚI: Xem chi tiết Khoa & Danh sách Giảng viên ---
        public IActionResult Detail(string id)
        {
            var dept = _deptRepo.GetById(id);
            if (dept == null) return NotFound();

            var deptTeachers = _teacherRepo.GetAll().Where(t => t.DepartmentId == id).ToList();
            var deptSubjects = _subjectRepo.GetAll().Where(s => s.DepartmentId == id).ToList();
            var viewModel = new DepartmentDetailViewModel
            {
                Department = dept,
                Teachers = deptTeachers,
                Subjects = deptSubjects
            };
            return View(viewModel);
        }
    }
}