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
    public class SubjectController : Controller
    {
        private readonly string _csvFilePath;

        public SubjectController()
        {
            _csvFilePath = Path.Combine(Directory.GetCurrentDirectory(), "CSV_DATA", "subjects.csv");
        }

        // --- HELPER METHODS ---
        private List<SubjectModel> GetAllSubjects()
        {
            var list = new List<SubjectModel>();
            if (!System.IO.File.Exists(_csvFilePath)) return list;

            string[] lines = System.IO.File.ReadAllLines(_csvFilePath);
            // Bỏ qua header (i=1)
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;

                // Regex tách dấu phẩy (xử lý trường hợp tên môn học có dấu phẩy trong ngoặc kép)
                var pattern = ",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))";
                var values = Regex.Split(line, pattern);
                for (int j = 0; j < values.Length; j++) values[j] = values[j].Trim('"');

                if (values.Length >= 3)
                {
                    list.Add(new SubjectModel
                    {
                        SubjectId = values[0],
                        SubjectName = values[1],
                        Class = values[2]
                    });
                }
            }
            return list;
        }

        private string FormatToCsvLine(SubjectModel m)
        {
            // Format: ID,"Name",Class
            return $"{m.SubjectId},\"{m.SubjectName}\",{m.Class}";
        }

        // --- ACTIONS ---

        // 1. List
        public IActionResult List()
        {
            var data = GetAllSubjects();
            return View(data);
        }

        // 2. Add
        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Add(SubjectModel model)
        {
            try
            {
                string line = FormatToCsvLine(model);
                System.IO.File.AppendAllText(_csvFilePath, Environment.NewLine + line);
                return RedirectToAction("List");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error: " + ex.Message);
                return View(model);
            }
        }

        // 3. Edit
        [HttpGet]
        public IActionResult Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var item = GetAllSubjects().FirstOrDefault(x => x.SubjectId == id);
            return item == null ? NotFound() : View(item);
        }

        [HttpPost]
        public IActionResult Edit(SubjectModel model)
        {
            try
            {
                string[] allLines = System.IO.File.ReadAllLines(_csvFilePath);
                var newContent = new List<string>();
                if (allLines.Length > 0) newContent.Add(allLines[0]); // Header

                bool found = false;
                for (int i = 1; i < allLines.Length; i++)
                {
                    var cols = allLines[i].Split(',');
                    // Giả định ID là cột đầu tiên
                    if (cols[0] == model.SubjectId)
                    {
                        newContent.Add(FormatToCsvLine(model));
                        found = true;
                    }
                    else
                    {
                        newContent.Add(allLines[i]);
                    }
                }

                if (found)
                {
                    System.IO.File.WriteAllLines(_csvFilePath, newContent);
                    return RedirectToAction("List");
                }

                ModelState.AddModelError("", "ID not found");
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error: " + ex.Message);
                return View(model);
            }
        }

        // 4. Delete
        public IActionResult Delete(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id)) return NotFound();
                string[] allLines = System.IO.File.ReadAllLines(_csvFilePath);
                var newContent = new List<string>();
                if (allLines.Length > 0) newContent.Add(allLines[0]);

                bool deleted = false;
                for (int i = 1; i < allLines.Length; i++)
                {
                    var cols = allLines[i].Split(',');
                    if (cols[0] != id) newContent.Add(allLines[i]);
                    else deleted = true;
                }

                if (deleted) System.IO.File.WriteAllLines(_csvFilePath, newContent);
                return RedirectToAction("List");
            }
            catch
            {
                return RedirectToAction("List");
            }
        }
    }
}