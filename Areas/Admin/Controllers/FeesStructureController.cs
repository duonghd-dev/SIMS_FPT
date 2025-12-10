// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using SIMS_FPT.Data.Interfaces;
// using SIMS_FPT.Models;

// namespace SIMS_FPT.Areas.Admin.Controllers
// {
//     [Authorize(Roles = "Admin")]
//     [Area("Admin")]
//     public class FeesStructureController : Controller
//     {
//         private readonly IFeesStructureRepository _repo;

//         public FeesStructureController(IFeesStructureRepository repo)
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
//             ViewBag.ClassList = _repo.GetUniqueClasses();
//             return View();
//         }

//         [HttpPost]
//         public IActionResult Add(FeesStructureModel m)
//         {
//             _repo.Add(m);
//             return RedirectToAction("List");
//         }

//         [HttpGet]
//         public IActionResult Edit(string id)
//         {
//             var item = _repo.GetById(id);
//             if (item == null) return NotFound();

//             ViewBag.ClassList = _repo.GetUniqueClasses();
//             return View(item);
//         }

//         [HttpPost]
//         public IActionResult Edit(FeesStructureModel m)
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
