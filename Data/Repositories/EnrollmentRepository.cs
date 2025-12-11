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
    public class EnrollmentRepository : IEnrollmentRepository
    {
        private readonly string _filePath;
        private readonly CsvConfiguration _config;

        public EnrollmentRepository()
        {
            _filePath = Path.Combine(Directory.GetCurrentDirectory(), "CSV_DATA", "enrollments.csv");
            _config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                HeaderValidated = null,
            };
        }

        private List<Enrollment> ReadAll()
        {
            if (!File.Exists(_filePath)) return new List<Enrollment>();
            using var reader = new StreamReader(_filePath);
            using var csv = new CsvReader(reader, _config);
            return csv.GetRecords<Enrollment>().ToList();
        }

        private void WriteAll(List<Enrollment> enrollments)
        {
            using var writer = new StreamWriter(_filePath);
            using var csv = new CsvWriter(writer, _config);
            csv.WriteRecords(enrollments);
        }

        public void AddEnrollment(Enrollment enrollment)
        {
            var list = ReadAll();
            enrollment.Id = list.Any() ? list.Max(x => x.Id) + 1 : 1;
            list.Add(enrollment);
            WriteAll(list);
        }

        public List<Enrollment> GetEnrollmentsByStudentId(int studentId)
        {
            return ReadAll().Where(e => e.StudentId == studentId).ToList();
        }

        public bool IsEnrolled(int studentId, int classId)
        {
            var list = ReadAll();
            return list.Any(e => e.StudentId == studentId && e.ClassId == classId);
        }
    }
}