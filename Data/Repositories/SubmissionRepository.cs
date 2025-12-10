using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SIMS_FPT.Data.Repositories
{
    public class SubmissionRepository : ISubmissionRepository
    {
        private readonly string _csvFilePath;

        public SubmissionRepository()
        {
            _csvFilePath = Path.Combine(Directory.GetCurrentDirectory(), "CSV_DATA", "submissions.csv");
            if (!File.Exists(_csvFilePath))
            {
                File.WriteAllText(_csvFilePath, "StudentId,AssignmentId,Grade,TeacherComments,SubmissionDate,FilePath\n");
            }
        }

        public List<SubmissionModel> GetByAssignmentId(string assignmentId)
        {
            return GetAll().Where(s => s.AssignmentId == assignmentId).ToList();
        }

        public SubmissionModel GetByStudentAndAssignment(string studentId, string assignmentId)
        {
            return GetAll().FirstOrDefault(s => s.StudentId == studentId && s.AssignmentId == assignmentId);
        }

        public void SaveSubmission(SubmissionModel model)
        {
            var all = GetAll();
            var existing = all.FirstOrDefault(s => s.StudentId == model.StudentId && s.AssignmentId == model.AssignmentId);

            if (existing != null)
            {
                // Update
                existing.Grade = model.Grade;
                existing.TeacherComments = model.TeacherComments;
                existing.SubmissionDate = DateTime.Now; // Update timestamp
                // existing.FilePath = model.FilePath; // Update if needed
            }
            else
            {
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
        }

        private void WriteAll(List<SubmissionModel> list)
        {
            var lines = new List<string> { "StudentId,AssignmentId,Grade,TeacherComments,SubmissionDate,FilePath" };
            foreach (var m in list)
            {
                lines.Add($"{m.StudentId},{m.AssignmentId},{m.Grade},\"{m.TeacherComments}\",{m.SubmissionDate},{m.FilePath}");
            }
            File.WriteAllLines(_csvFilePath, lines);
        }
    }
}