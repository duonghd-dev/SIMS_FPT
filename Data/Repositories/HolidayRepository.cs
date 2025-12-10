// using SIMS_FPT.Data.Interfaces;
// using SIMS_FPT.Models;
// using System.Collections.Generic;
// using System.IO;
// using System.Linq;
// using System.Text.RegularExpressions;

// namespace SIMS_FPT.Data.Repositories
// {
//     public class HolidayRepository : IHolidayRepository
//     {
//         private readonly string _path;

//         public HolidayRepository()
//         {
//             _path = Path.Combine(Directory.GetCurrentDirectory(), "CSV_DATA", "holidays.csv");

//             if (!File.Exists(_path))
//             {
//                 var header = "id,name,type,start_date,end_date";
//                 File.WriteAllText(_path, header);
//             }
//         }

//         public List<HolidayModel> GetAll()
//         {
//             var list = new List<HolidayModel>();
//             string[] lines = File.ReadAllLines(_path);

//             var pattern = ",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))";

//             for (int i = 1; i < lines.Length; i++)
//             {
//                 string line = lines[i];
//                 if (string.IsNullOrWhiteSpace(line)) continue;

//                 var values = Regex.Split(line, pattern)
//                                   .Select(x => x.Trim('"'))
//                                   .ToArray();

//                 if (values.Length < 5) continue;

//                 list.Add(new HolidayModel
//                 {
//                     Id = values[0],
//                     Name = values[1],
//                     Type = values[2],
//                     StartDate = DateTime.TryParse(values[3], out var s) ? s : DateTime.MinValue,
//                     EndDate = DateTime.TryParse(values[4], out var e) ? e : DateTime.MinValue,
//                 });
//             }

//             return list;
//         }

//         public HolidayModel GetById(string id)
//         {
//             return GetAll().FirstOrDefault(x => x.Id == id);
//         }

//         public void Add(HolidayModel m)
//         {
//             string line = Format(m);
//             File.AppendAllText(_path, Environment.NewLine + line);
//         }

//         public void Update(HolidayModel m)
//         {
//             var all = GetAll();
//             int idx = all.FindIndex(x => x.Id == m.Id);

//             if (idx < 0) return;

//             all[idx] = m;

//             var header = File.ReadLines(_path).First();
//             var lines = new List<string> { header };
//             lines.AddRange(all.Select(Format));

//             File.WriteAllLines(_path, lines);
//         }

//         public void Delete(string id)
//         {
//             var all = GetAll().Where(x => x.Id != id).ToList();

//             var header = File.ReadLines(_path).First();
//             var lines = new List<string> { header };
//             lines.AddRange(all.Select(Format));

//             File.WriteAllLines(_path, lines);
//         }

//         private string Format(HolidayModel m)
//         {
//             return $"{m.Id},\"{m.Name}\",\"{m.Type}\",{m.StartDate:yyyy-MM-dd},{m.EndDate:yyyy-MM-dd}";
//         }
//     }
// }
