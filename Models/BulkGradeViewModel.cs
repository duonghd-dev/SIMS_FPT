using System.Collections.Generic;

namespace SIMS_FPT.Models.ViewModels
{
    public class BulkGradeViewModel
    {
        public string AssignmentId { get; set; } = string.Empty;
        public string AssignmentTitle { get; set; } = string.Empty;
        public int MaxPoints { get; set; }
        public bool IsPublished { get; set; }
        public List<StudentGradeItem> StudentGrades { get; set; } = new List<StudentGradeItem>();
    }

    public class StudentGradeItem
    {
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public bool HasSubmitted { get; set; }
        public string SubmissionFilePath { get; set; } = string.Empty;
        public double? Grade { get; set; }
        public string Feedback { get; set; } = string.Empty;
    }
}