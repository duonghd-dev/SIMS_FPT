using Microsoft.AspNetCore.Mvc;
using SIMS_FPT.Business.Interfaces;
using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Models;
using SIMS_FPT.Models.ViewModels;
using System;
using System.Linq;

namespace SIMS_FPT.Areas.Instructor.Controllers
{
    [Area("Instructor")]
    public class AssignmentController : Controller
    {
        private readonly IAssignmentRepository _assignmentRepo;
        private readonly ISubjectRepository _subjectRepo;
        private readonly IStudentRepository _studentRepo;      
        private readonly ISubmissionRepository _submissionRepo;
        private readonly IGradingService _gradingService;

        public AssignmentController(IAssignmentRepository assignmentRepo,
                                    ISubjectRepository subjectRepo,
                                    IStudentRepository studentRepo,
                                    ISubmissionRepository submissionRepo,
                                    IGradingService gradingService)
        {
            _assignmentRepo = assignmentRepo;
            _subjectRepo = subjectRepo;
            _studentRepo = studentRepo;          
            _submissionRepo = submissionRepo;    
            _gradingService = gradingService;
        }

        public IActionResult Index()
        {
            var assignments = _assignmentRepo.GetAll();
            return View(assignments);
        }

        public IActionResult Create()
        {
            ViewBag.Subjects = _subjectRepo.GetAll();
            return View();
        }

        [HttpPost]
        public IActionResult Create(AssignmentModel model)
        {
            if (ModelState.IsValid)
            {
                model.AssignmentId = Guid.NewGuid().ToString();
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

            // Redirect back to Index after saving
            return RedirectToAction("Index");
        }
    }
}