using System.Collections.Generic;

namespace SIMS_FPT.Models
{
    public class DashboardViewModel
    {
        // 1. Widgets
        public int StudentCount { get; set; }
        public int DepartmentCount { get; set; }
        public int TeacherCount { get; set; }
        public decimal TotalRevenue { get; set; } // Nếu không dùng revenue thì set 0

        // 2. Charts
        public List<string> RevenueLabels { get; set; } = new();
        public List<decimal> RevenueData { get; set; } = new();

        public List<string> StudentClassLabels { get; set; } = new();
        public List<int> StudentClassData { get; set; } = new();

        // 3. Table data
        public List<StudentCSVModel> NewestStudents { get; set; } = new();
    }
}
