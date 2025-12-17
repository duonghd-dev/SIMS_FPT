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
                // Updated header with ClassId
                File.WriteAllText(_csvFilePath, "AssignmentId,SubjectId,Title,Description,DueDate,MaxPoints,AllowedFileTypes,AreGradesPublished,TeacherId,ClassId" + Environment.NewLine);
            }
        }

        public List<AssignmentModel> GetAll()
        {
            var list = new List<AssignmentModel>();
            if (!File.Exists(_csvFilePath)) return list;

            var lines = File.ReadAllLines(_csvFilePath);
            var csvRegex = new Regex("(?:^|,)(\"(?:[^\"]|\"\")*\"|[^,]*)", RegexOptions.Compiled);

            foreach (var line in lines.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var matches = csvRegex.Matches(line);
                if (matches.Count < 8) continue;

                string ParseCol(int index)
                {
                    if (index >= matches.Count) return "";
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
                        AreGradesPublished = bool.Parse(ParseCol(7)),
                        TeacherId = ParseCol(8),
                        ClassId = ParseCol(9) // [NEW] Read ClassId
                    });
                }
                catch { continue; }
            }
            return list;
        }

        public void Add(AssignmentModel m)
        {
            string line = Format(m);
            File.AppendAllText(_csvFilePath, Environment.NewLine + line);
        }

        public void Update(AssignmentModel model)
        {
            if (!File.Exists(_csvFilePath)) return;
            var lines = File.ReadAllLines(_csvFilePath).ToList();
            var newLines = new List<string> { lines[0] };

            for (int i = 1; i < lines.Count; i++)
            {
                var currentId = lines[i].Split(',')[0].Trim('"');
                if (currentId == model.AssignmentId) newLines.Add(Format(model));
                else newLines.Add(lines[i]);
            }
            File.WriteAllLines(_csvFilePath, newLines);
        }

        public AssignmentModel GetById(string id) => GetAll().FirstOrDefault(x => x.AssignmentId.Equals(id, StringComparison.OrdinalIgnoreCase));
        public List<AssignmentModel> GetBySubject(string subjectId) => GetAll().Where(x => x.SubjectId.Equals(subjectId, StringComparison.OrdinalIgnoreCase)).ToList();

        public void Delete(string id)
        {
            if (!File.Exists(_csvFilePath)) return;
            var lines = File.ReadAllLines(_csvFilePath);
            var newLines = lines.Where((line, index) => index == 0 || line.Split(',')[0].Trim('"') != id).ToList();
            File.WriteAllLines(_csvFilePath, newLines);
        }

        private string Format(AssignmentModel m)
        {
            string Escape(string s) => string.IsNullOrEmpty(s) ? string.Empty : s.Replace("\"", "\"\"");

            // [UPDATED] Changed format from yyyy-MM-dd to yyyy-MM-dd HH:mm
            return $"{m.AssignmentId},{m.SubjectId},\"{Escape(m.Title)}\",\"{Escape(m.Description)}\",{m.DueDate:yyyy-MM-dd HH:mm},{m.MaxPoints},\"{Escape(m.AllowedFileTypes)}\",{m.AreGradesPublished},{m.TeacherId},{m.ClassId}";
        }
    }
}