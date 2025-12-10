using SIMS_FPT.Models;
using System.Collections.Generic;

namespace SIMS_FPT.Data.Interfaces
{
    public interface IAssignmentRepository
    {
        List<AssignmentModel> GetAll();
        List<AssignmentModel> GetBySubject(string subjectId);
        AssignmentModel GetById(string id);
        void Add(AssignmentModel model);
        void Update(AssignmentModel model);
        void Delete(string id);
    }
}