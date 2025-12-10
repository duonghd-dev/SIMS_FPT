using CsvHelper.Configuration.Attributes;
using System;

namespace SIMS_FPT.Models
{
    public class AttendanceModel
    {
        [Name("attendance_id")]
        public string AttendanceId { get; set; }

        [Name("date")]
        public DateTime Date { get; set; }

        [Name("subject_id")]
        public string SubjectId { get; set; }

        [Name("teacher_id")]
        public string TeacherId { get; set; }

        [Name("student_id")]
        public string StudentId { get; set; }

        [Name("status")]
        public string Status { get; set; }

        [Name("remarks")]
        public string? Remarks { get; set; }
    }
}