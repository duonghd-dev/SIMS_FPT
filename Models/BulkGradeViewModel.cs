using System.Collections.Generic;

namespace SIMS_FPT.Models.ViewModels
{
    public class BulkGradeViewModel
    {
        public string AssignmentId { get; set; }
        public string AssignmentTitle { get; set; }
        public int MaxPoints { get; set; }
        public bool IsPublished { get; set; }

        public List<StudentGradeItem> StudentGrades { get; set; }
    }

    public class StudentGradeItem
    {
        public string StudentId { get; set; }
        public string StudentName { get; set; }
        public bool HasSubmitted { get; set; }
        public string SubmissionFilePath { get; set; } // For "Digital Submissions" link

        // Editable fields for Bulk Entry
        public double? Grade { get; set; }
        public string Feedback { get; set; }
    }
}