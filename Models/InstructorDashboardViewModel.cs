using System.Collections.Generic;

namespace SIMS_FPT.Models.ViewModels
{
    public class InstructorDashboardViewModel
    {
        // 2. Classroom & Schedule Management
        public List<ClassScheduleItem> TodayClasses { get; set; } = new List<ClassScheduleItem>();

        // 3. Student Monitoring & Analytics ("At-Risk" Alerts)
        public List<AtRiskStudent> AtRiskStudents { get; set; } = new List<AtRiskStudent>();

        // Data for ApexCharts (Pass as JSON to view)
        public List<int> PerformanceData { get; set; }
        public List<string> PerformanceLabels { get; set; }

        // 5. Personal/Administrative Summary
        public int LeaveDaysRemaining { get; set; }
        public string LastSalaryMonth { get; set; }
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
        public string Reason { get; set; } // e.g., "Low Attendance (< 75%)"
        public string RiskLevel { get; set; } // "High", "Medium"
    }
}