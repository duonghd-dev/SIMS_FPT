using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using SIMS_FPT.Models;
using SIMS_Project.Interface;
using System.Text.RegularExpressions;

namespace SIMS_FPT.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class InstructorController : Controller
    {
        private readonly string _csvFilePath;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IUserRepository _userRepository;

        public InstructorController(IWebHostEnvironment webHostEnvironment, IUserRepository userRepository)
        {
            _webHostEnvironment = webHostEnvironment;
            _userRepository = userRepository;
            _csvFilePath = Path.Combine(Directory.GetCurrentDirectory(), "CSV_DATA", "teachers.csv");
            EnsureTeacherCsvHeader();
        }

        private void EnsureTeacherCsvHeader()
        {
            if (!System.IO.File.Exists(_csvFilePath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_csvFilePath)!);
                var header = "teacher_id,name,gender,date_of_birth,mobile,joining_date,qualification,experience,username,email,password,address,city,state,country,image";
                System.IO.File.WriteAllText(_csvFilePath, header);
            }
        }

        private List<TeacherCSVModel> GetAllTeachersFromCsv()
        {
            var teachers = new List<TeacherCSVModel>();
            if (!System.IO.File.Exists(_csvFilePath)) return teachers;

            string[] lines = System.IO.File.ReadAllLines(_csvFilePath);
            // Skip header
            var pattern = ",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))";

            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;

                var values = Regex.Split(line, pattern).Select(v => v.Trim()).ToArray();
                for (int j = 0; j < values.Length; j++) values[j] = values[j].Trim('"');

                if (values.Length < 16) continue;

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
                        Address = values[11],
                        City = values[12],
                        State = values[13],
                        Country = values[14],
                        ImagePath = values[15]
                    };
                    teachers.Add(teacher);
                }
                catch
                {
                    // ignore malformed line
                }
            }

            return teachers;
        }

        private string FormatTeacherToCsvLine(TeacherCSVModel model)
        {
            // All fields that can contain commas are wrapped in quotes.
            return string.Join(",",
                model.TeacherId,
                $"\"{model.Name}\"",
                model.Gender,
                model.DateOfBirth == DateTime.MinValue ? "" : model.DateOfBirth.ToString("yyyy-MM-dd"),
                model.Mobile,
                model.JoiningDate == DateTime.MinValue ? "" : model.JoiningDate.ToString("yyyy-MM-dd"),
                $"\"{model.Qualification}\"",
                $"\"{model.Experience}\"",
                model.Username,
                model.Email,
                model.Password,
                $"\"{model.Address}\"",
                $"\"{model.City}\"",
                $"\"{model.State}\"",
                model.Country,
                model.ImagePath ?? ""
            );
        }

        private async Task<string?> UploadFile(TeacherCSVModel model)
        {
            if (model.TeacherImageFile == null) return null;

            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.TeacherImageFile.FileName);
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await model.TeacherImageFile.CopyToAsync(fileStream);
            }
            return "/images/" + uniqueFileName;
        }

        // ---------- Actions ----------

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
                // generate TeacherId if not provided
                if (string.IsNullOrWhiteSpace(model.TeacherId))
                {
                    model.TeacherId = "TCH" + DateTime.UtcNow.Ticks.ToString().Substring(10);
                }

                model.ImagePath = await UploadFile(model) ?? "/assets/img/profiles/avatar-02.jpg";

                // append teacher line
                var line = FormatTeacherToCsvLine(model);
                System.IO.File.AppendAllText(_csvFilePath, Environment.NewLine + line);

                // sync to users.csv
                _userRepository.AddTeacherUser(model);

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
            var teacher = GetAllTeachersFromCsv().FirstOrDefault(t => t.TeacherId == id);
            if (teacher == null) return NotFound();
            return View(teacher);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(TeacherCSVModel model, string originalUsername)
        {
            try
            {
                var all = GetAllTeachersFromCsv();
                var idx = all.FindIndex(t => t.TeacherId == model.TeacherId);
                if (idx < 0)
                {
                    ModelState.AddModelError("", "Teacher ID not found.");
                    return View(model);
                }

                if (model.TeacherImageFile != null)
                {
                    model.ImagePath = await UploadFile(model);
                }
                else
                {
                    model.ImagePath = all[idx].ImagePath;
                }

                // update in memory
                all[idx] = model;

                // write file back
                var header = "teacher_id,name,gender,date_of_birth,mobile,joining_date,qualification,experience,username,email,password,address,city,state,country,image";
                var lines = new List<string> { header };
                lines.AddRange(all.Select(FormatTeacherToCsvLine));
                System.IO.File.WriteAllLines(_csvFilePath, lines);

                // sync to users.csv (if username/email/password/name changed)
                _userRepository.UpdateUserFromTeacher(model, originalUsername);

                return RedirectToAction("List");
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
                var all = GetAllTeachersFromCsv();
                var teacher = all.FirstOrDefault(t => t.TeacherId == id);
                if (teacher == null) return RedirectToAction("List");

                var newList = all.Where(t => t.TeacherId != id).ToList();
                var header = "teacher_id,name,gender,date_of_birth,mobile,joining_date,qualification,experience,username,email,password,address,city,state,country,image";
                var lines = new List<string> { header };
                lines.AddRange(newList.Select(FormatTeacherToCsvLine));
                System.IO.File.WriteAllLines(_csvFilePath, lines);

                // delete account in users.csv
                _userRepository.DeleteUserByUsername(teacher.Username);

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
            var teacher = GetAllTeachersFromCsv().FirstOrDefault(t => t.TeacherId == id);
            if (teacher == null) return NotFound();
            return View(teacher);
        }
    }
}
