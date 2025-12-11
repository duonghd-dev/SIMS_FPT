using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SIMS_FPT.Services
{
    public class TeacherService
    {
        private readonly ITeacherRepository _repo;
        private readonly IUserRepository _userRepo;
        private readonly IWebHostEnvironment _env;

        public TeacherService(ITeacherRepository repo, IUserRepository userRepo, IWebHostEnvironment env)
        {
            _repo = repo;
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

        public async Task Add(TeacherCSVModel model)
        {
            // 1. Upload image
            model.ImagePath = (model.TeacherImageFile != null)
                ? await UploadImage(model.TeacherImageFile)
                : "/assets/img/profiles/avatar-02.jpg";

            // 2. Save Teacher to CSV
            _repo.Add(model);

            // 3. AUTOMATICALLY CREATE USER
            if (!string.IsNullOrEmpty(model.Email) && !_userRepo.UsernameExists(model.Email))
            {
                var newUser = new Users
                {
                    Email = model.Email,
                    Password = model.TeacherId, // Default password is Teacher ID
                    FullName = model.Name,
                    Role = "Instructor",
                    LinkedId = model.TeacherId,
                    HashAlgorithm = "Plain" // <--- ADD THIS LINE to trigger hashing in the repo
                };
                _userRepo.AddUser(newUser);
            }
        }

        public async Task Update(TeacherCSVModel model)
        {
            if (model.TeacherImageFile != null)
                model.ImagePath = await UploadImage(model.TeacherImageFile);

            _repo.Update(model);

            // (Optional) Nếu muốn update cả Email bên bảng User khi sửa Teacher thì viết thêm logic ở đây
        }

        public void Delete(string id)
        {
            // Lấy thông tin GV trước khi xóa để lấy email xóa tài khoản
            var teacher = _repo.GetById(id);
            _repo.Delete(id);

            if (teacher != null && !string.IsNullOrEmpty(teacher.Email))
            {
                _userRepo.DeleteUserByUsername(teacher.Email);
            }
        }
    }
}