using Microsoft.AspNetCore.Mvc;
using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Models;
using System.Linq;

namespace SIMS_FPT.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ClassController : Controller
    {
        private readonly IClassRepository _classRepository;
        private readonly ISubjectRepository _subjectRepository;
        private readonly ITeacherRepository _teacherRepository;

        public ClassController(IClassRepository classRepository, ISubjectRepository subjectRepository, ITeacherRepository teacherRepository)
        {
            _classRepository = classRepository;
            _subjectRepository = subjectRepository;
            _teacherRepository = teacherRepository;
        }

        // GET: Admin/Class/List
        // GET: Admin/Class/List
        public IActionResult List()
        {
            var classes = _classRepository.GetAllClasses();

            // [THÊM MỚI] Lấy danh sách giáo viên và gửi sang View qua ViewBag
            ViewBag.Teachers = _teacherRepository.GetAll().ToList();

            return View(classes);
        }

        // GET: Admin/Class/Add
        public IActionResult Add()
        {
            ViewBag.Subjects = _subjectRepository.GetAll().ToList();
            ViewBag.Teachers = _teacherRepository.GetAll().ToList();
            return View();
        }

        // POST: Admin/Class/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(Class @class)
        {
            // ===> SỬA Ở ĐÂY: Bỏ qua kiểm tra các thuộc tính điều hướng <===
            ModelState.Remove("Subject");
            ModelState.Remove("Teacher");
            ModelState.Remove("Enrollments"); // Bỏ qua danh sách sinh viên nếu có

            if (ModelState.IsValid)
            {
                _classRepository.AddClass(@class);
                return RedirectToAction(nameof(List));
            }

            // Nếu vẫn lỗi, load lại dữ liệu cho Dropdown
            ViewBag.Subjects = _subjectRepository.GetAll().ToList();
            ViewBag.Teachers = _teacherRepository.GetAll().ToList();
            return View(@class);
        }

        // GET: Admin/Class/Edit/5
        public IActionResult Edit(int id)
        {
            var @class = _classRepository.GetClassById(id);
            if (@class == null)
            {
                return NotFound();
            }
            ViewBag.Subjects = _subjectRepository.GetAll().ToList();
            ViewBag.Teachers = _teacherRepository.GetAll().ToList();
            return View(@class);
        }

        // POST: Admin/Class/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Class @class)
        {
            if (id != @class.Id)
            {
                return NotFound();
            }

            // ===> SỬA Ở ĐÂY: Bỏ qua kiểm tra các thuộc tính điều hướng <===
            ModelState.Remove("Subject");
            ModelState.Remove("Teacher");
            ModelState.Remove("Enrollments");

            if (ModelState.IsValid)
            {
                _classRepository.UpdateClass(@class);
                return RedirectToAction(nameof(List));
            }
            ViewBag.Subjects = _subjectRepository.GetAll().ToList();
            ViewBag.Teachers = _teacherRepository.GetAll().ToList();
            return View(@class);
        }

        // POST: Admin/Class/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            _classRepository.DeleteClass(id);
            return RedirectToAction(nameof(List));
        }
    }
}