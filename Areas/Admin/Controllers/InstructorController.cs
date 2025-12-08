using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using SIMS_FPT.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;


namespace SIMS_FPT.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class InstructorController : Controller
    {
        private readonly string _csvFilePath;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public InstructorController(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
            _csvFilePath = Path.Combine(Directory.GetCurrentDirectory(), "CSV_DATA", "teachers.csv");
        }

        // --- HELPER METHODS ---
        private List<TeacherCSVModel> GetAllTeachersFromCsv()
        {
            var teachers = new List<TeacherCSVModel>();
            if (!System.IO.File.Exists(_csvFilePath)) return teachers;

            string[] lines = System.IO.File.ReadAllLines(_csvFilePath);
            // Bỏ qua header (i=1)
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;

                var pattern = ",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))";
                var values = Regex.Split(line, pattern);
                for (int j = 0; j < values.Length; j++) values[j] = values[j].Trim('"');

                // CSV gốc có 17 cột. Nếu ta thêm ảnh vào cuối thì sẽ là 18.
                if (values.Length >= 17)
                {
                    try
                    {
                        var teacher = new TeacherCSVModel
                        {
                            TeacherId = values[0],
                            Name = values[1],
                            Gender = values[2],
                            DateOfBirth = DateTime.TryParse(values[3], out var dob) ? dob : DateTime.MinValue,
                            Mobile = values[4],
                            JoiningDate = DateTime.TryParse(values[5], out var join) ? join : DateTime.MinValue,
                            Qualification = values[6],
                            Experience = values[7],
                            Username = values[8],
                            Email = values[9],
                            Password = values[10],
                            // values[11] là repeat_password, ta không cần lưu vào model hiển thị
                            Address = values[12],
                            City = values[13],
                            State = values[14],
                            ZipCode = values[15],
                            Country = values[16]
                        };

                        // Kiểm tra nếu có cột thứ 18 (ImagePath) thì lấy, không thì để default
                        if (values.Length > 17)
                        {
                            teacher.ImagePath = values[17];
                        }

                        teachers.Add(teacher);
                    }
                    catch { /* Ignore lines with bad data */ }
                }
            }
            return teachers;
        }

        private string FormatTeacherToCsvLine(TeacherCSVModel model)
        {
            // Format đúng thứ tự cột trong file CSV + Thêm ImagePath vào cuối
            return string.Format("{0},\"{1}\",{2},{3},{4},{5},\"{6}\",\"{7}\",{8},{9},{10},{10},\"{11}\",\"{12}\",\"{13}\",{14},\"{15}\",{16}",
                model.TeacherId, model.Name, model.Gender, model.DateOfBirth.ToString("yyyy-MM-dd"),
                model.Mobile, model.JoiningDate.ToString("yyyy-MM-dd"), model.Qualification, model.Experience,
                model.Username, model.Email, model.Password, // Repeat pass giống pass
                model.Address, model.City, model.State, model.ZipCode, model.Country,
                model.ImagePath // Cột mới thêm cuối cùng
            );
        }

        private async Task<string> UploadFile(TeacherCSVModel model)
        {
            if (model.TeacherImageFile != null)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.TeacherImageFile.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.TeacherImageFile.CopyToAsync(fileStream);
                }
                return "/images/" + uniqueFileName;
            }
            return null;
        }

        // --- ACTIONS ---

        public IActionResult List()
        {
            var teachers = GetAllTeachersFromCsv();
            return View(teachers);
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(TeacherCSVModel model)
        {
            try
            {
                string newImagePath = await UploadFile(model);
                model.ImagePath = newImagePath ?? "/assets/img/profiles/avatar-02.jpg"; // Ảnh mặc định

                string line = FormatTeacherToCsvLine(model);
                System.IO.File.AppendAllText(_csvFilePath, Environment.NewLine + line);
                return RedirectToAction("List");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error adding teacher: " + ex.Message);
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var teachers = GetAllTeachersFromCsv();
            var teacher = teachers.FirstOrDefault(t => t.TeacherId == id);
            if (teacher == null) return NotFound();
            return View(teacher);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(TeacherCSVModel model)
        {
            try
            {
                if (model.TeacherImageFile != null)
                {
                    model.ImagePath = await UploadFile(model);
                }

                string[] allLines = System.IO.File.ReadAllLines(_csvFilePath);
                var newContent = new List<string> { allLines[0] }; // Keep header
                bool found = false;

                for (int i = 1; i < allLines.Length; i++)
                {
                    var cols = allLines[i].Split(',');
                    // ID là cột đầu tiên (index 0)
                    string currentId = cols[0];

                    if (currentId == model.TeacherId)
                    {
                        newContent.Add(FormatTeacherToCsvLine(model));
                        found = true;
                    }
                    else
                    {
                        newContent.Add(allLines[i]);
                    }
                }

                if (found)
                {
                    System.IO.File.WriteAllLines(_csvFilePath, newContent);
                    return RedirectToAction("List");
                }

                ModelState.AddModelError("", "Teacher ID not found.");
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error updating: " + ex.Message);
                return View(model);
            }
        }

        public IActionResult DeleteTeacher(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id)) return NotFound();
                string[] allLines = System.IO.File.ReadAllLines(_csvFilePath);
                var newContent = new List<string>();
                if (allLines.Length > 0) newContent.Add(allLines[0]);

                bool isDeleted = false;
                for (int i = 1; i < allLines.Length; i++)
                {
                    var cols = allLines[i].Split(',');
                    string currentId = cols[0]; // ID cột đầu

                    if (currentId != id) newContent.Add(allLines[i]);
                    else isDeleted = true;
                }

                if (isDeleted) System.IO.File.WriteAllLines(_csvFilePath, newContent);
                return RedirectToAction("List");
            }
            catch
            {
                return RedirectToAction("List");
            }
        }

        [HttpGet]
        public IActionResult Detail(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var teachers = GetAllTeachersFromCsv();
            var teacher = teachers.FirstOrDefault(t => t.TeacherId == id);
            if (teacher == null) return NotFound();
            return View(teacher);
        }
    }
}