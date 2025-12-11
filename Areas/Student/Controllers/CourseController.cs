using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Models;
using System;
using System.Linq;
using System.Security.Claims;

namespace SIMS_FPT.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "Student")] // Chỉ sinh viên mới vào được
    public class CourseController : Controller
    {
        private readonly IClassRepository _classRepo;
        private readonly IEnrollmentRepository _enrollmentRepo;
        private readonly ISubjectRepository _subjectRepo; // Để hiển thị tên môn học nếu cần

        public CourseController(IClassRepository classRepo, IEnrollmentRepository enrollmentRepo, ISubjectRepository subjectRepo)
        {
            _classRepo = classRepo;
            _enrollmentRepo = enrollmentRepo;
            _subjectRepo = subjectRepo;
        }

        // 1. Danh sách các lớp có thể đăng ký
        public IActionResult AvailableCourses()
        {
            // Lấy ID của sinh viên đang đăng nhập (User ID)
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int userId = int.Parse(userIdStr);

            // Lấy tất cả các lớp
            var allClasses = _classRepo.GetAllClasses();
            
            // Lấy danh sách môn học để hiển thị tên môn (Mapping Subject)
            var subjects = _subjectRepo.GetAll();
            foreach(var c in allClasses)
            {
                c.Subject = subjects.FirstOrDefault(s => s.SubjectId == c.SubjectId);
            }

            // (Tuỳ chọn) Loại bỏ các lớp đã đăng ký rồi để không hiện nút đăng ký nữa
            // Hoặc xử lý ở View
            ViewBag.EnrolledClassIds = _enrollmentRepo.GetEnrollmentsByStudentId(userId)
                                                      .Select(e => e.ClassId)
                                                      .ToList();

            return View(allClasses);
        }

        // 2. Xử lý Đăng ký lớp (Enroll)
        [HttpPost]
        public IActionResult Enroll(int classId)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login", "Login", new { area = "" });

            int userId = int.Parse(userIdStr);

            // Kiểm tra xem đã đăng ký chưa
            if (_enrollmentRepo.IsEnrolled(userId, classId))
            {
                TempData["Error"] = "Bạn đã tham gia lớp này rồi!";
                return RedirectToAction("AvailableCourses");
            }

            // Tạo mới Enrollment
            var newEnrollment = new Enrollment
            {
                ClassId = classId,
                StudentId = userId,
                EnrollmentDate = DateTime.Now,
                Grade = null // Chưa có điểm
            };

            _enrollmentRepo.AddEnrollment(newEnrollment);

            TempData["Success"] = "Đăng ký lớp học thành công!";
            return RedirectToAction("MyCourses");
        }

        // 3. Xem danh sách lớp CỦA TÔI (kèm điểm số)
        public IActionResult MyCourses()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int userId = int.Parse(userIdStr);

            // Lấy danh sách Enrollment của sinh viên này
            var myEnrollments = _enrollmentRepo.GetEnrollmentsByStudentId(userId);

            // Map thông tin Class vào Enrollment để hiển thị tên lớp
            var classes = _classRepo.GetAllClasses();
            var subjects = _subjectRepo.GetAll();

            foreach (var item in myEnrollments)
            {
                var cls = classes.FirstOrDefault(c => c.Id == item.ClassId);
                if (cls != null)
                {
                    // Map môn học vào lớp
                    cls.Subject = subjects.FirstOrDefault(s => s.SubjectId == cls.SubjectId);
                    item.Class = cls;
                }
            }

            return View(myEnrollments);
        }
    }
}