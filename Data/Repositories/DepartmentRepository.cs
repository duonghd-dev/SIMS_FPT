using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SIMS_FPT.Data.Repositories
{
    public class DepartmentRepository : IDepartmentRepository
    {
        private readonly string _path;

        public DepartmentRepository()
        {
            _path = Path.Combine(Directory.GetCurrentDirectory(), "CSV_DATA", "departments.csv");

            if (!File.Exists(_path))
            {
                File.WriteAllText(_path, "department_id,department_name,head_of_department,start_date,no_of_students");
            }
        }

        private List<DepartmentModel> Parse()
        {
            var list = new List<DepartmentModel>();

            string[] lines = File.ReadAllLines(_path);
            var pattern = ",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))";

            for (int i = 1; i < lines.Length; i++)
            {
                var vals = Regex.Split(lines[i], pattern);
                for (int j = 0; j < vals.Length; j++)
                    vals[j] = vals[j].Trim('"');

                if (vals.Length >= 5)
                {
                    list.Add(new DepartmentModel
                    {
                        DepartmentId = vals[0],
                        DepartmentName = vals[1],
                        HeadOfDepartment = vals[2],
                        StartDate = DateTime.TryParse(vals[3], out var sd) ? sd : DateTime.MinValue,
                        NoOfStudents = int.TryParse(vals[4], out var n) ? n : 0
                    });
                }
            }

            return list;
        }

        private string Format(DepartmentModel m)
        {
            return $"{m.DepartmentId},\"{m.DepartmentName}\",\"{m.HeadOfDepartment}\",{m.StartDate:yyyy-MM-dd},{m.NoOfStudents}";
        }

        public List<DepartmentModel> GetAll()
        {
            return Parse();
        }

        public DepartmentModel GetById(string id)
        {
            return Parse().FirstOrDefault(x => x.DepartmentId == id);
        }

        public void Add(DepartmentModel m)
        {
            File.AppendAllText(_path, Environment.NewLine + Format(m));
        }

        public void Update(DepartmentModel m)
        {
            var lines = File.ReadAllLines(_path).ToList();

            for (int i = 1; i < lines.Count; i++)
            {
                if (lines[i].Split(',')[0] == m.DepartmentId)
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
