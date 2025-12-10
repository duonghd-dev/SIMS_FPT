using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SIMS_FPT.Data.Repositories
{
    public class AssignmentRepository : IAssignmentRepository
    {
        private readonly string _csvFilePath;

        public AssignmentRepository()
        {
            _csvFilePath = Path.Combine(Directory.GetCurrentDirectory(), "CSV_DATA", "assignments.csv");
            if (!File.Exists(_csvFilePath))
            {
                // Create file with headers if it doesn't exist
                File.WriteAllText(_csvFilePath, "AssignmentId,SubjectId,Title,Description,DueDate,MaxPoints,AllowedFileTypes,AreGradesPublished" + Environment.NewLine);
            }
        }

        public List<AssignmentModel> GetAll()
        {
            var list = new List<AssignmentModel>();
            if (!File.Exists(_csvFilePath)) return list;

            var lines = File.ReadAllLines(_csvFilePath);

            // Regex to handle CSV lines with quoted fields containing commas
            // Matches: quoted strings OR non-comma sequences
            var csvRegex = new Regex("(?:^|,)(\"(?:[^\"]|\"\")*\"|[^,]*)", RegexOptions.Compiled);

            foreach (var line in lines.Skip(1)) // Skip header
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var matches = csvRegex.Matches(line);

                // We expect at least 8 columns. 
                // Note: matches may include empty strings for empty columns.
                if (matches.Count < 8) continue;

                // Helper to trim quotes and unescape "" to "
                string ParseCol(int index)
                {
                    var val = matches[index].Value.TrimStart(',').Trim();
                    if (val.StartsWith("\"") && val.EndsWith("\""))
                    {
                        val = val.Substring(1, val.Length - 2).Replace("\"\"", "\"");
                    }
                    return val;
                }

                try
                {
                    list.Add(new AssignmentModel
                    {
                        AssignmentId = ParseCol(0),
                        SubjectId = ParseCol(1),
                        Title = ParseCol(2),
                        Description = ParseCol(3),
                        DueDate = DateTime.Parse(ParseCol(4)),
                        MaxPoints = int.Parse(ParseCol(5)),
                        AllowedFileTypes = ParseCol(6),
                        AreGradesPublished = bool.Parse(ParseCol(7))
                    });
                }
                catch
                {
                    // Skip malformed lines
                    continue;
                }
            }
            return list;
        }

        public void Add(AssignmentModel m)
        {
            // Use the standard Format method to ensure consistency with Update logic
            string line = Format(m);
            File.AppendAllText(_csvFilePath, Environment.NewLine + line);
        }

        public void Update(AssignmentModel model)
        {
            if (!File.Exists(_csvFilePath)) return;

            var lines = File.ReadAllLines(_csvFilePath).ToList();
            // Keep the header
            var newLines = new List<string> { lines[0] };

            for (int i = 1; i < lines.Count; i++)
            {
                // Simple split to get ID is risky if ID is quoted, but usually IDs are safe.
                // Better to use the same logic, but for performance, we check startsWith if IDs are clean.
                // Or simply re-parse the ID carefully.
                var currentId = lines[i].Split(',')[0].Trim('"');

                if (currentId == model.AssignmentId)
                {
                    newLines.Add(Format(model));
                }
                else
                {
                    newLines.Add(lines[i]);
                }
            }
            File.WriteAllLines(_csvFilePath, newLines);
        }

        public AssignmentModel GetById(string id) => GetAll().FirstOrDefault(x => x.AssignmentId == id);

        public List<AssignmentModel> GetBySubject(string subjectId) => GetAll().Where(x => x.SubjectId == subjectId).ToList();

        public void Delete(string id)
        {
            if (!File.Exists(_csvFilePath)) return;
            var lines = File.ReadAllLines(_csvFilePath);
            var newLines = lines.Where((line, index) => index == 0 || line.Split(',')[0].Trim('"') != id).ToList();
            File.WriteAllLines(_csvFilePath, newLines);
        }

        // Standardized Formatter: Quotes text fields, escapes internal quotes
        private string Format(AssignmentModel m)
        {
            string Escape(string s) => s?.Replace("\"", "\"\"") ?? "";

            return $"{m.AssignmentId},{m.SubjectId},\"{Escape(m.Title)}\",\"{Escape(m.Description)}\",{m.DueDate:yyyy-MM-dd},{m.MaxPoints},\"{Escape(m.AllowedFileTypes)}\",{m.AreGradesPublished}";
        }
    }
}