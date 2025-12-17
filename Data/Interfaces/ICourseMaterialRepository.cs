using System.Collections.Generic;
using SIMS_FPT.Models;

namespace SIMS_FPT.Data.Interfaces
{
    public interface ICourseMaterialRepository
    {
        List<CourseMaterialModel> GetAll();
        List<CourseMaterialModel> GetBySubject(string subjectId);
        CourseMaterialModel GetById(string id);
        void Add(CourseMaterialModel model);
        void Update(CourseMaterialModel model);
        void Delete(string id);
    }
}

