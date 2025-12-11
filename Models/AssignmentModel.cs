using System;
using System.ComponentModel.DataAnnotations;

namespace SIMS_FPT.Models
{
    public class AssignmentModel
    {
        public string AssignmentId { get; set; } = Guid.NewGuid().ToString();
        [Required(ErrorMessage = "Please select a class.")]
        public string ClassId { get; set; }

        // We keep this for backward compatibility and data integrity, but it will be auto-filled
        public string SubjectId { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        public string Title { get; set; }

        public string Description { get; set; }
        public DateTime DueDate { get; set; }
        public int MaxPoints { get; set; }

        public string? AllowedFileTypes { get; set; }
        public bool AreGradesPublished { get; set; } = false;
        public string? TeacherId { get; set; }
    }
}