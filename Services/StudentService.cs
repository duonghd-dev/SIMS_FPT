using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SIMS_FPT.Services
{
    public class StudentService
    {
        private readonly IStudentRepository _studentRepo;
        private readonly IUserRepository _userRepo;
        private readonly IWebHostEnvironment _env;

        public StudentService(IStudentRepository studentRepo, IUserRepository userRepo, IWebHostEnvironment env)
        {
            _studentRepo = studentRepo;
            _userRepo = userRepo;
            _env = env;
        }

        public async Task<string?> UploadImage(IFormFile? file)
        {
            if (file == null || file.Length == 0) return null;
            string folder = Path.Combine(_env.WebRootPath, "images");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            string unique = Guid.NewGuid() + "_" + file.FileName;
            string path = Path.Combine(folder, unique);
            using (var fs = new FileStream(path, FileMode.Create)) await file.CopyToAsync(fs);
            return "/images/" + unique;
        }

        public async Task Add(StudentCSVModel model)
        {
            // 1. Upload ảnh
            model.ImagePath = (model.StudentImageFile != null)
                ? await UploadImage(model.StudentImageFile)
                : "/assets/img/profiles/avatar-01.jpg";

            // 2. Lưu Student
            _studentRepo.Add(model);

            // 3. TẠO USER (Dùng Email để đăng nhập)
            if (!string.IsNullOrEmpty(model.Email) && !_userRepo.UsernameExists(model.Email))
            {
                var newUser = new Users
                {
                    Email = model.Email,        // Email chính
                    Password = model.StudentId, // Mật khẩu mặc định là Mã SV
                    FullName = model.FullName,
                    Role = "Student",
                    LinkedId = model.StudentId
                };
                _userRepo.AddUser(newUser);
            }
        }

        public async Task Update(StudentCSVModel model)
        {
            if (model.StudentImageFile != null)
                model.ImagePath = await UploadImage(model.StudentImageFile);
            _studentRepo.Update(model);
        }
    }
}