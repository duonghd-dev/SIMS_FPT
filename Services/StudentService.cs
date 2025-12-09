using Microsoft.AspNetCore.Hosting;
using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SIMS_FPT.Services
{
    public class StudentService
    {
        private readonly IStudentRepository _repo;
        private readonly IWebHostEnvironment _env;

        public StudentService(IStudentRepository repo, IWebHostEnvironment env)
        {
            _repo = repo;
            _env = env;
        }

        public Task<string> UploadImage(StudentCSVModel model)
        {
            if (model.StudentImageFile == null) return Task.FromResult<string>(null);

            string folder = Path.Combine(_env.WebRootPath, "images");

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string unique = Guid.NewGuid() + "_" + model.StudentImageFile.FileName;
            string path = Path.Combine(folder, unique);

            using (var fs = new FileStream(path, FileMode.Create))
            {
                model.StudentImageFile.CopyTo(fs);
            }

            return Task.FromResult("/images/" + unique);
        }

        public async Task Add(StudentCSVModel model)
        {
            model.ImagePath = await UploadImage(model) ??
                              "/assets/img/profiles/avatar-01.jpg";

            _repo.Add(model);
        }

        public async Task Update(StudentCSVModel model)
        {
            if (model.StudentImageFile != null)
                model.ImagePath = await UploadImage(model);

            _repo.Update(model);
        }
    }
}
