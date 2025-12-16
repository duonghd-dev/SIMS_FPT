using System.Collections.Generic;
using SIMS_FPT.Models;

namespace SIMS_FPT.Models.ViewModels
{
    public class StudentProfileViewModel
    {
        public StudentCSVModel Student { get; set; } = new StudentCSVModel();
        public List<AssignmentHistoryItem> AssignmentHistory { get; set; } = new();
        public double AverageScorePercent { get; set; }
    }

    public class AssignmentHistoryItem
    {
        public string AssignmentId { get; set; } = string.Empty;
        public string AssignmentTitle { get; set; } = string.Empty;
        public string SubjectId { get; set; } = string.Empty;
        public double? Grade { get; set; }
        public int MaxPoints { get; set; }
        public string TeacherComments { get; set; } = string.Empty;
        public bool Submitted => Grade.HasValue || !string.IsNullOrEmpty(TeacherComments);
    }
}


