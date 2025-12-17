using SIMS_FPT.Models.ViewModels;
using System.Collections.Generic;

namespace SIMS_FPT.Services.Interfaces
{
    public interface IInstructorDashboardService
    {
        /// <summary>
        /// Get instructor dashboard data for a specific teacher
        /// </summary>
        InstructorDashboardViewModel GetDashboard(string teacherId, string? studentId = null);
    }
}
