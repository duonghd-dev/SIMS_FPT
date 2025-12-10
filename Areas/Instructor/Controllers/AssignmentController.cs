using Microsoft.AspNetCore.Mvc;
using SIMS_FPT.Business.Interfaces;
using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Models;
using SIMS_FPT.Models.ViewModels;
using System;
using System.Collections.Generic; // Added for List
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
                if (string.IsNullOrEmpty(model.AssignmentId)) model.AssignmentId = Guid.NewGuid().ToString();
                _assignmentRepo.Add(model);
                return RedirectToAction("Index");
            }
            ViewBag.Subjects = _subjectRepo.GetAll();
            return View(model);
        }

        // GET: Instructor/Assignment/Grade/{id}
        [HttpGet]
        public IActionResult Grade(string id)
        {
            var assignment = _assignmentRepo.GetById(id);
            if (assignment == null) return NotFound();

            // 1. Get all students for this subject
            var students = _studentRepo.GetBySubject(assignment.SubjectId);

            // 2. Get existing submissions
            var submissions = _submissionRepo.GetByAssignmentId(id);

            // 3. Prepare ViewModel
            var model = new BulkGradeViewModel
            {
                AssignmentId = assignment.AssignmentId,
                AssignmentTitle = assignment.Title,
                MaxPoints = assignment.MaxPoints,
                IsPublished = assignment.AreGradesPublished,
                StudentGrades = new List<StudentGradeItem>()
            };

            foreach (var s in students)
            {
                var sub = submissions.FirstOrDefault(x => x.StudentId == s.StudentId);
                model.StudentGrades.Add(new StudentGradeItem
                {
                    StudentId = s.StudentId,
                    StudentName = s.FullName, // Assuming StudentModel has FullName
                    HasSubmitted = sub != null && !string.IsNullOrEmpty(sub.FilePath),
                    SubmissionFilePath = sub?.FilePath,
                    Grade = sub?.Grade,
                    Feedback = sub?.TeacherComments
                });
            }

            return View(model);
        }

        // POST: Instructor/Assignment/Grade
        [HttpPost]
        public IActionResult Grade(BulkGradeViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            _gradingService.ProcessGrades(model);

            // Redirect back to Index after saving
            return RedirectToAction("Index");
        }
    }
}