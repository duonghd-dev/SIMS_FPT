using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Models;
using System.Collections.Generic;
using System.Linq;
using SIMS_FPT.Models.ViewModels;
using System.Security.Claims;

namespace SIMS_FPT.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "Student")]
    public class CourseController : Controller
    {
        private readonly IStudentClassRepository _studentClassRepo;
        private readonly IClassRepository _classRepo;
        private readonly ICourseMaterialRepository _materialRepo;
        private readonly ISubjectRepository _subjectRepo;

        public CourseController(IStudentClassRepository studentClassRepo,
                                IClassRepository classRepo,
                                ICourseMaterialRepository materialRepo, ISubjectRepository subjectRepo)
        {
            _studentClassRepo = studentClassRepo;
            _classRepo = classRepo;
            _materialRepo = materialRepo;
            _subjectRepo = subjectRepo;
        }

        public IActionResult Index()
        {
            var studentId = User.FindFirst("LinkedId")?.Value;
            if (string.IsNullOrEmpty(studentId)) return RedirectToAction("Dashboard", "Home");

            var enrollments = _studentClassRepo.GetByStudentId(studentId);
            var myCourses = new List<MyCourseViewModel>();

            foreach (var enrollment in enrollments)
            {
                var classInfo = _classRepo.GetById(enrollment.ClassId);
                if (classInfo != null)
                {
                    // Correctly get the subject name for display purposes
                    var subject = _subjectRepo.GetAll().FirstOrDefault(s => s.SubjectId == classInfo.SubjectId);
                    // classInfo.SubjectName = subject?.SubjectName ?? "Unknown"; // Assuming you add this property to ClassModel for display

                    // Directly use the SubjectId from the class info to get materials
                    var materials = _materialRepo.GetAll()
                     .Where(m => m.SubjectId == classInfo.SubjectId && m.TeacherId == classInfo.TeacherName)
                     .OrderByDescending(m => m.UploadDate)
                     .ToList();

                    myCourses.Add(new MyCourseViewModel
                    {
                        ClassInfo = classInfo,
                        Materials = materials
                    });
                }
            }

            return View(myCourses);
        }
    }
}

