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
    public class DepartmentRepository : IDepartmentRepository
    {
        private readonly string _filePath;
        private readonly CsvConfiguration _config;

        public DepartmentRepository()
        {
            _filePath = Path.Combine(Directory.GetCurrentDirectory(), "CSV_DATA", "departments.csv");
            
            _config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                HeaderValidated = null
            };
        }

        private List<DepartmentModel> ReadAll()
        {
            if (!File.Exists(_filePath)) return new List<DepartmentModel>();
            try
            {
                using var reader = new StreamReader(_filePath);
                using var csv = new CsvReader(reader, _config);
                return csv.GetRecords<DepartmentModel>().ToList();
            }
            catch
            {
                return new List<DepartmentModel>();
            }
        }

        private void WriteAll(List<DepartmentModel> departments)
        {
            var dir = Path.GetDirectoryName(_filePath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            using var writer = new StreamWriter(_filePath);
            using var csv = new CsvWriter(writer, _config);
            csv.WriteRecords(departments);
        }

        public List<DepartmentModel> GetAll()
        {
            return ReadAll();
        }

        public DepartmentModel GetById(string id)
        {
            return ReadAll().FirstOrDefault(d => d.DepartmentId == id);
        }

        public void Add(DepartmentModel model)
        {
            var list = ReadAll();
            if (list.Any(d => d.DepartmentId == model.DepartmentId)) return;

            list.Add(model);
            WriteAll(list);
        }

        public void Update(DepartmentModel model)
        {
            var list = ReadAll();
            var index = list.FindIndex(d => d.DepartmentId == model.DepartmentId);
            if (index != -1)
            {
                list[index] = model;
                WriteAll(list);
            }
        }

        public void Delete(string id)
        {
            var list = ReadAll();
            var item = list.FirstOrDefault(d => d.DepartmentId == id);
            if (item != null)
            {
                list.Remove(item);
                WriteAll(list);
            }
        }
    }
}