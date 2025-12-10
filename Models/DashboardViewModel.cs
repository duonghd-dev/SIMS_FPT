using System.Collections.Generic;

namespace SIMS_FPT.Models
{
    public class DashboardViewModel
    {
        // 1. Thẻ thống kê (Widgets)
        public int StudentCount { get; set; }
        public int DepartmentCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TeacherCount { get; set; } // Thêm đếm số giáo viên

        // 2. Dữ liệu biểu đồ (Charts)
        public List<string> RevenueLabels { get; set; } = new List<string>();
        public List<decimal> RevenueData { get; set; } = new List<decimal>();

        public List<string> StudentClassLabels { get; set; } = new List<string>();
        public List<int> StudentClassData { get; set; } = new List<int>();

        // 3. Dữ liệu bảng (Tables & Lists)
        public List<StudentCSVModel> NewestStudents { get; set; } = new List<StudentCSVModel>();
        public List<ExpenseModel> RecentExpenses { get; set; } = new List<ExpenseModel>();
    }
}