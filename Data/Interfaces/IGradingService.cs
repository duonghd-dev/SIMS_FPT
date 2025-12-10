using SIMS_FPT.Models.ViewModels;

namespace SIMS_FPT.Business.Interfaces
{
    public interface IGradingService
    {
        void ProcessGrades(BulkGradeViewModel model);
    }
}