using Microsoft.AspNetCore.Mvc;
using SIMS_FPT.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
namespace SIMS_FPT.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class HolidayController : Controller
    {
        private readonly string _csvFilePath;

        public HolidayController()
        {
            _csvFilePath = Path.Combine(Directory.GetCurrentDirectory(), "CSV_DATA", "holidays.csv");
        }

        // --- HELPER METHODS ---
        private List<HolidayModel> GetAllHolidays()
        {
            var list = new List<HolidayModel>();
            if (!System.IO.File.Exists(_csvFilePath)) return list;

            string[] lines = System.IO.File.ReadAllLines(_csvFilePath);
            // Bỏ qua header
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;

                // Tách CSV (xử lý dấu phẩy trong ngoặc kép)
                var pattern = ",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))";
                var values = Regex.Split(line, pattern);
                for (int j = 0; j < values.Length; j++) values[j] = values[j].Trim('"');

                if (values.Length >= 5)
                {
                    try
                    {
                        list.Add(new HolidayModel
                        {
                            Id = values[0],
                            Name = values[1],
                            Type = values[2],
                            StartDate = DateTime.TryParse(values[3], out var s) ? s : DateTime.MinValue,
                            EndDate = DateTime.TryParse(values[4], out var e) ? e : DateTime.MinValue
                        });
                    }
                    catch { }
                }
            }
            return list;
        }

        private string FormatToCsvLine(HolidayModel m)
        {
            // Format: id,name,type,start_date,end_date
            return $"{m.Id},\"{m.Name}\",\"{m.Type}\",{m.StartDate:yyyy-MM-dd},{m.EndDate:yyyy-MM-dd}";
        }

        // --- ACTIONS ---

        // 1. List
        public IActionResult List()
        {
            var data = GetAllHolidays();
            return View(data);
        }

        // 2. Add
        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Add(HolidayModel model)
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

        // 3. Edit (Bạn chưa gửi giao diện Edit nhưng tôi làm sẵn luôn để đồng bộ)
        [HttpGet]
        public IActionResult Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var item = GetAllHolidays().FirstOrDefault(x => x.Id == id);
            return item == null ? NotFound() : View(item);
        }

        [HttpPost]
        public IActionResult Edit(HolidayModel model)
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
                    if (cols[0] == model.Id)
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