using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Helpers;
using SIMS_FPT.Models;
using SIMS_FPT.Models.ViewModels;
using System.Linq;

namespace SIMS_FPT.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ClassController : Controller
    {
        private readonly IClassRepository _classRepo;
        private readonly ISubjectRepository _subjectRepo;
        private readonly ITeacherRepository _teacherRepo;
        private readonly IDepartmentRepository _deptRepo;
        private readonly IStudentRepository _studentRepo;
        private readonly IStudentClassRepository _studentClassRepo;
        private readonly IClassSubjectRepository _classSubjectRepo;

        public ClassController(
            IClassRepository classRepo,
            ISubjectRepository subjectRepo,
            ITeacherRepository teacherRepo,
            IDepartmentRepository deptRepo,
            IStudentRepository studentRepo,
            IStudentClassRepository studentClassRepo,
            IClassSubjectRepository classSubjectRepo)
        {
            _classRepo = classRepo;
            _subjectRepo = subjectRepo;
            _teacherRepo = teacherRepo;
            _deptRepo = deptRepo;
            _studentRepo = studentRepo;
            _studentClassRepo = studentClassRepo;
            _classSubjectRepo = classSubjectRepo;
        }

        // 1. Xem danh sách lớp
        public IActionResult List()
        {
            var classes = _classRepo.GetAll();
            var classSubjects = _classSubjectRepo.GetAll();
            var subjects = _subjectRepo.GetAll();
            var teachers = _teacherRepo.GetAll();

            var viewModel = classes.Select(c => new ClassDetailViewModel
            {
                Class = c,
                SubjectTeachers = (from cs in classSubjects
                                   where cs.ClassId == c.ClassId
                                   join s in subjects on cs.SubjectId equals s.SubjectId into subjectGroup
                                   from sub in subjectGroup.DefaultIfEmpty()
                                   join t in teachers on cs.TeacherId equals t.TeacherId into teacherGroup
                                   from teach in teacherGroup.DefaultIfEmpty()
                                   select new ClassSubjectViewModel
                                   {
                                       SubjectId = cs.SubjectId,
                                       SubjectName = sub != null ? sub.SubjectName : $"Unknown ({cs.SubjectId})",
                                       TeacherId = cs.TeacherId,
                                       TeacherName = teach != null ? teach.Name : $"Unknown ({cs.TeacherId})"
                                   }).ToList()
            }).ToList();

            return View(viewModel);
        }

        // 2. Tạo lớp mới (Giao diện)
        [HttpGet]
        public IActionResult Add()
        {
            ViewBag.AllSubjects = _subjectRepo.GetAll();
            LoadAllTeachersWithSubjects();
            return View(new ClassDetailViewModel { Class = new ClassModel() });
        }

        // 2. Tạo lớp mới (Xử lý)
        [HttpPost]
        public IActionResult Add(ClassDetailViewModel viewModel, List<string> SubjectIds, List<string> TeacherIds)
        {
            var model = viewModel.Class;

            // Validate Class ID format
            if (string.IsNullOrWhiteSpace(model.ClassId) || !ValidationHelper.IsValidId(model.ClassId))
            {
                ModelState.AddModelError("ClassId", "Class ID must be 3-20 alphanumeric characters!");
                ViewBag.AllSubjects = _subjectRepo.GetAll();
                LoadAllTeachersWithSubjects();
                return View(new ClassDetailViewModel { Class = model });
            }

            // Check for duplicate Class ID
            if (!string.IsNullOrWhiteSpace(model.ClassId) && _classRepo.GetById(model.ClassId) != null)
            {
                ModelState.AddModelError("ClassId", "Class ID already exists in the system!");
                ViewBag.AllSubjects = _subjectRepo.GetAll();
                LoadAllTeachersWithSubjects();
                return View(new ClassDetailViewModel { Class = model });
            }

            // Validate required fields
            if (string.IsNullOrEmpty(model.ClassName))
                ModelState.AddModelError("ClassName", "Class Name is required.");
            if (string.IsNullOrEmpty(model.Semester))
                ModelState.AddModelError("Semester", "Semester is required.");
            if (model.NumberOfStudents <= 0)
                ModelState.AddModelError("NumberOfStudents", "Number of students must be at least 1!");

            // Validate at least one subject-teacher pair
            if (SubjectIds == null || !SubjectIds.Any() || TeacherIds == null || !TeacherIds.Any())
            {
                ModelState.AddModelError("", "Please add at least one Subject-Teacher pair!");
                ViewBag.AllSubjects = _subjectRepo.GetAll();
                LoadAllTeachersWithSubjects();
                return View(new ClassDetailViewModel { Class = model });
            }

            if (ModelState.IsValid)
            {
                // 1. Tạo Class
                _classRepo.Add(model);

                // 2. Thêm các ClassSubject records
                for (int i = 0; i < SubjectIds.Count && i < TeacherIds.Count; i++)
                {
                    var classSubject = new ClassSubjectModel
                    {
                        ClassId = model.ClassId,
                        SubjectId = SubjectIds[i],
                        TeacherId = TeacherIds[i]
                    };
                    _classSubjectRepo.Add(classSubject);
                }

                return RedirectToAction("List");
            }

            ViewBag.AllSubjects = _subjectRepo.GetAll();
            LoadAllTeachersWithSubjects();
            return View(new ClassDetailViewModel { Class = model });
        }

        // 3. Sửa lớp (Giao diện)
        [HttpGet]
        public IActionResult Edit(string id)
        {
            var classModel = _classRepo.GetById(id);
            if (classModel == null) return NotFound();

            var classSubjects = _classSubjectRepo.GetByClassId(id);
            var subjects = _subjectRepo.GetAll();
            var teachers = _teacherRepo.GetAll();

            var viewModel = new ClassDetailViewModel
            {
                Class = classModel,
                SubjectTeachers = (from cs in classSubjects
                                   join s in subjects on cs.SubjectId equals s.SubjectId into subjectGroup
                                   from sub in subjectGroup.DefaultIfEmpty()
                                   join t in teachers on cs.TeacherId equals t.TeacherId into teacherGroup
                                   from teach in teacherGroup.DefaultIfEmpty()
                                   select new ClassSubjectViewModel
                                   {
                                       SubjectId = cs.SubjectId,
                                       SubjectName = sub != null ? sub.SubjectName : cs.SubjectId,
                                       TeacherId = cs.TeacherId,
                                       TeacherName = teach != null ? teach.Name : cs.TeacherId
                                   }).ToList()
            };

            ViewBag.AllSubjects = _subjectRepo.GetAll();
            LoadAllTeachersWithSubjects();
            return View(viewModel);
        }

        // 3. Sửa lớp (Xử lý)
        [HttpPost]
        public IActionResult Edit(ClassDetailViewModel viewModel, List<string> SubjectIds, List<string> TeacherIds)
        {
            var model = viewModel.Class;

            if (string.IsNullOrEmpty(model.ClassName))
                ModelState.AddModelError("ClassName", "Class Name is required.");
            if (string.IsNullOrEmpty(model.Semester))
                ModelState.AddModelError("Semester", "Semester is required.");
            if (model.NumberOfStudents <= 0)
                ModelState.AddModelError("NumberOfStudents", "Number of students must be at least 1!");

            if (SubjectIds == null || !SubjectIds.Any() || TeacherIds == null || !TeacherIds.Any())
            {
                ModelState.AddModelError("", "Please add at least one Subject-Teacher pair!");
            }

            if (ModelState.IsValid)
            {
                // 1. Update Class info
                _classRepo.Update(model);

                // 2. Delete old ClassSubject records
                _classSubjectRepo.DeleteByClassId(model.ClassId!);

                // 3. Add new ClassSubject records
                for (int i = 0; i < SubjectIds.Count && i < TeacherIds.Count; i++)
                {
                    var classSubject = new ClassSubjectModel
                    {
                        ClassId = model.ClassId,
                        SubjectId = SubjectIds[i],
                        TeacherId = TeacherIds[i]
                    };
                    _classSubjectRepo.Add(classSubject);
                }

                return RedirectToAction("List");
            }

            var viewModel2 = new ClassDetailViewModel { Class = model };
            ViewBag.AllSubjects = _subjectRepo.GetAll();
            LoadAllTeachersWithSubjects();
            return View(viewModel2);
        }

        // 4. Xóa lớp
        public IActionResult Delete(string id)
        {
            // Xóa ClassSubject records trước
            _classSubjectRepo.DeleteByClassId(id);
            // Xóa Class
            _classRepo.Delete(id);
            return RedirectToAction("List");
        }

        // 5. Trang quản lý sinh viên của lớp (GET)
        [HttpGet]
        public IActionResult ManageStudents(string id)
        {
            var classInfo = _classRepo.GetById(id);
            if (classInfo == null) return NotFound();

            // Lấy danh sách ID sinh viên đã ở trong lớp
            var enrolledRelations = _studentClassRepo.GetByClassId(id);
            var enrolledStudentIds = enrolledRelations.Select(x => x.StudentId).ToList();

            // Lấy tất cả sinh viên
            var allStudents = _studentRepo.GetAll();

            var viewModel = new ClassEnrollmentViewModel
            {
                ClassInfo = classInfo,
                // Lọc ra danh sách sinh viên đã có trong lớp
                EnrolledStudents = allStudents.Where(s => enrolledStudentIds.Contains(s.StudentId)).ToList(),
                // Lọc ra danh sách sinh viên CHƯA có trong lớp (để hiển thị checkbox chọn)
                AvailableStudents = allStudents.Where(s => !enrolledStudentIds.Contains(s.StudentId)).ToList()
            };

            return View(viewModel);
        }

        // 6. Xử lý thêm sinh viên vào lớp (POST)
        [HttpPost]
        public IActionResult AddStudentsToClass(string classId, List<string> selectedStudents)
        {
            if (selectedStudents != null && selectedStudents.Any())
            {
                foreach (var studentId in selectedStudents)
                {
                    if (!_studentClassRepo.IsEnrolled(classId, studentId))
                    {
                        var newEnrollment = new StudentClassModel
                        {
                            ClassId = classId,
                            StudentId = studentId,
                            JoinedDate = DateTime.Now
                        };
                        _studentClassRepo.Add(newEnrollment);
                    }
                }

                // Cập nhật lại sỉ số lớp
                UpdateClassCount(classId);
            }

            return RedirectToAction("ManageStudents", new { id = classId });
        }

        // 7. Xóa 1 sinh viên khỏi lớp
        public IActionResult RemoveStudentFromClass(string classId, string studentId)
        {
            _studentClassRepo.Remove(classId, studentId);
            UpdateClassCount(classId);
            return RedirectToAction("ManageStudents", new { id = classId });
        }

        // Hàm phụ: Cập nhật lại cột NumberOfStudents trong file classes.csv
        private void UpdateClassCount(string classId)
        {
            var currentClass = _classRepo.GetById(classId);
            if (currentClass != null)
            {
                var count = _studentClassRepo.GetByClassId(classId).Count;
                currentClass.NumberOfStudents = count;
                _classRepo.Update(currentClass);
            }
        }

        private void LoadAllTeachersWithSubjects()
        {
            var allSubjects = _subjectRepo.GetAll();
            var allTeachers = _teacherRepo.GetAll();

            var teachersWithSubjects = allTeachers.Select(t => new
            {
                Id = t.TeacherId,
                Name = $"{t.TeacherId} - {t.Name}",
                SubjectIds = allSubjects
                    .Where(s => !string.IsNullOrEmpty(s.TeacherIds) &&
                                s.TeacherIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(id => id.Trim())
                                .Contains(t.TeacherId))
                    .Select(s => s.SubjectId)
                    .ToList()
            }).ToList();

            ViewBag.AllTeachers = teachersWithSubjects;
        }
    }
}