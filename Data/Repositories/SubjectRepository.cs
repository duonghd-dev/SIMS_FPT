using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Hosting; // Added for IWebHostEnvironment
using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace SIMS_FPT.Data.Repositories
{
    public class SubjectRepository : ISubjectRepository
    {
        private readonly string _filePath;
        private readonly CsvConfiguration _config;

        public SubjectRepository(IWebHostEnvironment env)
        {
            _filePath = Path.Combine(env.ContentRootPath, "CSV_DATA", "subjects.csv");

            // Fix 1: Initialize the CsvConfiguration properly in the constructor
            _config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                HeaderValidated = null
            };
        }

        // Fix 2: Helper method to read data using CsvHelper (replaces the broken GetAll loop)
        private List<SubjectModel> ReadAll()
        {
            if (!File.Exists(_filePath)) return new List<SubjectModel>();
            try
            {
                using var reader = new StreamReader(_filePath);
                using var csv = new CsvReader(reader, _config);
                return csv.GetRecords<SubjectModel>().ToList();
            }
            catch
            {
                return new List<SubjectModel>();
            }
        }

        private void WriteAll(List<SubjectModel> list)
        {
            var dir = Path.GetDirectoryName(_filePath);
            if (string.IsNullOrEmpty(dir)) dir = Directory.GetCurrentDirectory();
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            using var writer = new StreamWriter(_filePath);
            using var csv = new CsvWriter(writer, _config);
            csv.WriteRecords(list);
        }

        // Fix 3: Clean implementation of GetAll using the helper
        public List<SubjectModel> GetAll() => ReadAll();

        public SubjectModel GetById(string id) => ReadAll().FirstOrDefault(s => s.SubjectId.Equals(id, StringComparison.OrdinalIgnoreCase));

        public void Add(SubjectModel m)
        {
            var l = ReadAll();
            l.Add(m);
            WriteAll(l);
        }

        public void Update(SubjectModel m)
        {
            var l = ReadAll();
            var i = l.FindIndex(x => x.SubjectId == m.SubjectId);
            if (i != -1)
            {
                l[i] = m;
                WriteAll(l);
            }
        }

        public void Delete(string id)
        {
            var l = ReadAll();
            var item = l.FirstOrDefault(x => x.SubjectId == id);
            if (item != null)
            {
                l.Remove(item);
                WriteAll(l);
            }
        }
    }
}