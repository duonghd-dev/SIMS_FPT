using SIMS_FPT.Models;
using System.Collections.Generic;

namespace SIMS_FPT.Data.Interfaces
{
    public interface ITeacherRepository
    {
        List<TeacherCSVModel> GetAll();
        TeacherCSVModel GetById(string id);
        void Add(TeacherCSVModel model);
        void Update(TeacherCSVModel model);
        void Delete(string id);
    }
}
