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

        [Name("department_id")]
        [Required(ErrorMessage = "Department is required.")]
        public string? DepartmentId { get; set; }

        [Name("number_of_students")]
        [Range(0, int.MaxValue, ErrorMessage = "Number of students cannot be negative.")]
        public int NumberOfStudents { get; set; }
    }
}