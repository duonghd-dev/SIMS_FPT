using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SIMS_FPT.Data.Repositories
{
    public class ExpenseRepository : IExpenseRepository
    {
        private readonly string _path;

        public ExpenseRepository()
        {
            _path = Path.Combine(Directory.GetCurrentDirectory(), "CSV_DATA", "expenses.csv");

            if (!File.Exists(_path))
            {
                File.WriteAllText(_path,
                    "id,item_name,item_quality,amount,purchase_source,purchase_date,purchase_by");
            }
        }

        private List<ExpenseModel> Parse()
        {
            var list = new List<ExpenseModel>();
            string[] lines = File.ReadAllLines(_path);

            var pattern = ",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))";

            for (int i = 1; i < lines.Length; i++)
            {
                var v = Regex.Split(lines[i], pattern);
                if (v.Length < 7) continue;

                list.Add(new ExpenseModel
                {
                    Id = v[0],
                    ItemName = v[1],
                    ItemQuality = v[2],
                    Amount = decimal.TryParse(v[3], out var a) ? a : 0,
                    PurchaseSource = v[4],
                    PurchaseDate = DateTime.TryParse(v[5], out var d) ? d : DateTime.MinValue,
                    PurchaseBy = v[6]
                });
            }
            return list;
        }

        private string Format(ExpenseModel m)
        {
            return $"{m.Id},{m.ItemName},{m.ItemQuality},{m.Amount},{m.PurchaseSource},{m.PurchaseDate:yyyy-MM-dd},{m.PurchaseBy}";
        }

        public List<ExpenseModel> GetAll() => Parse();

        public ExpenseModel GetById(string id) => Parse().FirstOrDefault(x => x.Id == id);

        public void Add(ExpenseModel m)
        {
            File.AppendAllText(_path, Environment.NewLine + Format(m));
        }

        public void Update(ExpenseModel m)
        {
            var lines = File.ReadAllLines(_path).ToList();

            for (int i = 1; i < lines.Count; i++)
            {
                if (lines[i].Split(',')[0] == m.Id)
                {
                    lines[i] = Format(m);
                    break;
                }
            }

            File.WriteAllLines(_path, lines);
        }

        public void Delete(string id)
        {
            var lines = File.ReadAllLines(_path)
                            .Where(l => l.Split(',')[0] != id)
                            .ToList();

            File.WriteAllLines(_path, lines);
        }
    }
}
