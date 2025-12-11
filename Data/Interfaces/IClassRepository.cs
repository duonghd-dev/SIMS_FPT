using SIMS_FPT.Models;
using System.Collections.Generic;

namespace SIMS_FPT.Data.Interfaces
{
    public interface IClassRepository
    {
        List<Class> GetAllClasses();
        Class GetClassById(int id);
        void AddClass(Class @class);
        void UpdateClass(Class @class);
        void DeleteClass(int id);
    }
}
