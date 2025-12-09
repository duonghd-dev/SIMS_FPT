using System;

namespace SIMS_FPT.Models
{
    public class CourseMaterialModel
    {
        public string MaterialId { get; set; } = Guid.NewGuid().ToString();
        public string SubjectId { get; set; }
        public string Title { get; set; }
        public string FilePath { get; set; } // Stores "files/week1_notes.pdf"
        public string VideoUrl { get; set; } // YouTube link
        public string Category { get; set; } // "Week 1", "Week 2"
        public DateTime UploadDate { get; set; } = DateTime.Now;
    }
}