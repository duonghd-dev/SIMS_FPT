using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using SIMS_FPT.Models;
using System.Text.RegularExpressions;

namespace SIMS_FPT.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DepartmentController : Controller
    {
        private readonly string _csvFilePath;

        public DepartmentController()
        {
            // Đường dẫn file CSV: ProjectRoot/CSV_DATA/departments.csv
            _csvFilePath = Path.Combine(Directory.GetCurrentDirectory(), "CSV_DATA", "departments.csv");
        }

        // --- HELPER METHODS ---
        private List<DepartmentModel> GetAllDepartments()
        {
            var list = new List<DepartmentModel>();
            if (!System.IO.File.Exists(_csvFilePath)) return list;

            string[] lines = System.IO.File.ReadAllLines(_csvFilePath);
            // Bỏ qua header (i=1)
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;

                // Tách CSV (xử lý dấu phẩy)
                var pattern = ",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))";
                var values = Regex.Split(line, pattern);
                for (int j = 0; j < values.Length; j++) values[j] = values[j].Trim('"');

                if (values.Length >= 5)
                {
                    try
                    {
                        list.Add(new DepartmentModel
                        {
                            DepartmentId = values[0],
                            DepartmentName = values[1],
                            HeadOfDepartment = values[2],
                            StartDate = DateTime.TryParse(values[3], out var d) ? d : DateTime.MinValue,
                            NoOfStudents = int.TryParse(values[4], out var n) ? n : 0
                        });
                    }
                    catch { }
                }
            }
            return list;
        }

        private string FormatToCsvLine(DepartmentModel m)
        {
            return $"{m.DepartmentId},\"{m.DepartmentName}\",\"{m.HeadOfDepartment}\",{m.StartDate:yyyy-MM-dd},{m.NoOfStudents}";
        }

        // 1. List
        public IActionResult List()
        {
            var data = GetAllDepartments();
            return View(data);
        }

        // 2. Add
        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Add(DepartmentModel model)
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
            var item = GetAllDepartments().FirstOrDefault(x => x.DepartmentId == id);
            return item == null ? NotFound() : View(item);
        }

        [HttpPost]
        public IActionResult Edit(DepartmentModel model)
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
                    if (cols[0] == model.DepartmentId)
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