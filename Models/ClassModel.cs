// File: SIMS_FPT/Models/ClassModel.cs
using CsvHelper.Configuration.Attributes;

namespace SIMS_FPT.Models
{
    public class ClassModel
    {
        [Name("class_id")]
        public string ClassId { get; set; }

        [Name("class_name")]
        public string ClassName { get; set; }

        [Name("subject_id")] // Liên kết với SubjectModel
        public string SubjectId { get; set; }

        [Name("teacher_id")] // Liên kết với Teacher (User có Role Teacher)
        public string TeacherId { get; set; }
        
        [Name("semeter")]
        public string Semester { get; set; } // Ví dụ: Spring2025
    }
}