using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Models;

namespace SIMS_FPT.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class HolidayController : Controller
    {
        private readonly IHolidayRepository _repo;

        public HolidayController(IHolidayRepository repo)
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
        public IActionResult Add(HolidayModel model)
        {
            _repo.Add(model);
            return RedirectToAction("List");
        }

        [HttpGet]
        public IActionResult Edit(string id)
        {
            var item = _repo.GetById(id);
            if (item == null) return NotFound();

            return View(item);
        }

        [HttpPost]
        public IActionResult Edit(HolidayModel model)
        {
            _repo.Update(model);
            return RedirectToAction("List");
        }

        public IActionResult Delete(string id)
        {
            _repo.Delete(id);
            return RedirectToAction("List");
        }
    }
}
