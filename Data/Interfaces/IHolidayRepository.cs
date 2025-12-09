using SIMS_FPT.Models;
using System.Collections.Generic;

namespace SIMS_FPT.Data.Interfaces
{
    public interface IHolidayRepository
    {
        List<HolidayModel> GetAll();
        HolidayModel GetById(string id);
        void Add(HolidayModel model);
        void Update(HolidayModel model);
        void Delete(string id);
    }
}
