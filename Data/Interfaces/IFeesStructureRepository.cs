using SIMS_FPT.Models;
using System.Collections.Generic;

namespace SIMS_FPT.Data.Interfaces
{
    public interface IFeesStructureRepository
    {
        List<FeesStructureModel> GetAll();
        FeesStructureModel GetById(string id);
        void Add(FeesStructureModel m);
        void Update(FeesStructureModel m);
        void Delete(string id);

        List<string> GetUniqueClasses();
    }
}
