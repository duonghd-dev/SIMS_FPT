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
                using var reader = new StreamReader(_filePath);
                using var csv = new CsvReader(reader, _config);
                return csv.GetRecords<SubmissionModel>().ToList();
            }
            catch
            {
                return new List<SubmissionModel>();
            }
        }

        private void WriteAll(List<SubmissionModel> list)
        {
            var dir = Path.GetDirectoryName(_filePath);
            if (string.IsNullOrEmpty(dir)) dir = Directory.GetCurrentDirectory();
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            using var writer = new StreamWriter(_filePath);
            using var csv = new CsvWriter(writer, _config);
            csv.WriteRecords(list);
        }

        public SubmissionModel GetByStudentAndAssignment(string studentId, string assignmentId)
        {
            return ReadAll().FirstOrDefault(s => s.StudentId.Equals(studentId, StringComparison.OrdinalIgnoreCase) &&
                                                  s.AssignmentId.Equals(assignmentId, StringComparison.OrdinalIgnoreCase));
        }

        public List<SubmissionModel> GetByAssignmentId(string assignmentId)
        {
            return ReadAll().Where(s => s.AssignmentId == assignmentId).ToList();
        }

        public void SaveSubmission(SubmissionModel model)
        {
            var list = ReadAll();
            var existing = list.FirstOrDefault(s => s.StudentId.Equals(model.StudentId, StringComparison.OrdinalIgnoreCase) &&
                                                     s.AssignmentId.Equals(model.AssignmentId, StringComparison.OrdinalIgnoreCase));

            if (existing != null)
            {
                // Update existing
                existing.Grade = model.Grade;
                existing.TeacherComments = model.TeacherComments;
                existing.FilePath = model.FilePath;
                existing.SubmissionDate = model.SubmissionDate;
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
                    s.StudentId.Equals(sub.StudentId, StringComparison.OrdinalIgnoreCase) &&
                    s.AssignmentId.Equals(sub.AssignmentId, StringComparison.OrdinalIgnoreCase));

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
        // --- BỔ SUNG HÀM NÀY VÀO CUỐI CLASS ---
        public void Delete(string submissionId)
        {
            var list = ReadAll();
            // Tìm bài nộp cần xóa
            var itemToRemove = list.FirstOrDefault(s => s.SubmissionId == submissionId);

            if (itemToRemove != null)
            {
                list.Remove(itemToRemove);
                WriteAll(list); // Ghi lại danh sách mới đã xóa item
            }
        }
    }
}