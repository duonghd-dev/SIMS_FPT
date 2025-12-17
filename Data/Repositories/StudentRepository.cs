using CsvHelper;
using CsvHelper.Configuration;
using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Models;
using System;
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
            _config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                HeaderValidated = null
            };
        }

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

        private void WriteAll(List<StudentCSVModel> students)
        {
            var dir = Path.GetDirectoryName(_filePath);
            if (string.IsNullOrEmpty(dir)) dir = Directory.GetCurrentDirectory();
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            using var writer = new StreamWriter(_filePath);
            using var csv = new CsvWriter(writer, _config);
            csv.WriteRecords(students);
        }

        public List<StudentCSVModel> GetAll() => ReadAll();

        public StudentCSVModel GetById(string id) => ReadAll().FirstOrDefault(s => s.StudentId.Equals(id, StringComparison.OrdinalIgnoreCase));

        public List<StudentCSVModel> GetBySubject(string subjectId)
        {
            // Logic lấy sinh viên theo môn học sẽ được xử lý ở bảng trung gian StudentClass
            // Tạm thời trả về tất cả hoặc danh sách rỗng
            return GetAll();
        }

        public void Add(StudentCSVModel student)
        {
            var students = ReadAll();
            if (students.Any(s => s.StudentId == student.StudentId)) return;
            students.Add(student);
            WriteAll(students);
        }

        public void Update(StudentCSVModel student)
        {
            var students = ReadAll();
            var index = students.FindIndex(s => s.StudentId == student.StudentId);
            if (index != -1)
            {
                if (string.IsNullOrEmpty(student.ImagePath))
                {
                    student.ImagePath = students[index].ImagePath;
                }
                students[index] = student;
                WriteAll(students);
            }
        }

        public void Delete(string id)
        {
            var students = ReadAll();
            var item = students.FirstOrDefault(s => s.StudentId == id);
            if (item != null)
            {
                students.Remove(item);
                WriteAll(students);
            }
        }
    }
}