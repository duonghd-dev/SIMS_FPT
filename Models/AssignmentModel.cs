using System;
using System.ComponentModel.DataAnnotations;

namespace SIMS_FPT.Models
{
    public class AssignmentModel
    {
        public string AssignmentId { get; set; } = Guid.NewGuid().ToString();

        [Required(ErrorMessage = "Please select a subject.")]
        public string SubjectId { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        public string Title { get; set; }

        public string Description { get; set; }
        public DateTime DueDate { get; set; }
        public int MaxPoints { get; set; }

        // [FIX] Make nullable to prevent validation errors if dropdown sends empty value
        public string? AllowedFileTypes { get; set; }

        public bool AreGradesPublished { get; set; } = false;

        // Owner ID
        public string? TeacherId { get; set; }
    }
}