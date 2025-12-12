using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Models;
using System.Security.Claims;
using System.Linq;
using System.Collections.Generic;
using System;

namespace SIMS_FPT.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "Student")]
    public class ClassController : Controller
    {
        private readonly IClassRepository _classRepo;
        private readonly IStudentClassRepository _studentClassRepo;
        private readonly IStudentRepository _studentRepo;

        public ClassController(IClassRepository classRepo, 
                               IStudentClassRepository studentClassRepo,
                               IStudentRepository studentRepo)
        {
            _classRepo = classRepo;
            _studentClassRepo = studentClassRepo;
            _studentRepo = studentRepo;
        }

        public IActionResult Index()
        {
            var studentId = User.FindFirst("LinkedId")?.Value;

            if (string.IsNullOrEmpty(studentId))
            {
                TempData["Error"] = "Your account is not linked to any student profile.";
                return View(new List<ClassViewModel>());
            }

            var allClasses = _classRepo.GetAll();
            var model = allClasses.Select(c => new ClassViewModel
            {
                Class = c,
                IsJoined = _studentClassRepo.IsEnrolled(c.ClassId, studentId) 
            }).ToList();

            return View(model);
        }

        [HttpPost]
        public IActionResult Join(string classId)
        {
            var studentId = User.FindFirst("LinkedId")?.Value;

            if (string.IsNullOrEmpty(studentId))
            {
                TempData["Error"] = "Error: Student ID not found.";
                return RedirectToAction("Index");
            }

            if (!_studentClassRepo.IsEnrolled(classId, studentId))
            {
                var enrollment = new StudentClassModel
                {
                    EnrollmentId = Guid.NewGuid().ToString(),
                    ClassId = classId,
                    StudentId = studentId,
                    JoinedDate = DateTime.Now
                };
                _studentClassRepo.Add(enrollment); 
                TempData["Success"] = "Enrolled successfully!";
            }
            else
            {
                TempData["Warning"] = "You are already enrolled in this class.";
            }

            return RedirectToAction("Index");
        }

        public IActionResult Classmates(string classId)
        {
            var classInfo = _classRepo.GetById(classId);
            if (classInfo == null) return NotFound();

            var enrollments = _studentClassRepo.GetByClassId(classId);
            var students = new List<StudentCSVModel>();
            
            foreach (var item in enrollments)
            {
                var student = _studentRepo.GetById(item.StudentId);
                if (student != null) students.Add(student);
            }

            ViewBag.ClassName = classInfo.ClassName;
            return View(students);
        }
    }

    public class ClassViewModel
    {
        public ClassModel Class { get; set; }
        public bool IsJoined { get; set; }
    }
}