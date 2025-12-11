// File: SIMS_FPT/Data/Interfaces/IClassRepository.cs
using SIMS_FPT.Models;
using System.Collections.Generic;

namespace SIMS_FPT.Data.Interfaces
{
    public interface IClassRepository
    {
        List<ClassModel> GetAll();
        ClassModel GetById(string id);
        void Add(ClassModel classModel);
        void Update(ClassModel classModel);
        void Delete(string id);
    }
}