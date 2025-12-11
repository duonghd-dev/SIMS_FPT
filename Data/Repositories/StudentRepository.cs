using CsvHelper;
using CsvHelper.Configuration;
using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Models;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace SIMS_FPT.Data.Repositories
{
    public class StudentRepository : IStudentRepository
    {
        private readonly string _filePath;
        private readonly CsvConfiguration _config;

        public StudentRepository()
        {
            _filePath = Path.Combine(Directory.GetCurrentDirectory(), "CSV_DATA", "students.csv");

            // Cấu hình CsvHelper chấp nhận dữ liệu thiếu hoặc header không khớp 100%
            _config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                HeaderValidated = null
            };
        }

        // --- Hàm Helper đọc file ---
        private List<StudentCSVModel> ReadAll()
        {
            if (!File.Exists(_filePath)) return new List<StudentCSVModel>();
            try
            {
                using var reader = new StreamReader(_filePath);
                using var csv = new CsvReader(reader, _config);
                return csv.GetRecords<StudentCSVModel>().ToList();
            }
            catch
            {
                return new List<StudentCSVModel>();
            }
        }

        // --- Hàm Helper ghi file ---
        private void WriteAll(List<StudentCSVModel> students)
        {
            // Tạo thư mục nếu chưa tồn tại
            var dir = Path.GetDirectoryName(_filePath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            using var writer = new StreamWriter(_filePath);
            using var csv = new CsvWriter(writer, _config);
            csv.WriteRecords(students);
        }

        // --- Thực thi Interface ---

        public List<StudentCSVModel> GetAll()
        {
            return ReadAll();
        }

        public StudentCSVModel GetByEmail(string email)
        {
            return ReadAll().FirstOrDefault(s => s.Email == email);
        }

        public void Add(StudentCSVModel student)
        {
            var students = ReadAll();
            // Kiểm tra trùng Email
            if (students.Any(s => s.Email == student.Email)) return;

            students.Add(student);
            WriteAll(students); // Ghi lại toàn bộ danh sách
        }

        public void Update(StudentCSVModel student)
        {
            var students = ReadAll();
            var index = students.FindIndex(s => s.Email == student.Email);

            if (index != -1)
            {
                // Nếu người dùng không upload ảnh mới, giữ lại ảnh cũ
                if (string.IsNullOrEmpty(student.ImagePath))
                {
                    student.ImagePath = students[index].ImagePath;
                }

                students[index] = student;
                WriteAll(students);
            }
        }

        public void Delete(string email)
        {
            var students = ReadAll();
            var item = students.FirstOrDefault(s => s.Email == email);
            if (item != null)
            {
                students.Remove(item);
                WriteAll(students);
            }
        }
    }
}