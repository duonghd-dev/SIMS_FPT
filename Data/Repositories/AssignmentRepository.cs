using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SIMS_FPT.Data.Repositories
{
    public class AssignmentRepository : IAssignmentRepository
    {
        private readonly string _csvFilePath;

        public AssignmentRepository()
        {
            _csvFilePath = Path.Combine(Directory.GetCurrentDirectory(), "CSV_DATA", "assignments.csv");
<<<<<<< HEAD
            if (!File.Exists(_csvFilePath)) File.Create(_csvFilePath).Dispose();
=======
            if (!File.Exists(_csvFilePath))
            {
                File.WriteAllText(_csvFilePath, "AssignmentId,SubjectId,Title,Description,DueDate,MaxPoints,AllowedFileTypes,AreGradesPublished,TeacherId" + Environment.NewLine);
            }
>>>>>>> master
        }

        public List<AssignmentModel> GetAll()
        {
            // Simplified CSV parsing logic for brevity - ideally use a helper or library
            var list = new List<AssignmentModel>();
            if (!File.Exists(_csvFilePath)) return list;

            var lines = File.ReadAllLines(_csvFilePath);
<<<<<<< HEAD
            foreach (var line in lines.Skip(1)) // Skip header
            {
                var v = line.Split(','); // Note: Use robust Regex splitting like in SubjectRepository if needed
                if (v.Length < 7) continue;

                list.Add(new AssignmentModel
                {
                    AssignmentId = v[0],
                    SubjectId = v[1],
                    Title = v[2],
                    Description = v[3],
                    DueDate = DateTime.Parse(v[4]),
                    MaxPoints = int.Parse(v[5]),
                    AllowedFileTypes = v[6],
                    AreGradesPublished = bool.Parse(v[7])
                });
=======
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
                        TeacherId = ParseCol(8)
                    });
                }
                catch { continue; }
>>>>>>> master
            }
            return list;
        }

        public void Add(AssignmentModel m)
        {
<<<<<<< HEAD
            string line = $"{m.AssignmentId},{m.SubjectId},{m.Title},{m.Description},{m.DueDate},{m.MaxPoints},{m.AllowedFileTypes},{m.AreGradesPublished}";
=======
            string line = Format(m);
>>>>>>> master
            File.AppendAllText(_csvFilePath, Environment.NewLine + line);
        }

        // Implement Update, Delete, GetBySubject similarly...
        public List<AssignmentModel> GetBySubject(string subjectId) => GetAll().Where(x => x.SubjectId == subjectId).ToList();
        public AssignmentModel GetById(string id) => GetAll().FirstOrDefault(x => x.AssignmentId == id);

        public void Update(AssignmentModel model)
        {
            if (!File.Exists(_csvFilePath)) return;
<<<<<<< HEAD

            string[] lines = File.ReadAllLines(_csvFilePath);
            // Keep the header row
=======
            var lines = File.ReadAllLines(_csvFilePath).ToList();
>>>>>>> master
            var newLines = new List<string> { lines[0] };

            for (int i = 1; i < lines.Length; i++)
            {
<<<<<<< HEAD
                // Get the ID from the current line (assuming ID is the first column)
                string currentId = lines[i].Split(',')[0];

                if (currentId == model.AssignmentId)
                {
                    // Replace with the new formatted data
                    newLines.Add(Format(model));
                }
                else
                {
                    // Keep the existing line
                    newLines.Add(lines[i]);
                }
=======
                var currentId = lines[i].Split(',')[0].Trim('"');
                if (currentId == model.AssignmentId) newLines.Add(Format(model));
                else newLines.Add(lines[i]);
>>>>>>> master
            }
            File.WriteAllLines(_csvFilePath, newLines);
        }

<<<<<<< HEAD
=======
        public AssignmentModel GetById(string id) => GetAll().FirstOrDefault(x => x.AssignmentId == id);
        public List<AssignmentModel> GetBySubject(string subjectId) => GetAll().Where(x => x.SubjectId == subjectId).ToList();

>>>>>>> master
        public void Delete(string id)
        {
            if (!File.Exists(_csvFilePath)) return;

            string[] lines = File.ReadAllLines(_csvFilePath);
            var newLines = new List<string> { lines[0] }; // Keep header

            for (int i = 1; i < lines.Length; i++)
            {
                string currentId = lines[i].Split(',')[0];

                // Only add lines that DO NOT match the ID we want to delete
                if (currentId != id)
                {
                    newLines.Add(lines[i]);
                }
            }

            File.WriteAllLines(_csvFilePath, newLines);
        }
<<<<<<< HEAD
        private string Format(AssignmentModel m)
        {
            return $"{m.AssignmentId},{m.SubjectId},\"{m.Title}\",\"{m.Description}\",{m.DueDate},{m.MaxPoints},\"{m.AllowedFileTypes}\",{m.AreGradesPublished}";
=======

       
        private string Format(AssignmentModel m)
        {
            string Escape(string s) => s?.Replace("\"", "\"\"") ?? "";
            return $"{m.AssignmentId},{m.SubjectId},\"{Escape(m.Title)}\",\"{Escape(m.Description)}\",{m.DueDate:yyyy-MM-dd},{m.MaxPoints},\"{Escape(m.AllowedFileTypes)}\",{m.AreGradesPublished},{m.TeacherId}";
>>>>>>> master
        }

    }
}