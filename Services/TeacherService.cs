using Microsoft.AspNetCore.Hosting;
using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Models;

namespace SIMS_FPT.Services
{
    public class TeacherService
    {
        private readonly ITeacherRepository _repo;
        private readonly IUserRepository _userRepo;
        private readonly IWebHostEnvironment _env;

        public TeacherService(ITeacherRepository repo,
                              IUserRepository userRepo,
                              IWebHostEnvironment env)
        {
            _repo = repo;
            _userRepo = userRepo;
            _env = env;
        }

        public async Task<string?> UploadImage(TeacherCSVModel m)
        {
            if (m.TeacherImageFile == null) return null;

            string folder = Path.Combine(_env.WebRootPath, "images");

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string unique = Guid.NewGuid() + "_" + m.TeacherImageFile.FileName;
            string path = Path.Combine(folder, unique);

            using (var fs = new FileStream(path, FileMode.Create))
            {
                await m.TeacherImageFile.CopyToAsync(fs);
            }

            return "/images/" + unique;
        }

        public async Task Add(TeacherCSVModel model)
        {
            if (string.IsNullOrWhiteSpace(model.TeacherId))
                model.TeacherId = "TCH" + DateTime.UtcNow.Ticks.ToString().Substring(10);

            model.ImagePath = await UploadImage(model)
                               ?? "/assets/img/profiles/avatar-02.jpg";

            _repo.Add(model);

            _userRepo.AddTeacherUser(model);
        }

        public async Task Update(TeacherCSVModel model, string originalUsername)
        {
            if (model.TeacherImageFile != null)
                model.ImagePath = await UploadImage(model);

            _repo.Update(model);

            _userRepo.UpdateUserFromTeacher(model, originalUsername);
        }

        public void Delete(string id)
        {
            var teacher = _repo.GetById(id);
            if (teacher == null) return;

            _repo.Delete(id);
            _userRepo.DeleteUserByUsername(teacher.Username);
        }
    }
}
