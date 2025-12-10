using SIMS_FPT.Models;
using System.Collections.Generic;

namespace SIMS_FPT.Data.Interfaces
{
    public interface IDepartmentRepository
    {
        List<DepartmentModel> GetAll();
        DepartmentModel GetById(string id);
        void Add(DepartmentModel model);
        void Update(DepartmentModel model);
        void Delete(string id);
    }
}