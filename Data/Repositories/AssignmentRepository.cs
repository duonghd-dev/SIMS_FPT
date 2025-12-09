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
            if (!File.Exists(_csvFilePath)) File.Create(_csvFilePath).Dispose();
        }

        public List<AssignmentModel> GetAll()
        {
            // Simplified CSV parsing logic for brevity - ideally use a helper or library
            var list = new List<AssignmentModel>();
            if (!File.Exists(_csvFilePath)) return list;

            var lines = File.ReadAllLines(_csvFilePath);
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
            }
            return list;
        }

        public void Add(AssignmentModel m)
        {
            string line = $"{m.AssignmentId},{m.SubjectId},{m.Title},{m.Description},{m.DueDate},{m.MaxPoints},{m.AllowedFileTypes},{m.AreGradesPublished}";
            File.AppendAllText(_csvFilePath, Environment.NewLine + line);
        }

        // Implement Update, Delete, GetBySubject similarly...
        public List<AssignmentModel> GetBySubject(string subjectId) => GetAll().Where(x => x.SubjectId == subjectId).ToList();
        public AssignmentModel GetById(string id) => GetAll().FirstOrDefault(x => x.AssignmentId == id);
        public void Update(AssignmentModel m) { /* Implementation similar to SubjectRepository.Update */ }
        public void Delete(string id) { /* Implementation similar to SubjectRepository.Delete */ }
    }
}