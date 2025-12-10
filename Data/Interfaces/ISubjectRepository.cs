using SIMS_FPT.Models;
using System.Collections.Generic;

namespace SIMS_FPT.Data.Interfaces
{
    public interface ISubjectRepository
    {
        List<SubjectModel> GetAll();
        SubjectModel GetById(string id);
        void Add(SubjectModel model);
        void Update(SubjectModel model);
        void Delete(string id);
    }
}