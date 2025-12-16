using System.Collections.Generic;

namespace SIMS_FPT.Models.ViewModels
{
    public class InstructorDashboardViewModel
    {
        public List<RecentActivityItem> RecentActivities { get; set; } = new List<RecentActivityItem>();


        public List<ClassScheduleItem> TodayClasses { get; set; } = new List<ClassScheduleItem>();

        // NEW: List of all classes taught by the instructor
        public List<TeachingClassInfo> TeachingClasses { get; set; } = new List<TeachingClassInfo>();
        public List<AtRiskStudent> AtRiskStudents { get; set; } = new List<AtRiskStudent>();

        // Data for performance chart
        public List<string> PerformanceLabels { get; set; } = new();
        public List<double> ClassAverageSeries { get; set; } = new();
        public List<double> StudentSeries { get; set; } = new();
        public string SelectedStudentId { get; set; } = string.Empty;
        public string SelectedStudentName { get; set; } = string.Empty;
        public List<StudentOption> StudentOptions { get; set; } = new();
        public List<int> SubmissionCounts { get; set; } = new();
        public List<int> GradedCounts { get; set; } = new();
        public int TotalSubmissions { get; set; }
        public int TotalGraded { get; set; }

        public int LeaveDaysRemaining { get; set; }
        public string LastSalaryMonth { get; set; } = string.Empty;

    }

    public class TeachingClassInfo
    {
        public string ClassId { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public int StudentCount { get; set; }
        public string Semester { get; set; } = string.Empty;
    }

    public class ClassScheduleItem
    {
        public string Time { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public string Room { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
    }

    public class AtRiskStudent
    {
        public string StudentName { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string RiskLevel { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
    }

    public class StudentOption
    {
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
    }

    public class RecentActivityItem
    {
        public string StudentName { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public string AssignmentTitle { get; set; } = string.Empty;
        public DateTime SubmissionDate { get; set; }
        public string TimeAgo { get; set; } = string.Empty;
    }
}