using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SIMS_FPT.Data.Repositories
{
    public class SalaryRepository : ISalaryRepository
    {
        private readonly string _path;

        public SalaryRepository()
        {
            _path = Path.Combine(Directory.GetCurrentDirectory(), "CSV_DATA", "salary.csv");

            if (!File.Exists(_path))
            {
                File.WriteAllText(_path,
                    "staff_id,name,gender,joining_date,amount,status");
            }
        }

        private List<SalaryModel> Parse()
        {
            var list = new List<SalaryModel>();
            string[] lines = File.ReadAllLines(_path);

            var pattern = ",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))";

            for (int i = 1; i < lines.Length; i++)
            {
                var v = Regex.Split(lines[i], pattern);
                if (v.Length < 6) continue;

                list.Add(new SalaryModel
                {
                    StaffId = v[0],
                    Name = v[1],
                    Gender = v[2],
                    JoiningDate = DateTime.TryParse(v[3], out var d) ? d : DateTime.MinValue,
                    Amount = decimal.TryParse(v[4], out var a) ? a : 0,
                    Status = v[5]
                });
            }

            return list;
        }

        private string Format(SalaryModel m)
        {
            return $"{m.StaffId},{m.Name},{m.Gender},{m.JoiningDate:yyyy-MM-dd},{m.Amount},{m.Status}";
        }

        public List<SalaryModel> GetAll() => Parse();

        public SalaryModel GetById(string id)
            => Parse().FirstOrDefault(x => x.StaffId == id);

        public void Add(SalaryModel m)
        {
            File.AppendAllText(_path, Environment.NewLine + Format(m));
        }

        public void Update(SalaryModel m)
        {
            var lines = File.ReadAllLines(_path).ToList();

            for (int i = 1; i < lines.Count; i++)
            {
                if (lines[i].Split(',')[0] == m.StaffId)
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
