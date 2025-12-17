using System;

namespace SIMS_FPT.Models
{
    public class SubmissionModel
    {
        public string SubmissionId { get; set; } = Guid.NewGuid().ToString();
        public string AssignmentId { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;

        // Digital Submission
        public string FilePath { get; set; } = string.Empty; // Path to the uploaded file on server
        public DateTime SubmissionDate { get; set; }

        // Feedback Loop & Grading
        public double? Grade { get; set; } // Nullable, as it might not be graded yet
        public string TeacherComments { get; set; } = string.Empty;
    }
}