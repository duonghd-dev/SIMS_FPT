using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using SIMS_FPT.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SIMS_FPT.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class StudentController : Controller
    {
        private readonly string _csvFilePath;
        private readonly IWebHostEnvironment _webHostEnvironment; 

        public StudentController(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
            _csvFilePath = Path.Combine(Directory.GetCurrentDirectory(), "CSV_DATA", "students.csv");
        }


        // Hàm đọc toàn bộ file CSV và chuyển thành List Model
        private List<StudentCSVModel> GetAllStudentsFromCsv()
        {
            var students = new List<StudentCSVModel>();
            if (!System.IO.File.Exists(_csvFilePath)) return students;

            string[] lines = System.IO.File.ReadAllLines(_csvFilePath);
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;

                var pattern = ",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))";
                var values = Regex.Split(line, pattern);
                for (int j = 0; j < values.Length; j++) values[j] = values[j].Trim('"');

                if (values.Length > 20)
                {
                    try
                    {
                        students.Add(new StudentCSVModel
                        {
                            FirstName = values[0], LastName = values[1], StudentId = values[2], Gender = values[3],
                            DateOfBirth = DateTime.TryParse(values[4], out var dob) ? dob : DateTime.MinValue,
                            ClassName = values[5], Religion = values[6],
                            JoiningDate = DateTime.TryParse(values[7], out var join) ? join : DateTime.MinValue,
                            MobileNumber = values[8], AdmissionNumber = values[9], Section = values[10], ImagePath = values[11],
                            FatherName = values[12], FatherOccupation = values[13], FatherMobile = values[14], FatherEmail = values[15],
                            MotherName = values[16], MotherOccupation = values[17], MotherMobile = values[18], MotherEmail = values[19],
                            Address = values[20], PermanentAddress = values.Length > 21 ? values[21] : ""
                        });
                    }
                    catch { }
                }
            }
            return students;
        }

        // Hàm chuyển đổi Model thành chuỗi CSV để lưu
        private string FormatStudentToCsvLine(StudentCSVModel model)
        {
            // Format chuỗi CSV, bao quanh các trường text bằng ngoặc kép để an toàn
            return string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},\"{20}\",\"{21}\"",
                    model.FirstName, model.LastName, model.StudentId, model.Gender, model.DateOfBirth.ToString("yyyy-MM-dd"),
                    model.ClassName, model.Religion, model.JoiningDate.ToString("yyyy-MM-dd"), model.MobileNumber,
                    model.AdmissionNumber, model.Section, model.ImagePath,
                    model.FatherName, model.FatherOccupation, model.FatherMobile, model.FatherEmail,
                    model.MotherName, model.MotherOccupation, model.MotherMobile, model.MotherEmail,
                    model.Address, model.PermanentAddress);
        }

        private async Task<string> UploadFile(StudentCSVModel model)
        {
            string uniqueFileName = null;
            if (model.StudentImageFile != null)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                uniqueFileName = Guid.NewGuid().ToString() + "_" + model.StudentImageFile.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.StudentImageFile.CopyToAsync(fileStream);
                }
                return "/images/" + uniqueFileName;
            }
            return null;
        }


        public IActionResult ListStudent()
        {
            var students = GetAllStudentsFromCsv();
            return View(students);
        }

        [HttpGet]
        public IActionResult AddStudent()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddStudent(StudentCSVModel model)
        {
            try
            {
                string newImagePath = await UploadFile(model);
                model.ImagePath = newImagePath ?? "/assets/img/profiles/avatar-01.jpg"; 

                string line = FormatStudentToCsvLine(model);
                System.IO.File.AppendAllText(_csvFilePath, Environment.NewLine + line);
                return RedirectToAction("ListStudent");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi thêm mới: " + ex.Message);
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult EditStudent(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var students = GetAllStudentsFromCsv();
            var student = students.FirstOrDefault(s => s.StudentId == id);
            if (student == null) return NotFound();
            return View(student);
        }

        [HttpPost]
        public async Task<IActionResult> EditStudent(StudentCSVModel model)
        {
            try
            {
                if (model.StudentImageFile != null)
                {
                    model.ImagePath = await UploadFile(model);
                }

                string[] allLines = System.IO.File.ReadAllLines(_csvFilePath);
                var newContent = new List<string> { allLines[0] }; // Giữ header
                bool found = false;

                for (int i = 1; i < allLines.Length; i++)
                {
                    var cols = allLines[i].Split(',');
                    string currentId = cols.Length > 2 ? cols[2] : "";

                    if (currentId == model.StudentId)
                    {
                        newContent.Add(FormatStudentToCsvLine(model));
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
                    return RedirectToAction("ListStudent");
                }
                
                ModelState.AddModelError("", "Không tìm thấy ID sinh viên.");
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi cập nhật: " + ex.Message);
                return View(model);
            }
        }

        public IActionResult DeleteStudent(string id)
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
                    string line = allLines[i];
                    
                    var cols = line.Split(','); 
                    string currentId = cols.Length > 2 ? cols[2] : "";

                    if (currentId != id)
                    {
                        newContent.Add(line);
                    }
                    else
                    {
                        isDeleted = true;
                    }
                }

                if (isDeleted)
                {
                    System.IO.File.WriteAllLines(_csvFilePath, newContent);
                }

                return RedirectToAction("ListStudent");
            }
            catch
            {
                return RedirectToAction("ListStudent");
            }
        }

        [HttpGet]
        public IActionResult ViewStudent(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var students = GetAllStudentsFromCsv();
            var student = students.FirstOrDefault(s => s.StudentId == id);

            if (student == null) return NotFound();

            return View(student);
        }
    }
}