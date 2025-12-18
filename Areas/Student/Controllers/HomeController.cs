using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Models;
using SIMS_FPT.Models.ViewModels;
using SIMS_FPT.Helpers;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.IO;
using System;

namespace SIMS_FPT.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "Student")]
    public class HomeController : Controller
    {
        private readonly IStudentRepository _studentRepo;
        private readonly IUserRepository _userRepo;

        public HomeController(IStudentRepository studentRepo, IUserRepository userRepo)
        {
            _studentRepo = studentRepo;
            _userRepo = userRepo;
        }

        public IActionResult Dashboard()
        {
            return View();
        }

        // [GET] Hiển thị Profile
        public IActionResult Profile()
        {
            var studentId = User.FindFirst("LinkedId")?.Value;
            if (string.IsNullOrEmpty(studentId)) return RedirectToAction("Login", "Login", new { area = "" });

            var student = _studentRepo.GetById(studentId);
            if (student == null) return NotFound();

            // [SỬA LỖI] Dùng GetByLinkedId thay vì GetByUsername
            var user = _userRepo.GetByLinkedId(studentId);

            var model = new StudentProfileViewModel
            {
                StudentId = student.StudentId,
                FullName = student.FullName, // Property này có sẵn trong StudentCSVModel (computed)
                Email = student.Email,

                // [SỬA LỖI] StudentCSVModel dùng DateOfBirth, không phải DOB
                DOB = student.DateOfBirth,
                Gender = student.Gender,

                // [SỬA LỖI] StudentCSVModel KHÔNG CÓ PhoneNumber và Address -> Để null
                PhoneNumber = null,
                Address = null,

                // [SỬA LỖI] Ảnh lưu ở Student (ImagePath), không phải User
                AvatarPath = student.ImagePath
            };

            return View(model);
        }

        // [POST] Cập nhật Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Profile(StudentProfileViewModel model, IFormFile? avatarFile)
        {
            var studentId = User.FindFirst("LinkedId")?.Value;
            if (string.IsNullOrEmpty(studentId)) return RedirectToAction("Login", "Login", new { area = "" });

            var student = _studentRepo.GetById(studentId);
            if (student == null) return NotFound();

            // 1. Cập nhật Avatar nếu có
            if (avatarFile != null && avatarFile.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(avatarFile.FileName);
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", fileName);
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    avatarFile.CopyTo(stream);
                }

                // [SỬA LỖI] Lưu vào StudentCSVModel.ImagePath
                student.ImagePath = "/images/" + fileName;
                model.AvatarPath = student.ImagePath;

                // Cập nhật Student (vì ảnh nằm trong Student)
                _studentRepo.Update(student);
            }
            else
            {
                model.AvatarPath = student.ImagePath;
            }

            // 2. Xử lý đổi mật khẩu 
            if (!string.IsNullOrEmpty(model.CurrentPassword))
            {
                // [SỬA LỖI] Dùng GetByLinkedId
                var userObj = _userRepo.GetByLinkedId(studentId);
                if (userObj != null)
                {
                    // [SỬA LỖI] Dùng Verify (không phải VerifyPassword), và thuộc tính Password (không phải PasswordHash)
                    if (!PasswordHasherHelper.Verify(model.CurrentPassword, userObj.Password))
                    {
                        ModelState.AddModelError("CurrentPassword", "Incorrect current password.");
                    }
                    else if (string.IsNullOrEmpty(model.NewPassword))
                    {
                        ModelState.AddModelError("NewPassword", "New password is required.");
                    }
                    else
                    {
                        // [SỬA LỖI] Dùng Hash (không phải HashPassword), lưu vào Password
                        userObj.Password = PasswordHasherHelper.Hash(model.NewPassword);
                        _userRepo.Update(userObj);
                        TempData["SuccessPass"] = "Password changed successfully!";
                    }
                }
            }

            // 3. Cập nhật thông tin sinh viên
            // Bỏ qua validate PhoneNumber và Address vì model không lưu được
            ModelState.Remove("PhoneNumber");
            ModelState.Remove("Address");

            if (ModelState.IsValid)
            {
                // [SỬA LỖI] Tách FullName thành FirstName/LastName để lưu vào StudentCSVModel
                if (!string.IsNullOrEmpty(model.FullName))
                {
                    var names = model.FullName.Trim().Split(' ', 2);
                    student.FirstName = names[0];
                    if (names.Length > 1) student.LastName = names[1];
                    else student.LastName = "";
                }

                // [SỬA LỖI] DateOfBirth thay vì DOB
                student.DateOfBirth = model.DOB ?? student.DateOfBirth;
                student.Gender = model.Gender ?? "Other";

                // Lưu ý: Không update PhoneNumber, Address vì không có trong DB

                _studentRepo.Update(student);
                TempData["SuccessInfo"] = "Profile updated successfully!";
                return RedirectToAction("Profile");
            }

            return View(model);
        }
    }
}