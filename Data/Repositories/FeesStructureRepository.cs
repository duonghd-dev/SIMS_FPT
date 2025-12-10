// using SIMS_FPT.Data.Interfaces;
// using SIMS_FPT.Models;
// using System.Collections.Generic;
// using System.IO;
// using System.Linq;
// using System.Text.RegularExpressions;

// namespace SIMS_FPT.Data.Repositories
// {
//     public class FeesStructureRepository : IFeesStructureRepository
//     {
//         private readonly string _feesPath;
//         private readonly string _studentsPath;

//         public FeesStructureRepository()
//         {
//             _feesPath = Path.Combine(Directory.GetCurrentDirectory(), "CSV_DATA", "fees_structure.csv");
//             _studentsPath = Path.Combine(Directory.GetCurrentDirectory(), "CSV_DATA", "students.csv");

//             if (!File.Exists(_feesPath))
//             {
//                 File.WriteAllText(_feesPath, "id,fees_name,class,amount,start_date,end_date");
//             }
//         }

//         public List<string> GetUniqueClasses()
//         {
//             var list = new List<string>();

//             if (!File.Exists(_studentsPath)) return list;

//             string[] lines = File.ReadAllLines(_studentsPath);

//             var pattern = ",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))";

//             for (int i = 1; i < lines.Length; i++)
//             {
//                 var vals = Regex.Split(lines[i], pattern);
//                 if (vals.Length > 5)
//                 {
//                     string cls = vals[5].Trim('"');
//                     if (!string.IsNullOrEmpty(cls))
//                         list.Add(cls);
//                 }
//             }

//             return list.Distinct().OrderBy(x => x).ToList();
//         }

//         public List<FeesStructureModel> GetAll()
//         {
//             var list = new List<FeesStructureModel>();

//             string[] lines = File.ReadAllLines(_feesPath);

//             var pattern = ",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))";

//             for (int i = 1; i < lines.Length; i++)
//             {
//                 var vals = Regex.Split(lines[i], pattern);

//                 if (vals.Length >= 6)
//                 {
//                     list.Add(new FeesStructureModel
//                     {
//                         Id = vals[0],
//                         FeesName = vals[1],
//                         Class = vals[2],
//                         Amount = decimal.TryParse(vals[3], out var a) ? a : 0,
//                         StartDate = DateTime.TryParse(vals[4], out var s) ? s : DateTime.MinValue,
//                         EndDate = DateTime.TryParse(vals[5], out var e) ? e : DateTime.MinValue,
//                     });
//                 }
//             }

//             return list;
//         }

//         public FeesStructureModel GetById(string id)
//         {
//             return GetAll().FirstOrDefault(x => x.Id == id);
//         }

//         public void Add(FeesStructureModel m)
//         {
//             File.AppendAllText(_feesPath, Environment.NewLine + Format(m));
//         }

//         public void Update(FeesStructureModel m)
//         {
//             var lines = File.ReadAllLines(_feesPath).ToList();

//             for (int i = 1; i < lines.Count; i++)
//             {
//                 if (lines[i].Split(',')[0] == m.Id)
//                 {
//                     lines[i] = Format(m);
//                     break;
//                 }
//             }

//             File.WriteAllLines(_feesPath, lines);
//         }

//         public void Delete(string id)
//         {
//             var lines = File.ReadAllLines(_feesPath)
//                             .Where(l => l.Split(',')[0] != id)
//                             .ToList();

//             File.WriteAllLines(_feesPath, lines);
//         }

//         private string Format(FeesStructureModel m)
//         {
//             return $"{m.Id},{m.FeesName},{m.Class},{m.Amount},{m.StartDate:yyyy-MM-dd},{m.EndDate:yyyy-MM-dd}";
//         }
//     }
// }
