// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using SIMS_FPT.Data.Interfaces;
// using SIMS_FPT.Models;

// namespace SIMS_FPT.Areas.Admin.Controllers
// {
//     [Area("Admin")]
//     [Authorize(Roles = "Admin")]
//     public class FeesController : Controller
//     {
//         private readonly IFeeRepository _repo;

//         public FeesController(IFeeRepository repo)
//         {
//             _repo = repo;
//         }

//         public IActionResult List()
//         {
//             return View(_repo.GetAll());
//         }

//         [HttpGet]
//         public IActionResult Add()
//         {
//             return View();
//         }

//         [HttpPost]
//         public IActionResult Add(FeeModel m)
//         {
//             _repo.Add(m);
//             return RedirectToAction("List");
//         }

//         [HttpGet]
//         public IActionResult Edit(string id)
//         {
//             var item = _repo.GetById(id);
//             if (item == null) return NotFound();
//             return View(item);
//         }

//         [HttpPost]
//         public IActionResult Edit(FeeModel m)
//         {
//             _repo.Update(m);
//             return RedirectToAction("List");
//         }

//         public IActionResult Delete(string id)
//         {
//             _repo.Delete(id);
//             return RedirectToAction("List");
//         }
//     }
// }
