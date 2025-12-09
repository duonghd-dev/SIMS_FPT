using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SIMS_FPT.Data.Repositories
{
    public class StudentRepository : IStudentRepository
    {
        private readonly string _csvFilePath;

        public StudentRepository()
        {
            _csvFilePath = Path.Combine(Directory.GetCurrentDirectory(), "CSV_DATA", "students.csv");
        }

        public List<StudentCSVModel> GetAll()
        {
            var students = new List<StudentCSVModel>();

            if (!File.Exists(_csvFilePath)) return students;

            string[] lines = File.ReadAllLines(_csvFilePath);

            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;

                var pattern = ",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))";
                var values = Regex.Split(line, pattern);
                for (int j = 0; j < values.Length; j++)
                    values[j] = values[j].Trim('"');

                if (values.Length > 20)
                {
                    try
                    {
                        students.Add(new StudentCSVModel
                        {
                            FirstName = values[0],
                            LastName = values[1],
                            StudentId = values[2],
                            Gender = values[3],
                            DateOfBirth = DateTime.TryParse(values[4], out var dob) ? dob : DateTime.MinValue,
                            ClassName = values[5],
                            Religion = values[6],
                            JoiningDate = DateTime.TryParse(values[7], out var join) ? join : DateTime.MinValue,
                            MobileNumber = values[8],
                            AdmissionNumber = values[9],
                            Section = values[10],
                            ImagePath = values[11],
                            FatherName = values[12],
                            FatherOccupation = values[13],
                            FatherMobile = values[14],
                            FatherEmail = values[15],
                            MotherName = values[16],
                            MotherOccupation = values[17],
                            MotherMobile = values[18],
                            MotherEmail = values[19],
                            Address = values[20],
                            PermanentAddress = values.Length > 21 ? values[21] : ""
                        });
                    }
                    catch { }
                }
            }

            return students;
        }

        public StudentCSVModel GetById(string id)
        {
            return GetAll().FirstOrDefault(s => s.StudentId == id);
        }

        public void Add(StudentCSVModel student)
        {
            string line = Format(student);
            File.AppendAllText(_csvFilePath, Environment.NewLine + line);
        }

        public void Update(StudentCSVModel student)
        {
            string[] lines = File.ReadAllLines(_csvFilePath);
            var newLines = new List<string> { lines[0] };

            for (int i = 1; i < lines.Length; i++)
            {
                var cols = lines[i].Split(',');
                string currentId = cols.Length > 2 ? cols[2] : "";

                if (currentId == student.StudentId)
                    newLines.Add(Format(student));
                else
                    newLines.Add(lines[i]);
            }

            File.WriteAllLines(_csvFilePath, newLines);
        }

        public void Delete(string id)
        {
            string[] lines = File.ReadAllLines(_csvFilePath);
            var newLines = new List<string> { lines[0] };

            for (int i = 1; i < lines.Length; i++)
            {
                var cols = lines[i].Split(',');
                string currentId = cols.Length > 2 ? cols[2] : "";

                if (currentId != id)
                    newLines.Add(lines[i]);
            }

            File.WriteAllLines(_csvFilePath, newLines);
        }

        private string Format(StudentCSVModel m)
        {
            return string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},\"{20}\",\"{21}\"",
                m.FirstName, m.LastName, m.StudentId, m.Gender,
                m.DateOfBirth.ToString("yyyy-MM-dd"),
                m.ClassName, m.Religion,
                m.JoiningDate.ToString("yyyy-MM-dd"),
                m.MobileNumber, m.AdmissionNumber,
                m.Section, m.ImagePath,
                m.FatherName, m.FatherOccupation,
                m.FatherMobile, m.FatherEmail,
                m.MotherName, m.MotherOccupation,
                m.MotherMobile, m.MotherEmail,
                m.Address, m.PermanentAddress);
        }
    }
}
