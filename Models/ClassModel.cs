// File: SIMS_FPT/Models/ClassModel.cs
using CsvHelper.Configuration.Attributes;
using System.ComponentModel.DataAnnotations;

namespace SIMS_FPT.Models
{
    public class ClassModel
    {
        [Name("class_id")]
        [Required(ErrorMessage = "Class ID is required.")]
        public string? ClassId { get; set; }

        [Name("class_name")]
        [Required(ErrorMessage = "Class Name is required.")]
        public string? ClassName { get; set; }

        [Name("semester")]
        [Required(ErrorMessage = "Semester is required.")]
        public string? Semester { get; set; } // Ví dụ: Spring2025

        [Name("number_of_students")]
        [Range(1, int.MaxValue, ErrorMessage = "Number of students must be greater than 0.")]
        public int NumberOfStudents { get; set; }
    }
}