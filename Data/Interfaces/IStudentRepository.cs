using SIMS_FPT.Models;
using System.Collections.Generic;

namespace SIMS_FPT.Data.Interfaces
{
    public interface IStudentRepository
    {
        List<StudentCSVModel> GetAll();
        StudentCSVModel GetById(string id);
        void Add(StudentCSVModel student);
        void Update(StudentCSVModel student);
        void Delete(string id);
    }
}
