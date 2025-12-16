using CsvHelper.Configuration.Attributes;
using System.ComponentModel.DataAnnotations;

namespace SIMS_FPT.Models
{
    public class SubjectModel
    {
        [Name("subject_id")]
        [Required(ErrorMessage = "Subject ID is required.")]
        public string? SubjectId { get; set; }

        [Name("subject_name")]
        [Required(ErrorMessage = "Subject Name is required.")]
        public string? SubjectName { get; set; }

        [Name("department_id")]
        [Required(ErrorMessage = "Department is required.")]
        public string? DepartmentId { get; set; }

        [Name("credits")]
        [Range(1, 10, ErrorMessage = "Credits must be between 1 and 10.")]
        public int Credits { get; set; }

        [Name("teacher_ids")]
        public string? TeacherIds { get; set; } // Comma-separated teacher IDs

        [Ignore]
        public List<string>? SelectedTeacherIds { get; set; } // For multi-select binding
    }
}