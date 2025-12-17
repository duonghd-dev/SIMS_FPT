using System;
using System.ComponentModel.DataAnnotations;

namespace SIMS_FPT.Models
{
    public class CourseMaterialModel
    {
        public string MaterialId { get; set; } = Guid.NewGuid().ToString();

        [Required(ErrorMessage = "Subject is required.")]
        public string SubjectId { get; set; } = string.Empty;

        // [NEW] Added ClassId
        [Required(ErrorMessage = "Class is required.")]
        public string ClassId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Title is required.")]
        public string Title { get; set; } = string.Empty;

        public string? FilePath { get; set; }

        [Url(ErrorMessage = "Please enter a valid URL (e.g., https://youtube.com/...).")]
        public string? VideoUrl { get; set; }

        public string? Category { get; set; }

        public DateTime UploadDate { get; set; } = DateTime.Now;
    }
}