using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Hosting;
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

        public CourseMaterialRepository(IWebHostEnvironment env)
        {
            _filePath = Path.Combine(env.ContentRootPath, "CSV_DATA", "course_materials.csv");

            _config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                HeaderValidated = null
            };

            if (!File.Exists(_filePath))
            {
                var dir = Path.GetDirectoryName(_filePath);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                // [UPDATED] Added ClassId to header
                File.WriteAllText(_filePath, "MaterialId,SubjectId,ClassId,Title,FilePath,VideoUrl,Category,UploadDate" + Environment.NewLine);
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
            ReadAll().Where(m => m.SubjectId.Equals(subjectId, StringComparison.OrdinalIgnoreCase)).ToList();

        public CourseMaterialModel GetById(string id)
        {
            return ReadAll().FirstOrDefault(m => m.MaterialId == id);
        }

        public void Add(CourseMaterialModel model)
        {
            var list = ReadAll();
            if (string.IsNullOrEmpty(model.MaterialId)) model.MaterialId = Guid.NewGuid().ToString();

            list.Add(model);
            WriteAll(list);
        }

        public void Update(CourseMaterialModel model)
        {
            var list = ReadAll();
            var index = list.FindIndex(m => m.MaterialId == model.MaterialId);

            if (index != -1)
            {
                // Replace the item at the index
                list[index] = model;
                WriteAll(list);
            }
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