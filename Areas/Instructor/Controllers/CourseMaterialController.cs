using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Models;
using System;
using System.IO;
using System.Linq;

namespace SIMS_FPT.Areas.Instructor.Controllers
{
    [Area("Instructor")]
    [Authorize]
    public class CourseMaterialController : Controller
    {
        private readonly ICourseMaterialRepository _materialRepo;
        private readonly ISubjectRepository _subjectRepo;
        private readonly IWebHostEnvironment _env;

        public CourseMaterialController(ICourseMaterialRepository materialRepo,
                                        ISubjectRepository subjectRepo,
                                        IWebHostEnvironment env)
        {
            _materialRepo = materialRepo;
            _subjectRepo = subjectRepo;
            _env = env;
        }

        public IActionResult Index()
        {
            var materials = _materialRepo.GetAll()
                .OrderByDescending(m => m.UploadDate)
                .ToList();
            ViewBag.Subjects = _subjectRepo.GetAll();
            return View(materials);
        }

        public IActionResult Create()
        {
            ViewBag.Subjects = _subjectRepo.GetAll();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CourseMaterialModel model, IFormFile uploadFile)
        {
            ModelState.Remove("MaterialId"); // generated server-side
            ModelState.Remove("UploadDate"); // set server-side

            if (uploadFile == null && string.IsNullOrWhiteSpace(model.VideoUrl))
            {
                ModelState.AddModelError(string.Empty, "Please upload a file or provide a video link.");
            }

            if (uploadFile != null)
            {
                var ext = Path.GetExtension(uploadFile.FileName).ToLowerInvariant();
                var allowed = new[] { ".pdf", ".ppt", ".pptx" };
                if (!allowed.Contains(ext))
                {
                    ModelState.AddModelError(string.Empty, "Only PDF, PPT, or PPTX files are allowed.");
                }
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Subjects = _subjectRepo.GetAll();
                TempData["Error"] = "Please fix the errors below and try again.";
                return View(model);
            }

            if (uploadFile != null)
            {
                var dir = Path.Combine(_env.WebRootPath, "materials", model.SubjectId ?? "general");
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(uploadFile.FileName)}";
                var filePath = Path.Combine(dir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    uploadFile.CopyTo(stream);
                }

                model.FilePath = Path.Combine("materials", model.SubjectId ?? "general", fileName).Replace("\\", "/");
            }

            model.UploadDate = DateTime.Now;
            if (string.IsNullOrEmpty(model.MaterialId)) model.MaterialId = Guid.NewGuid().ToString();
            try
            {
                _materialRepo.Add(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Save failed: {ex.Message}";
                ViewBag.Subjects = _subjectRepo.GetAll();
                return View(model);
            }
            TempData["Success"] = "Material saved successfully.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Delete(string id)
        {
            var mat = _materialRepo.GetById(id);
            if (mat != null && !string.IsNullOrEmpty(mat.FilePath))
            {
                var phys = Path.Combine(_env.WebRootPath, mat.FilePath.TrimStart('/', '\\'));
                if (System.IO.File.Exists(phys))
                {
                    System.IO.File.Delete(phys);
                }
            }

            _materialRepo.Delete(id);
            return RedirectToAction("Index");
        }
    }
}

