using SIMS_FPT.Models;
using System.Collections.Generic;

namespace SIMS_FPT.Data.Interfaces
{
    public interface IExpenseRepository
    {
        List<ExpenseModel> GetAll();
        ExpenseModel GetById(string id);
        void Add(ExpenseModel m);
        void Update(ExpenseModel m);
        void Delete(string id);
    }
}
