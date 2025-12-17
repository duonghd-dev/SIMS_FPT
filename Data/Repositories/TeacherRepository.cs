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
    public class TeacherRepository : ITeacherRepository
    {
        private readonly string _filePath;
        private readonly CsvConfiguration _config;

        public TeacherRepository()
        {
            _filePath = Path.Combine(Directory.GetCurrentDirectory(), "CSV_DATA", "teachers.csv");

            // Cấu hình CsvHelper
            _config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                HeaderValidated = null
            };
        }

        // --- Helper Đọc ---
        private List<TeacherCSVModel> ReadAll()
        {
            if (!File.Exists(_filePath)) return new List<TeacherCSVModel>();
            try
            {
                using var reader = new StreamReader(_filePath);
                using var csv = new CsvReader(reader, _config);
                return csv.GetRecords<TeacherCSVModel>().ToList();
            }
            catch
            {
                return new List<TeacherCSVModel>();
            }
        }

        // --- Helper Ghi ---
        private void WriteAll(List<TeacherCSVModel> teachers)
        {
            var dir = Path.GetDirectoryName(_filePath);
            if (string.IsNullOrEmpty(dir)) dir = Directory.GetCurrentDirectory();
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            using var writer = new StreamWriter(_filePath);
            using var csv = new CsvWriter(writer, _config);
            csv.WriteRecords(teachers);
        }

        // --- Implementation ---

        public List<TeacherCSVModel> GetAll()
        {
            return ReadAll();
        }

        public TeacherCSVModel GetById(string id)
        {
            return ReadAll().FirstOrDefault(t => t.TeacherId.Equals(id, StringComparison.OrdinalIgnoreCase));
        }

        public void Add(TeacherCSVModel model)
        {
            var teachers = ReadAll();
            if (teachers.Any(t => t.TeacherId == model.TeacherId)) return;

            teachers.Add(model);
            WriteAll(teachers);
        }

        public void Update(TeacherCSVModel model)
        {
            var teachers = ReadAll();
            var index = teachers.FindIndex(t => t.TeacherId == model.TeacherId);

            if (index != -1)
            {
                if (string.IsNullOrEmpty(model.ImagePath))
                {
                    model.ImagePath = teachers[index].ImagePath;
                }

                teachers[index] = model;
                WriteAll(teachers);
            }
        }

        public void Delete(string id)
        {
            var teachers = ReadAll();
            var item = teachers.FirstOrDefault(t => t.TeacherId == id);
            if (item != null)
            {
                teachers.Remove(item);
                WriteAll(teachers);
            }
        }
    }
}