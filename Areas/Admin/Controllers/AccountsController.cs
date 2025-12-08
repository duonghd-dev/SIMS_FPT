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
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AccountsController : Controller
    {
        // Khai báo đường dẫn cho 3 file CSV
        private readonly string _expensesPath = Path.Combine(Directory.GetCurrentDirectory(), "CSV_DATA", "expenses.csv");
        private readonly string _feesPath = Path.Combine(Directory.GetCurrentDirectory(), "CSV_DATA", "fees.csv");
        private readonly string _salaryPath = Path.Combine(Directory.GetCurrentDirectory(), "CSV_DATA", "salary.csv");

        // ============================================================
        // 1. MODULE EXPENSES (Chi tiêu)
        // ============================================================
        #region EXPENSES

        private List<ExpenseModel> GetExpenses()
        {
            var list = new List<ExpenseModel>();
            if (!System.IO.File.Exists(_expensesPath)) return list;
            var lines = System.IO.File.ReadAllLines(_expensesPath);
            for (int i = 1; i < lines.Length; i++)
            {
                var val = Regex.Split(lines[i], ",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
                if (val.Length >= 7)
                {
                    try {
                        list.Add(new ExpenseModel {
                            Id = val[0], ItemName = val[1], ItemQuality = val[2],
                            Amount = decimal.TryParse(val[3], out var a) ? a : 0,
                            PurchaseSource = val[4],
                            PurchaseDate = DateTime.TryParse(val[5], out var d) ? d : DateTime.MinValue,
                            PurchaseBy = val[6]
                        });
                    } catch {}
                }
            }
            return list;
        }

        // Action: Hiển thị danh sách Expenses
        public IActionResult Expenses()
        {
            return View("Expenses/List", GetExpenses());
        }

        // Action: Thêm Expense
        [HttpGet]
        public IActionResult AddExpense() => View("Expenses/Add");
        
        [HttpPost]
        public IActionResult AddExpense(ExpenseModel m)
        {
            string line = $"{m.Id},{m.ItemName},{m.ItemQuality},{m.Amount},{m.PurchaseSource},{m.PurchaseDate:yyyy-MM-dd},{m.PurchaseBy}";
            System.IO.File.AppendAllText(_expensesPath, Environment.NewLine + line);
            return RedirectToAction("Expenses");
        }

        // Action: Sửa Expense
        [HttpGet]
        public IActionResult EditExpense(string id)
        {
            var item = GetExpenses().FirstOrDefault(x => x.Id == id);
            return item == null ? NotFound() : View("Expenses/Edit", item);
        }

        [HttpPost]
        public IActionResult EditExpense(ExpenseModel m)
        {
            var lines = System.IO.File.ReadAllLines(_expensesPath).ToList();
            for(int i=1; i<lines.Count; i++) {
                if(lines[i].Split(',')[0] == m.Id) {
                    lines[i] = $"{m.Id},{m.ItemName},{m.ItemQuality},{m.Amount},{m.PurchaseSource},{m.PurchaseDate:yyyy-MM-dd},{m.PurchaseBy}";
                    break;
                }
            }
            System.IO.File.WriteAllLines(_expensesPath, lines);
            return RedirectToAction("Expenses");
        }
        
        // Action: Xóa Expense
        public IActionResult DeleteExpense(string id)
        {
             var lines = System.IO.File.ReadAllLines(_expensesPath).Where(l => l.Split(',')[0] != id).ToList();
             System.IO.File.WriteAllLines(_expensesPath, lines);
             return RedirectToAction("Expenses");
        }
        #endregion

        // ============================================================
        // 2. MODULE FEES (Thu phí)
        // ============================================================
        #region FEES

        private List<FeeModel> GetFees()
        {
            var list = new List<FeeModel>();
            if (!System.IO.File.Exists(_feesPath)) return list;
            var lines = System.IO.File.ReadAllLines(_feesPath);
            for (int i = 1; i < lines.Length; i++)
            {
                var val = Regex.Split(lines[i], ",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
                if (val.Length >= 7)
                {
                    try {
                        list.Add(new FeeModel {
                            Id = val[0], StudentName = val[1], Gender = val[2], FeesType = val[3],
                            Amount = decimal.TryParse(val[4], out var a) ? a : 0,
                            PaidDate = DateTime.TryParse(val[5], out var d) ? d : DateTime.MinValue,
                            Status = val[6]
                        });
                    } catch {}
                }
            }
            return list;
        }

        public IActionResult Fees() => View("Fees/List", GetFees());

        [HttpGet] public IActionResult AddFee() => View("Fees/Add");

        [HttpPost] public IActionResult AddFee(FeeModel m)
        {
            string line = $"{m.Id},{m.StudentName},{m.Gender},{m.FeesType},{m.Amount},{m.PaidDate:yyyy-MM-dd},{m.Status}";
            System.IO.File.AppendAllText(_feesPath, Environment.NewLine + line);
            return RedirectToAction("Fees");
        }

        [HttpGet] public IActionResult EditFee(string id) => View("Fees/Edit", GetFees().FirstOrDefault(x => x.Id == id));

        [HttpPost] public IActionResult EditFee(FeeModel m)
        {
            var lines = System.IO.File.ReadAllLines(_feesPath).ToList();
            for(int i=1; i<lines.Count; i++) {
                if(lines[i].Split(',')[0] == m.Id) {
                    lines[i] = $"{m.Id},{m.StudentName},{m.Gender},{m.FeesType},{m.Amount},{m.PaidDate:yyyy-MM-dd},{m.Status}";
                    break;
                }
            }
            System.IO.File.WriteAllLines(_feesPath, lines);
            return RedirectToAction("Fees");
        }

        public IActionResult DeleteFee(string id)
        {
             var lines = System.IO.File.ReadAllLines(_feesPath).Where(l => l.Split(',')[0] != id).ToList();
             System.IO.File.WriteAllLines(_feesPath, lines);
             return RedirectToAction("Fees");
        }
        #endregion

        // ============================================================
        // 3. MODULE SALARY (Lương)
        // ============================================================
        #region SALARY

        private List<SalaryModel> GetSalaries()
        {
            var list = new List<SalaryModel>();
            if (!System.IO.File.Exists(_salaryPath)) return list;
            var lines = System.IO.File.ReadAllLines(_salaryPath);
            for (int i = 1; i < lines.Length; i++)
            {
                var val = Regex.Split(lines[i], ",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
                if (val.Length >= 6)
                {
                    try {
                        list.Add(new SalaryModel {
                            StaffId = val[0], Name = val[1], Gender = val[2],
                            JoiningDate = DateTime.TryParse(val[3], out var d) ? d : DateTime.MinValue,
                            Amount = decimal.TryParse(val[4], out var a) ? a : 0,
                            Status = val[5]
                        });
                    } catch {}
                }
            }
            return list;
        }

        public IActionResult Salary() => View("Salary/List", GetSalaries());

        [HttpGet] public IActionResult AddSalary() => View("Salary/Add");

        [HttpPost] public IActionResult AddSalary(SalaryModel m)
        {
            string line = $"{m.StaffId},{m.Name},{m.Gender},{m.JoiningDate:yyyy-MM-dd},{m.Amount},{m.Status}";
            System.IO.File.AppendAllText(_salaryPath, Environment.NewLine + line);
            return RedirectToAction("Salary");
        }

        [HttpGet] public IActionResult EditSalary(string id) => View("Salary/Edit", GetSalaries().FirstOrDefault(x => x.StaffId == id));

        [HttpPost] public IActionResult EditSalary(SalaryModel m)
        {
            var lines = System.IO.File.ReadAllLines(_salaryPath).ToList();
            for(int i=1; i<lines.Count; i++) {
                if(lines[i].Split(',')[0] == m.StaffId) {
                    lines[i] = $"{m.StaffId},{m.Name},{m.Gender},{m.JoiningDate:yyyy-MM-dd},{m.Amount},{m.Status}";
                    break;
                }
            }
            System.IO.File.WriteAllLines(_salaryPath, lines);
            return RedirectToAction("Salary");
        }

        public IActionResult DeleteSalary(string id)
        {
             var lines = System.IO.File.ReadAllLines(_salaryPath).Where(l => l.Split(',')[0] != id).ToList();
             System.IO.File.WriteAllLines(_salaryPath, lines);
             return RedirectToAction("Salary");
        }
        #endregion
    }
}