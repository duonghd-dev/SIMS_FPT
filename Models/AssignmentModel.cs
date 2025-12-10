using System;

namespace SIMS_FPT.Models
{
    public class AssignmentModel
    {
        public string AssignmentId { get; set; } = Guid.NewGuid().ToString();
        public string SubjectId { get; set; } // Links to SubjectModel
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; }
        public int MaxPoints { get; set; }
        public string AllowedFileTypes { get; set; } // e.g., ".pdf, .docx"

        // "Result Publishing": Toggle to let students see grades
        public bool AreGradesPublished { get; set; } = false;
    }
}