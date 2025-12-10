using System;
using System.ComponentModel.DataAnnotations; // 1. Added missing namespace

namespace SIMS_FPT.Models
{
    public class CourseMaterialModel
    {
        public string MaterialId { get; set; } = Guid.NewGuid().ToString();

        // 2. Fixed syntax error: Combined [Required] and ErrorMessage correctly
        [Required(ErrorMessage = "Subject is required.")]
        public string SubjectId { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        public string Title { get; set; }

        public string? FilePath { get; set; } // Stores "materials/week1_notes.pdf"

        [Url(ErrorMessage = "Please enter a valid URL (e.g., https://youtube.com/...).")]
        public string? VideoUrl { get; set; } // YouTube link

        public string? Category { get; set; } // "Week 1", "Week 2"

        public DateTime UploadDate { get; set; } = DateTime.Now;
    }
}