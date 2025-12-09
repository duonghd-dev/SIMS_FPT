using System.Collections.Generic;

namespace SIMS_FPT.Models
{
    public class DashboardViewModel
    {
        // Các số liệu tổng quan cũ
        public int StudentCount { get; set; }
        public int DepartmentCount { get; set; }
        public decimal TotalRevenue { get; set; }

        // --- DỮ LIỆU BIỂU ĐỒ MỚI ---
        
        // Chart 1: Doanh thu theo Năm (Revenue)
        public List<string> RevenueLabels { get; set; } = new List<string>(); // VD: ["2020", "2021", "2022"]
        public List<decimal> RevenueData { get; set; } = new List<decimal>(); // VD: [1000, 5000, 2000]

        // Chart 2: Số lượng học sinh theo Lớp (Number of Students)
        public List<string> StudentClassLabels { get; set; } = new List<string>(); // VD: ["10A", "10B", "11A"]
        public List<int> StudentClassData { get; set; } = new List<int>(); // VD: [50, 40, 45]
    }
}