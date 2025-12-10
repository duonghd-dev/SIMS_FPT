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
    public class SubjectRepository : ISubjectRepository
    {
        private readonly string _filePath;
        private readonly CsvConfiguration _config;

        public SubjectRepository()
        {
            _filePath = Path.Combine(Directory.GetCurrentDirectory(), "CSV_DATA", "subjects.csv");
            _config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                HeaderValidated = null
            };
        }

        private List<SubjectModel> ReadAll()
        {
            if (!File.Exists(_filePath)) return new List<SubjectModel>();
            try
            {
                using var reader = new StreamReader(_filePath);
                using var csv = new CsvReader(reader, _config);
                return csv.GetRecords<SubjectModel>().ToList();
            }
            catch { return new List<SubjectModel>(); }
        }

        private void WriteAll(List<SubjectModel> list)
        {
            var dir = Path.GetDirectoryName(_filePath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            using var writer = new StreamWriter(_filePath);
            using var csv = new CsvWriter(writer, _config);
            csv.WriteRecords(list);
        }

        public List<SubjectModel> GetAll() => ReadAll();
        public SubjectModel GetById(string id) => ReadAll().FirstOrDefault(s => s.SubjectId == id);
        public void Add(SubjectModel m) { var l = ReadAll(); l.Add(m); WriteAll(l); }
        public void Update(SubjectModel m)
        {
            var l = ReadAll();
            var i = l.FindIndex(x => x.SubjectId == m.SubjectId);
            if (i != -1) { l[i] = m; WriteAll(l); }
        }
        public void Delete(string id)
        {
            var l = ReadAll();
            var i = l.FirstOrDefault(x => x.SubjectId == id);
            if (i != null) { l.Remove(i); WriteAll(l); }
        }
    }
}