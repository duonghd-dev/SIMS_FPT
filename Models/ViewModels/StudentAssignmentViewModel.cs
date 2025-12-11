using SIMS_FPT.Models;
using System;

namespace SIMS_FPT.Models.ViewModels
{
    public class StudentAssignmentViewModel
    {
        public AssignmentModel Assignment { get; set; }
        public SubmissionModel? Submission { get; set; }

        public bool HasSubmission => Submission != null && !string.IsNullOrEmpty(Submission.FilePath);
        public bool HasGrade => Submission?.Grade != null;
        public string TeacherComments => Submission?.TeacherComments ?? string.Empty;
        public double? Grade => Submission?.Grade;
        public DateTime? SubmittedAt => Submission?.SubmissionDate;
    }
}

