using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SIMS_FPT.Data.Repositories
{
    public class TeacherRepository : ITeacherRepository
    {
        private readonly string _path;

        public TeacherRepository()
        {
            _path = Path.Combine(Directory.GetCurrentDirectory(), "CSV_DATA", "teachers.csv");

            if (!File.Exists(_path))
            {
                var header = "teacher_id,name,gender,date_of_birth,mobile,joining_date,qualification,experience,username,email,password,address,city,state,country,image";
                File.WriteAllText(_path, header);
            }
        }

        public List<TeacherCSVModel> GetAll()
        {
            var list = new List<TeacherCSVModel>();
            string[] lines = File.ReadAllLines(_path);

            var pattern = ",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))";

            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;

                var values = Regex.Split(line, pattern).Select(v => v.Trim('"')).ToArray();
                if (values.Length < 16) continue;

                list.Add(new TeacherCSVModel
                {
                    TeacherId = values[0],
                    Name = values[1],
                    Gender = values[2],
                    DateOfBirth = DateTime.TryParse(values[3], out var dob) ? dob : DateTime.MinValue,
                    Mobile = values[4],
                    JoiningDate = DateTime.TryParse(values[5], out var join) ? join : DateTime.MinValue,
                    Qualification = values[6],
                    Experience = values[7],
                    Username = values[8],
                    Email = values[9],
                    Password = values[10],
                    Address = values[11],
                    City = values[12],
                    State = values[13],
                    Country = values[14],
                    ImagePath = values[15]
                });
            }

            return list;
        }

        public TeacherCSVModel GetById(string id)
        {
            return GetAll().FirstOrDefault(t => t.TeacherId == id);
        }

        public void Add(TeacherCSVModel model)
        {
            string line = Format(model);
            File.AppendAllText(_path, Environment.NewLine + line);
        }

        public void Update(TeacherCSVModel model)
        {
            var all = GetAll();
            int idx = all.FindIndex(t => t.TeacherId == model.TeacherId);

            if (idx < 0) return;

            all[idx] = model;

            var header = File.ReadLines(_path).First();
            var lines = new List<string> { header };
            lines.AddRange(all.Select(Format));

            File.WriteAllLines(_path, lines);
        }

        public void Delete(string id)
        {
            var all = GetAll().Where(t => t.TeacherId != id).ToList();

            var header = File.ReadLines(_path).First();
            var lines = new List<string> { header };
            lines.AddRange(all.Select(Format));

            File.WriteAllLines(_path, lines);
        }

        private string Format(TeacherCSVModel m)
        {
            return string.Join(",",
                m.TeacherId,
                $"\"{m.Name}\"",
                m.Gender,
                m.DateOfBirth.ToString("yyyy-MM-dd"),
                m.Mobile,
                m.JoiningDate.ToString("yyyy-MM-dd"),
                $"\"{m.Qualification}\"",
                $"\"{m.Experience}\"",
                m.Username,
                m.Email,
                m.Password,
                $"\"{m.Address}\"",
                $"\"{m.City}\"",
                $"\"{m.State}\"",
                m.Country,
                m.ImagePath ?? ""
            );
        }
    }
}
