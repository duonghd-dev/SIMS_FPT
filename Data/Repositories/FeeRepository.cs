// using SIMS_FPT.Data.Interfaces;
// using SIMS_FPT.Models;
// using System.Collections.Generic;
// using System.IO;
// using System.Linq;
// using System.Text.RegularExpressions;

// namespace SIMS_FPT.Data.Repositories
// {
//     public class FeeRepository : IFeeRepository
//     {
//         private readonly string _path;

//         public FeeRepository()
//         {
//             _path = Path.Combine(Directory.GetCurrentDirectory(), "CSV_DATA", "fees.csv");

//             if (!File.Exists(_path))
//             {
//                 File.WriteAllText(_path,
//                     "id,student_name,gender,fees_type,amount,paid_date,status");
//             }
//         }

//         private List<FeeModel> Parse()
//         {
//             var list = new List<FeeModel>();
//             string[] lines = File.ReadAllLines(_path);

//             var pattern = ",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))";

//             for (int i = 1; i < lines.Length; i++)
//             {
//                 var v = Regex.Split(lines[i], pattern);
//                 if (v.Length < 7) continue;

//                 list.Add(new FeeModel
//                 {
//                     Id = v[0],
//                     StudentName = v[1],
//                     Gender = v[2],
//                     FeesType = v[3],
//                     Amount = decimal.TryParse(v[4], out var a) ? a : 0,
//                     PaidDate = DateTime.TryParse(v[5], out var d) ? d : DateTime.MinValue,
//                     Status = v[6]
//                 });
//             }

//             return list;
//         }

//         private string Format(FeeModel m)
//         {
//             return $"{m.Id},{m.StudentName},{m.Gender},{m.FeesType},{m.Amount},{m.PaidDate:yyyy-MM-dd},{m.Status}";
//         }

//         public List<FeeModel> GetAll() => Parse();

//         public FeeModel GetById(string id)
//             => Parse().FirstOrDefault(x => x.Id == id);

//         public void Add(FeeModel m)
//         {
//             File.AppendAllText(_path, Environment.NewLine + Format(m));
//         }

//         public void Update(FeeModel m)
//         {
//             var lines = File.ReadAllLines(_path).ToList();

//             for (int i = 1; i < lines.Count; i++)
//             {
//                 if (lines[i].Split(',')[0] == m.Id)
//                 {
//                     lines[i] = Format(m);
//                     break;
//                 }
//             }

//             File.WriteAllLines(_path, lines);
//         }

//         public void Delete(string id)
//         {
//             var lines = File.ReadAllLines(_path)
//                             .Where(l => l.Split(',')[0] != id)
//                             .ToList();

//             File.WriteAllLines(_path, lines);
//         }
//     }
// }
