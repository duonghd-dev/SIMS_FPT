using CsvHelper.Configuration.Attributes;
using System;

namespace SIMS_FPT.Models
{
    // Model này đại diện cho file student_classes.csv
    // Mỗi dòng là một sinh viên đăng ký vào một lớp
    public class StudentClassModel
    {
        [Name("enrollment_id")]
        public string EnrollmentId { get; set; } = Guid.NewGuid().ToString();

        [Name("class_id")]
        public string ClassId { get; set; } = string.Empty;

        [Name("student_id")]
        public string StudentId { get; set; } = string.Empty;

        [Name("joined_date")]
        public DateTime JoinedDate { get; set; } = DateTime.Now;
    }
}