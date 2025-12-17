using SIMS_FPT.Models;

namespace SIMS_FPT.Services.Interfaces
{
    public interface IDashboardService
    {
        /// <summary>
        /// Prepares admin dashboard data including stats, charts, and newest students
        /// </summary>
        DashboardViewModel GetAdminDashboardData();
    }
}
