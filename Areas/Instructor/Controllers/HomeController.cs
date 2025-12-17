using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Helpers;
using SIMS_FPT.Models;
using SIMS_FPT.Models.ViewModels;
using SIMS_FPT.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SIMS_FPT.Areas.Instructor.Controllers
{
    [Area("Instructor")]
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IInstructorDashboardService _dashboardService;
        private readonly ITeacherRepository _teacherRepo;
        private readonly IUserRepository _userRepo;
        private readonly ISubjectRepository _subjectRepo;
        private readonly IWebHostEnvironment _env;

        public HomeController(
            IInstructorDashboardService dashboardService,
            ITeacherRepository teacherRepo,
            IUserRepository userRepo,
            ISubjectRepository subjectRepo,
            IWebHostEnvironment env)
        {
            _dashboardService = dashboardService;
            _teacherRepo = teacherRepo;
            _userRepo = userRepo;
            _subjectRepo = subjectRepo;
            _env = env;
        }

        private string CurrentTeacherId
        {
            get
            {
                var linkedId = User.FindFirst("LinkedId")?.Value;
                if (!string.IsNullOrEmpty(linkedId)) return linkedId;
                return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity?.Name ?? "UNKNOWN";
            }
        }

        public IActionResult Dashboard(string? studentId = null)
        {
            var teacherId = CurrentTeacherId;
            var model = _dashboardService.GetDashboard(teacherId, studentId);

            // Set ViewBag for header avatar
            var teacher = _teacherRepo.GetById(teacherId);
            if (teacher != null)
            {
                ViewBag.ProfileImage = teacher.ImagePath;
            }

            return View(model);
        }

  

        [HttpGet]
        public IActionResult Profile()
        {
            var teacherId = CurrentTeacherId;
            var teacher = _teacherRepo.GetById(teacherId);
            if (teacher == null) return NotFound("Teacher profile not found.");

            var user = _userRepo.GetByLinkedId(teacherId);

            var model = new InstructorProfileViewModel
            {
                TeacherId = teacher.TeacherId,
                FullName = teacher.Name,
                Email = teacher.Email,
                Mobile = teacher.Mobile,
                Address = teacher.Address,
                City = teacher.City,
                Country = teacher.Country,
                ExistingImagePath = teacher.ImagePath
            };

            ViewBag.ProfileImage = teacher.ImagePath;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Profile(InstructorProfileViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var teacherId = CurrentTeacherId;
            if (model.TeacherId != teacherId) return Forbid();

            var teacher = _teacherRepo.GetById(teacherId);
            var user = _userRepo.GetByLinkedId(teacherId);
            if (teacher == null) return NotFound();

            teacher.Name = model.FullName ?? "";
            teacher.Email = model.Email ?? "";
            teacher.Mobile = model.Mobile ?? "";
            teacher.Address = model.Address ?? "";
            teacher.City = model.City ?? "";
            teacher.Country = model.Country ?? "";

            // Handle Image Upload
            if (model.ProfileImage != null)
            {
                var folder = Path.Combine(_env.WebRootPath, "assets/img/profiles");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                var fileName = $"{teacherId}_{Guid.NewGuid()}{Path.GetExtension(model.ProfileImage.FileName)}";
                var filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    model.ProfileImage.CopyTo(stream);
                }
                teacher.ImagePath = $"~/assets/img/profiles/{fileName}";
            }

            bool passwordChanged = !string.IsNullOrEmpty(model.NewPassword);
            if (passwordChanged)
            {
                teacher.Password = model.NewPassword;
            }

            _teacherRepo.Update(teacher);

            if (user != null)
            {
                user.FullName = teacher.Name ?? "";
                user.Email = teacher.Email ?? "";

                if (passwordChanged && !string.IsNullOrEmpty(model.NewPassword))
                {
                    user.Password = PasswordHasherHelper.Hash(model.NewPassword);
                    user.HashAlgorithm = "PBKDF2";
                }
                _userRepo.Update(user);
            }

            TempData["Success"] = "Profile updated successfully.";
            model.ExistingImagePath = teacher.ImagePath;
            ViewBag.ProfileImage = teacher.ImagePath;

            return View(model);
        }

        private List<ClassScheduleItem> BuildTodayClasses()
        {
            var subjects = _subjectRepo.GetAll().Take(3).ToList();
            return subjects.Select((s, index) => new ClassScheduleItem
            {
                SubjectName = s.SubjectName ?? "Unknown",
                Time = $"{8 + (index * 2)}:00 AM - {10 + (index * 2)}:00 AM",
                Room = $"Room A-{101 + index}",
                ClassName = s.SubjectId ?? "N/A"
            }).ToList();
        }
    }
}
