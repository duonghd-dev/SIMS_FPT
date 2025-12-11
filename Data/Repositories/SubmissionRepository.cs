using CsvHelper;
using CsvHelper.Configuration;
using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace SIMS_FPT.Data.Repositories
{
    public class SubmissionRepository : ISubmissionRepository
    {
        private readonly string _filePath;
        private readonly CsvConfiguration _config;

        public SubmissionRepository()
        {
            _filePath = Path.Combine(Directory.GetCurrentDirectory(), "CSV_DATA", "submissions.csv");
            _config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null, // Ignored missing fields like SubmissionId
                HeaderValidated = null
            };
        }

        private List<SubmissionModel> ReadAll()
        {
            if (!File.Exists(_filePath)) return new List<SubmissionModel>();
            try
            {
<<<<<<< HEAD
                // Update
                existing.Grade = model.Grade;
                existing.TeacherComments = model.TeacherComments;
                existing.SubmissionDate = DateTime.Now; // Update timestamp
                // existing.FilePath = model.FilePath; // Update if needed
=======
                using var reader = new StreamReader(_filePath);
                using var csv = new CsvReader(reader, _config);
                return csv.GetRecords<SubmissionModel>().ToList();
>>>>>>> master
            }
            catch
            {
<<<<<<< HEAD
                // Add
                model.SubmissionDate = DateTime.Now;
                all.Add(model);
            }

            WriteAll(all);
        }

        public void UpdateGrades(List<SubmissionModel> submissions)
        {
            var all = GetAll();
            foreach (var sub in submissions)
            {
                var existing = all.FirstOrDefault(s => s.StudentId == sub.StudentId && s.AssignmentId == sub.AssignmentId);
                if (existing != null)
                {
                    existing.Grade = sub.Grade;
                    existing.TeacherComments = sub.TeacherComments;
                }
                else
                {
                    all.Add(sub);
                }
            }
            WriteAll(all);
        }

        private List<SubmissionModel> GetAll()
        {
            var list = new List<SubmissionModel>();
            if (!File.Exists(_csvFilePath)) return list;

            var lines = File.ReadAllLines(_csvFilePath);
            foreach (var line in lines.Skip(1))
            {
                var v = line.Split(',');
                if (v.Length < 2) continue;

                var sub = new SubmissionModel
                {
                    StudentId = v[0],
                    AssignmentId = v[1],
                    TeacherComments = v.Length > 3 ? v[3].Trim('"') : ""
                };

                if (v.Length > 2 && double.TryParse(v[2], out double g)) sub.Grade = g;
                if (v.Length > 4 && DateTime.TryParse(v[4], out DateTime d)) sub.SubmissionDate = d;
                if (v.Length > 5) sub.FilePath = v[5];

                list.Add(sub);
            }
            return list;
=======
                return new List<SubmissionModel>();
            }
>>>>>>> master
        }

        private void WriteAll(List<SubmissionModel> list)
        {
            using var writer = new StreamWriter(_filePath);
            using var csv = new CsvWriter(writer, _config);
            csv.WriteRecords(list);
        }

        public SubmissionModel GetByStudentAndAssignment(string studentId, string assignmentId)
        {
            return ReadAll().FirstOrDefault(s => s.StudentId == studentId && s.AssignmentId == assignmentId);
        }

        public List<SubmissionModel> GetByAssignmentId(string assignmentId)
        {
            return ReadAll().Where(s => s.AssignmentId == assignmentId).ToList();
        }

        public void SaveSubmission(SubmissionModel model)
        {
            var list = ReadAll();
            var existing = list.FirstOrDefault(s => s.StudentId == model.StudentId && s.AssignmentId == model.AssignmentId);

            if (existing != null)
            {
<<<<<<< HEAD
                lines.Add($"{m.StudentId},{m.AssignmentId},{m.Grade},\"{m.TeacherComments}\",{m.SubmissionDate},{m.FilePath}");
=======
                // Update existing
                existing.Grade = model.Grade;
                existing.TeacherComments = model.TeacherComments;
                existing.FilePath = model.FilePath;
                existing.SubmissionDate = model.SubmissionDate;
>>>>>>> master
            }
            else
            {
                // Add new
                if (string.IsNullOrEmpty(model.SubmissionId)) model.SubmissionId = Guid.NewGuid().ToString();
                list.Add(model);
            }
            WriteAll(list);
        }

        // [NEW] Bulk Update Implementation
        public void UpdateGrades(List<SubmissionModel> submissions)
        {
            // 1. Read all existing data into memory
            var allSubmissions = ReadAll();

            // 2. Iterate through the incoming list of graded submissions
            foreach (var sub in submissions)
            {
                // Find match by StudentId and AssignmentId
                var existing = allSubmissions.FirstOrDefault(s =>
                    s.StudentId == sub.StudentId &&
                    s.AssignmentId == sub.AssignmentId);

                if (existing != null)
                {
                    // Update relevant fields
                    existing.Grade = sub.Grade;
                    existing.TeacherComments = sub.TeacherComments;

                    // Ideally, preserve the file path and date if they aren't changing, 
                    // but if the incoming model is complete, we can update these too.
                    if (!string.IsNullOrEmpty(sub.FilePath)) existing.FilePath = sub.FilePath;
                    if (sub.SubmissionDate != default) existing.SubmissionDate = sub.SubmissionDate;
                }
                else
                {
                    // Handle case where a grade is assigned to a student who hasn't submitted a file
                    if (string.IsNullOrEmpty(sub.SubmissionId)) sub.SubmissionId = Guid.NewGuid().ToString();
                    allSubmissions.Add(sub);
                }
            }

            // 3. Write the updated list back to CSV once
            WriteAll(allSubmissions);
        }
    }
}