using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Hosting;
using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Models;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace SIMS_FPT.Data.Repositories
{
    public class StudentClassRepository : IStudentClassRepository
    {
        private readonly string _filePath;
        private readonly CsvConfiguration _config;

        public StudentClassRepository(IWebHostEnvironment env)
        {
            _filePath = Path.Combine(env.ContentRootPath, "CSV_DATA", "student_classes.csv");
            _config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                HeaderValidated = null
            };
        }

        private List<StudentClassModel> ReadAll()
        {
            if (!File.Exists(_filePath)) return new List<StudentClassModel>();
            try
            {
                using var reader = new StreamReader(_filePath);
                using var csv = new CsvReader(reader, _config);
                return csv.GetRecords<StudentClassModel>().ToList();
            }
            catch
            {
                return new List<StudentClassModel>();
            }
        }

        private void WriteAll(List<StudentClassModel> list)
        {
            var dir = Path.GetDirectoryName(_filePath);
            if (string.IsNullOrEmpty(dir)) dir = Directory.GetCurrentDirectory();
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            using var writer = new StreamWriter(_filePath);
            using var csv = new CsvWriter(writer, _config);
            csv.WriteRecords(list);
        }

        public void Add(StudentClassModel model)
        {
            var list = ReadAll();
            // Tránh trùng lặp (1 sinh viên không thể vào 1 lớp 2 lần)
            if (!list.Any(x => x.ClassId == model.ClassId && x.StudentId == model.StudentId))
            {
                list.Add(model);
                WriteAll(list);
            }
        }

        public void Remove(string classId, string studentId)
        {
            var list = ReadAll();
            var item = list.FirstOrDefault(x => x.ClassId == classId && x.StudentId == studentId);
            if (item != null)
            {
                list.Remove(item);
                WriteAll(list);
            }
        }

        public List<StudentClassModel> GetByClassId(string classId)
        {
            return ReadAll().Where(x => x.ClassId == classId).ToList();
        }

        public List<StudentClassModel> GetByStudentId(string studentId)
        {
            return ReadAll().Where(x => x.StudentId == studentId).ToList();
        }

        public bool IsEnrolled(string classId, string studentId)
        {
            return ReadAll().Any(x => x.ClassId == classId && x.StudentId == studentId);
        }

        // ... (các hàm GetAll, Add có sẵn giữ nguyên) ...

        public void DeleteByClassAndStudent(string classId, string studentId)
        {
            var allRecords = ReadAll();
            var recordToDelete = allRecords.FirstOrDefault(x => x.ClassId == classId && x.StudentId == studentId);

            if (recordToDelete != null)
            {
                allRecords.Remove(recordToDelete);
                // Ghi đè lại file CSV
                using (var writer = new StreamWriter(_filePath))
                using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)))
                {
                    csv.WriteRecords(allRecords);
                }
            }
        }
        // ...
    }
}