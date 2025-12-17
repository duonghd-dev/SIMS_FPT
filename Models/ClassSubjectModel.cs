// File: SIMS_FPT/Models/ClassSubjectModel.cs
using CsvHelper.Configuration.Attributes;
using System.ComponentModel.DataAnnotations;

namespace SIMS_FPT.Models
{
    /// <summary>
    /// Junction table: Represents which subjects are taught in which class and by which teacher
    /// One class can have multiple subjects, each subject has one teacher for that class
    /// </summary>
    public class ClassSubjectModel
    {
        [Name("class_id")]
        [Required(ErrorMessage = "Class ID is required.")]
        public string? ClassId { get; set; }

        [Name("subject_id")]
        [Required(ErrorMessage = "Subject is required.")]
        public string? SubjectId { get; set; }

        [Name("teacher_id")]
        [Required(ErrorMessage = "Teacher is required.")]
        public string? TeacherId { get; set; }
    }
}
