using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SIMS_FPT.Data.Repositories
{
    public class SubjectRepository : ISubjectRepository
    {
        private readonly string _csvFilePath;

        public SubjectRepository()
        {
            _csvFilePath = Path.Combine(Directory.GetCurrentDirectory(), "CSV_DATA", "subjects.csv");
        }

        public List<SubjectModel> GetAll()
        {
            var list = new List<SubjectModel>();

            if (!File.Exists(_csvFilePath))
                return list;

            string[] lines = File.ReadAllLines(_csvFilePath);

            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;

                var pattern = ",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))";
                var values = Regex.Split(line, pattern);
                for (int j = 0; j < values.Length; j++)
                    values[j] = values[j].Trim('"');

                if (values.Length >= 3)
                {
                    list.Add(new SubjectModel
                    {
                        SubjectId = values[0],
                        SubjectName = values[1],
                        Class = values[2]
                    });
                }
            }

            return list;
        }

        public SubjectModel GetById(string id)
        {
            return GetAll().FirstOrDefault(s => s.SubjectId == id);
        }

        public void Add(SubjectModel model)
        {
            string line = Format(model);
            File.AppendAllText(_csvFilePath, Environment.NewLine + line);
        }

        public void Update(SubjectModel model)
        {
            string[] lines = File.ReadAllLines(_csvFilePath);
            var newLines = new List<string> { lines[0] }; // header

            for (int i = 1; i < lines.Length; i++)
            {
                string id = lines[i].Split(',')[0];

                if (id == model.SubjectId)
                    newLines.Add(Format(model));
                else
                    newLines.Add(lines[i]);
            }

            File.WriteAllLines(_csvFilePath, newLines);
        }

        public void Delete(string id)
        {
            string[] lines = File.ReadAllLines(_csvFilePath);
            var newLines = new List<string> { lines[0] };

            for (int i = 1; i < lines.Length; i++)
            {
                string current = lines[i].Split(',')[0];

                if (current != id)
                    newLines.Add(lines[i]);
            }

            File.WriteAllLines(_csvFilePath, newLines);
        }

        private string Format(SubjectModel m)
        {
            return $"{m.SubjectId},\"{m.SubjectName}\",{m.Class}";
        }
    }
}
