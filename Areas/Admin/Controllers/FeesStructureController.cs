using Microsoft.AspNetCore.Mvc;
using SIMS_FPT.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SIMS_FPT.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class FeesStructureController : Controller
    {
        private readonly string _csvFilePath;
        private readonly string _studentsPath; // Đường dẫn file Students

        public FeesStructureController()
        {
            _csvFilePath = Path.Combine(Directory.GetCurrentDirectory(), "CSV_DATA", "fees_structure.csv");
            _studentsPath = Path.Combine(Directory.GetCurrentDirectory(), "CSV_DATA", "students.csv");
        }

        // --- HELPER METHODS ---
        
        // 1. Lấy danh sách Class duy nhất từ students.csv
        private List<string> GetUniqueClasses()
        {
            var classList = new List<string>();
            if (!System.IO.File.Exists(_studentsPath)) return classList;

            string[] lines = System.IO.File.ReadAllLines(_studentsPath);
            // Bỏ qua header (i=1)
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;

                var pattern = ",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))";
                var values = Regex.Split(line, pattern);

                // Cột Class nằm ở vị trí index 5 (Theo header bạn gửi: first_name,last_name,student_id,gender,date_of_birth,class...)
                if (values.Length > 5)
                {
                    string className = values[5].Trim().Trim('"');
                    if (!string.IsNullOrEmpty(className))
                    {
                        classList.Add(className);
                    }
                }
            }
            // Lọc trùng và sắp xếp
            return classList.Distinct().OrderBy(x => x).ToList();
        }

        private List<FeesStructureModel> GetAll()
        {
            var list = new List<FeesStructureModel>();
            if (!System.IO.File.Exists(_csvFilePath)) return list;

            string[] lines = System.IO.File.ReadAllLines(_csvFilePath);
            for (int i = 1; i < lines.Length; i++)
            {
                var val = Regex.Split(lines[i], ",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
                if (val.Length >= 6)
                {
                    try {
                        list.Add(new FeesStructureModel {
                            Id = val[0], FeesName = val[1], Class = val[2],
                            Amount = decimal.TryParse(val[3], out var a) ? a : 0,
                            StartDate = DateTime.TryParse(val[4], out var s) ? s : DateTime.MinValue,
                            EndDate = DateTime.TryParse(val[5], out var e) ? e : DateTime.MinValue
                        });
                    } catch {}
                }
            }
            return list;
        }

        private string FormatCsv(FeesStructureModel m) => 
            $"{m.Id},{m.FeesName},{m.Class},{m.Amount},{m.StartDate:yyyy-MM-dd},{m.EndDate:yyyy-MM-dd}";


        // --- ACTIONS ---

        public IActionResult List() => View(GetAll());

        [HttpGet] 
        public IActionResult Add() 
        {
            ViewBag.ClassList = GetUniqueClasses(); // Truyền danh sách lớp sang View
            return View();
        }
        
        [HttpPost] 
        public IActionResult Add(FeesStructureModel m)
        {
            try {
                System.IO.File.AppendAllText(_csvFilePath, Environment.NewLine + FormatCsv(m));
                return RedirectToAction("List");
            } catch {
                ViewBag.ClassList = GetUniqueClasses();
                return View(m);
            }
        }

        [HttpGet] 
        public IActionResult Edit(string id)
        {
            var item = GetAll().FirstOrDefault(x => x.Id == id);
            ViewBag.ClassList = GetUniqueClasses(); // Truyền danh sách lớp sang View
            return item == null ? NotFound() : View(item);
        }

        [HttpPost] 
        public IActionResult Edit(FeesStructureModel m)
        {
            try {
                var lines = System.IO.File.ReadAllLines(_csvFilePath).ToList();
                for(int i=1; i<lines.Count; i++) {
                    if(lines[i].Split(',')[0] == m.Id) {
                        lines[i] = FormatCsv(m);
                        break;
                    }
                }
                System.IO.File.WriteAllLines(_csvFilePath, lines);
                return RedirectToAction("List");
            } catch {
                ViewBag.ClassList = GetUniqueClasses();
                return View(m);
            }
        }

        public IActionResult Delete(string id)
        {
            var lines = System.IO.File.ReadAllLines(_csvFilePath).Where(l => l.Split(',')[0] != id).ToList();
            System.IO.File.WriteAllLines(_csvFilePath, lines);
            return RedirectToAction("List");
        }
    }
}