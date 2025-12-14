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
        public string SelectedStudentId { get; set; }
        public string SelectedStudentName { get; set; }
        public List<StudentOption> StudentOptions { get; set; } = new();
        public List<int> SubmissionCounts { get; set; } = new();
        public List<int> GradedCounts { get; set; } = new();
        public int TotalSubmissions { get; set; }
        public int TotalGraded { get; set; }

        public int LeaveDaysRemaining { get; set; }
        public string LastSalaryMonth { get; set; }
    }

    public class TeachingClassInfo
    {
        public string ClassId { get; set; }
        public string ClassName { get; set; }
        public string SubjectCode { get; set; }
        public string SubjectName { get; set; }
        public int StudentCount { get; set; }
        public string Semester { get; set; }
    }

    public class ClassScheduleItem
    {
        public string Time { get; set; }
        public string SubjectName { get; set; }
        public string Room { get; set; }
        public string ClassName { get; set; }
    }

    public class AtRiskStudent
    {
        public string StudentName { get; set; }
        public string Reason { get; set; }
        public string RiskLevel { get; set; }
        public string StudentId { get; set; }
    }

    public class StudentOption
    {
        public string StudentId { get; set; }
        public string StudentName { get; set; }
    }

    public class RecentActivityItem
    {
        public string StudentName { get; set; }
        public string StudentId { get; set; }
        public string AssignmentTitle { get; set; }
        public DateTime SubmissionDate { get; set; }
        public string TimeAgo { get; set; }
    }
}