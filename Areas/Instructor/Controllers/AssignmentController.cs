// Areas/Instructor/Controllers/AssignmentController.cs
using Microsoft.AspNetCore.Mvc;
using SIMS_FPT.Business.Interfaces;
using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Models;
using SIMS_FPT.Models.ViewModels;
using System.Collections.Generic;

public class AssignmentController : Controller
{
    private readonly IAssignmentRepository _assignmentRepo;
    private readonly IGradingService _gradingService;
    private readonly ISubjectRepository _subjectRepo;

    public AssignmentController(IAssignmentRepository assignmentRepo, ISubjectRepository subjectRepo, IGradingService gradingService)
    {
        _assignmentRepo = assignmentRepo;
        _subjectRepo = subjectRepo;
        _gradingService = gradingService;
    }
    //GET: List all assignments
        public IActionResult Index()
    {
        var assignments = _assignmentRepo.GetAll();
        // In reality, filter this by the logged-in teacher's ID!
        return View(assignments);
    }

    // GET: Create
    public IActionResult Create()
    {
        ViewBag.Subjects = _subjectRepo.GetAll(); // Populate dropdown
        return View();
    }

    // POST: Create
    [HttpPost]
    public IActionResult Create(AssignmentModel model)
    {
        if (ModelState.IsValid)
        {
            model.AssignmentId = Guid.NewGuid().ToString(); // Generate ID
            _assignmentRepo.Add(model);
            return RedirectToAction("Index");
        }
        ViewBag.Subjects = _subjectRepo.GetAll();
        return View(model);
    }
    [HttpPost]
    public IActionResult BulkGrade(BulkGradeViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        _gradingService.ProcessBulkGrades(model);
        return RedirectToAction("Dashboard");
    }
}