using Microsoft.AspNetCore.Mvc;
using SIMS_FPT.Models; 
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;

namespace SIMS_FPT.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class StudentController : Controller
    {
        public IActionResult ListStudent()
        {
            var students = new List<StudentCSVModel>();

            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "CSV_DATA", "students.csv");

            if (System.IO.File.Exists(filePath))
            {
                string[] lines = System.IO.File.ReadAllLines(filePath);

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
                            var student = new StudentCSVModel
                            {
                                FirstName = values[0],
                                LastName = values[1],
                                StudentId = values[2],
                                DateOfBirth = DateTime.TryParse(values[4], out DateTime dob) ? dob : DateTime.MinValue,
                                ClassName = values[5],
                                MobileNumber = values[8],
                                ImagePath = values[11],
                                FatherName = values[12],
                                Address = values[20]
                            };
                            students.Add(student);
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }
            }

            return View(students);
        }

        [HttpGet]
        public IActionResult AddStudent()
        {
            return View();
        }

        // 2. Hàm xử lý lưu dữ liệu vào CSV (POST)
        [HttpPost]
        public IActionResult AddStudent(StudentCSVModel model)
        {
            try 
            {
                // Xử lý upload ảnh (nếu có) - Lưu tên file giả lập
                string imagePath = "/images/default.jpg";
                if (model.StudentImageFile != null)
                {
                    // Ở đây chỉ lấy tên file để lưu vào CSV cho đơn giản
                    imagePath = "/images/" + model.StudentImageFile.FileName;
                }

                // Format dòng dữ liệu để ghi vào CSV (Tuân thủ thứ tự cột của file CSV cũ)
                // Cẩn thận: Nếu trong chuỗi có dấu phẩy, cần bao quanh bằng dấu ngoặc kép
                var line = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},\"{20}\",\"{21}\"",
                    model.FirstName, model.LastName, model.StudentId, model.Gender, model.DateOfBirth.ToString("yyyy-MM-dd"),
                    model.ClassName, model.Religion, model.JoiningDate.ToString("yyyy-MM-dd"), model.MobileNumber,
                    model.AdmissionNumber, model.Section, imagePath,
                    model.FatherName, model.FatherOccupation, model.FatherMobile, model.FatherEmail,
                    model.MotherName, model.MotherOccupation, model.MotherMobile, model.MotherEmail,
                    model.Address, model.PermanentAddress);

                // Đường dẫn file CSV
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "CSV_DATA", "students.csv");

                // Ghi thêm dòng mới vào cuối file (Append)
                System.IO.File.AppendAllText(filePath, Environment.NewLine + line);

                // Lưu xong thì quay về trang danh sách
                return RedirectToAction("ListStudent");
            }
            catch (Exception ex)
            {
                // Nếu lỗi thì hiển thị lại form
                return View(model);
            }
        }
    }
}