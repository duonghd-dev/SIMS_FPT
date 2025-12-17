using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Hosting; // Required for accessing the real project folder
using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace SIMS_FPT.Data.Repositories
{
    public class CourseMaterialRepository : ICourseMaterialRepository
    {
        private readonly string _filePath;
        private readonly CsvConfiguration _config;

        // We inject 'IWebHostEnvironment' to find the true path of your project
        public CourseMaterialRepository(IWebHostEnvironment env)
        {
            // This ensures we read/write to 'SIMS_FPT/CSV_DATA/course_materials.csv'
            _filePath = Path.Combine(env.ContentRootPath, "CSV_DATA", "course_materials.csv");

            _config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                HeaderValidated = null
            };

            // Create the file with headers if it doesn't exist
            if (!File.Exists(_filePath))
            {
                var dir = Path.GetDirectoryName(_filePath);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                // Writing the headers you mentioned
                File.WriteAllText(_filePath, "MaterialId,SubjectId,Title,FilePath,VideoUrl,Category,UploadDate" + Environment.NewLine);
            }
        }

        private List<CourseMaterialModel> ReadAll()
        {
            if (!File.Exists(_filePath)) return new List<CourseMaterialModel>();

            try
            {
                using var reader = new StreamReader(_filePath);
                using var csv = new CsvReader(reader, _config);
                return csv.GetRecords<CourseMaterialModel>().ToList();
            }
            catch
            {
                // Returns empty list if file is empty or corrupted
                return new List<CourseMaterialModel>();
            }
        }

        private void WriteAll(List<CourseMaterialModel> items)
        {
            var dir = Path.GetDirectoryName(_filePath);
            if (string.IsNullOrEmpty(dir)) dir = Directory.GetCurrentDirectory();
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            using var writer = new StreamWriter(_filePath);
            using var csv = new CsvWriter(writer, _config);
            csv.WriteRecords(items);
        }

        public List<CourseMaterialModel> GetAll() => ReadAll();

        public List<CourseMaterialModel> GetBySubject(string subjectId) =>
            ReadAll().Where(m => m.SubjectId == subjectId).ToList();

        public CourseMaterialModel GetById(string id) => ReadAll().FirstOrDefault(m => m.MaterialId == id);

        public void Add(CourseMaterialModel model)
        {
            var list = ReadAll();
            // Assign a new ID if one is missing
            if (string.IsNullOrEmpty(model.MaterialId)) model.MaterialId = Guid.NewGuid().ToString();

            list.Add(model);
            WriteAll(list);
        }

        public void Delete(string id)
        {
            var list = ReadAll();
            var existing = list.FirstOrDefault(m => m.MaterialId == id);
            if (existing != null)
            {
                list.Remove(existing);
                WriteAll(list);
            }
        }
    }
}