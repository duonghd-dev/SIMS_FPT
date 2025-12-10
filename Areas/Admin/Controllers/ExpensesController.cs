using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Models;

namespace SIMS_FPT.Areas.Admin.Controllers
{
    [Area("Admin")]
    // [Authorize(Roles = "Admin")] // Bỏ comment khi bạn đã làm xong chức năng login
    public class ExpensesController : Controller
    {
        private readonly IExpenseRepository _repo;

        public ExpensesController(IExpenseRepository repo)
        {
            _repo = repo;
        }

        public IActionResult List()
        {
            return View(_repo.GetAll());
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Add(ExpenseModel m)
        {
            if (ModelState.IsValid)
            {
                _repo.Add(m);
                return RedirectToAction("List");
            }
            return View(m);
        }

        [HttpGet]
        public IActionResult Edit(string id)
        {
            var item = _repo.GetById(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost]
        public IActionResult Edit(ExpenseModel m)
        {
            if (ModelState.IsValid)
            {
                _repo.Update(m);
                return RedirectToAction("List");
            }
            return View(m);
        }

        public IActionResult Delete(string id)
        {
            _repo.Delete(id);
            return RedirectToAction("List");
        }
    }
}