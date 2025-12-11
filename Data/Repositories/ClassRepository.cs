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
    public class ClassRepository : IClassRepository
    {
        private readonly string _filePath;
        private readonly CsvConfiguration _config;

        public ClassRepository()
        {
            _filePath = Path.Combine(Directory.GetCurrentDirectory(), "CSV_DATA", "classes.csv");
            _config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                HeaderValidated = null,
            };
        }

        private List<Class> ReadAll()
        {
            if (!File.Exists(_filePath))
            {
                return new List<Class>();
            }
            using var reader = new StreamReader(_filePath);
            using var csv = new CsvReader(reader, _config);
            return csv.GetRecords<Class>().ToList();
        }

        private void WriteAll(List<Class> classes)
        {
            using var writer = new StreamWriter(_filePath);
            using var csv = new CsvWriter(writer, _config);
            csv.WriteRecords(classes);
        }

        public void AddClass(Class @class)
        {
            var classes = ReadAll();
            @class.Id = classes.Any() ? classes.Max(c => c.Id) + 1 : 1;
            classes.Add(@class);
            WriteAll(classes);
        }

        public void DeleteClass(int id)
        {
            var classes = ReadAll();
            var classToRemove = classes.FirstOrDefault(c => c.Id == id);
            if (classToRemove != null)
            {
                classes.Remove(classToRemove);
                WriteAll(classes);
            }
        }

        public List<Class> GetAllClasses()
        {
            return ReadAll();
        }

        public Class GetClassById(int id)
        {
            return ReadAll().FirstOrDefault(c => c.Id == id);
        }

        public void UpdateClass(Class @class)
        {
            var classes = ReadAll();
            var index = classes.FindIndex(c => c.Id == @class.Id);
            if (index != -1)
            {
                classes[index] = @class;
                WriteAll(classes);
            }
        }
    }
}
