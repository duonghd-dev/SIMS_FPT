using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SIMS_FPT.Data.Interfaces;
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
        // Mới thêm
        private readonly IStudentRepository _studentRepo;
        private readonly IStudentClassRepository _studentClassRepo;

        public ClassController(
            IClassRepository classRepo,
            ISubjectRepository subjectRepo,
            ITeacherRepository teacherRepo,
            IStudentRepository studentRepo,
            IStudentClassRepository studentClassRepo)
        {
            _classRepo = classRepo;
            _subjectRepo = subjectRepo;
            _teacherRepo = teacherRepo;
            _studentRepo = studentRepo;
            _studentClassRepo = studentClassRepo;
        }

        // 1. Xem danh sách lớp
        public IActionResult List()
        {
            // 1. Lấy tất cả dữ liệu
            var classes = _classRepo.GetAll();
            var subjects = _subjectRepo.GetAll();
            var teachers = _teacherRepo.GetAll();

            // 2. Kết hợp dữ liệu (Mapping) sử dụng LINQ
            // Dùng Left Join để nếu ID không tồn tại thì không bị lỗi chết trang
            var listViewModel = from c in classes
                                join s in subjects on c.SubjectName equals s.SubjectId into subjectGroup
                                from sub in subjectGroup.DefaultIfEmpty() // Nếu không tìm thấy môn, sub sẽ null

                                join t in teachers on c.TeacherName equals t.TeacherId into teacherGroup
                                from teach in teacherGroup.DefaultIfEmpty() // Nếu không tìm thấy GV, teach sẽ null

                                select new ClassModel
                                {
                                    ClassId = c.ClassId,
                                    ClassName = c.ClassName,
                                    Semester = c.Semester,
                                    NumberOfStudents = c.NumberOfStudents,

                                    // Nếu tìm thấy tên thì lấy, không thì báo lỗi hoặc hiện lại ID
                                    SubjectName = sub != null ? sub.SubjectName : $"Unknown ({c.SubjectName})",
                                    TeacherName = teach != null ? teach.Name : $"Unknown ({c.TeacherName})"
                                };

            // 3. Trả về danh sách ViewModel
            return View(listViewModel.ToList());
        }

        // 2. Tạo lớp mới (Giao diện)
        [HttpGet]
        public IActionResult Add()
        {
            // Lấy danh sách Subject và Teacher để hiển thị Dropdown
            // Value là ID, Text là Name (hoặc ID - Name cho dễ nhìn)
            ViewBag.Subjects = new SelectList(_subjectRepo.GetAll(), "SubjectId", "SubjectName");

            // Giả sử Teacher model có Id và FullName. Nếu TeacherRepository trả về Users, hãy dùng Users.
            // Ở đây tôi dùng _teacherRepo.GetAll() trả về danh sách giáo viên.
            // Cần đảm bảo TeacherModel/Users có trường Id và FullName tương ứng.
            var teachers = _teacherRepo.GetAll().Select(t => new
            {
                Id = t.TeacherId,
                Name = $"{t.TeacherId} - {t.Name}"
            });
            ViewBag.Teachers = new SelectList(teachers, "Id", "Name");

            return View();
        }

        // 2. Tạo lớp mới (Xử lý)
        [HttpPost]
        public IActionResult Add(ClassModel model)
        {
            // Kiểm tra trùng ID lớp
            if (_classRepo.GetById(model.ClassId) != null)
            {
                ModelState.AddModelError("ClassId", "Class ID already exists!");
                // Load lại dropdown nếu lỗi
                ViewBag.Subjects = new SelectList(_subjectRepo.GetAll(), "SubjectId", "SubjectName");
                var teachers = _teacherRepo.GetAll().Select(t => new { Id = t.TeacherId, Name = $"{t.TeacherId} - {t.Name}" });
                ViewBag.Teachers = new SelectList(teachers, "Id", "Name");
                return View(model);
            }

            if (ModelState.IsValid)
            {
                _classRepo.Add(model);
                return RedirectToAction("List");
            }

            // Load lại dropdown nếu model không hợp lệ
            ViewBag.Subjects = new SelectList(_subjectRepo.GetAll(), "SubjectId", "SubjectName");
            var teachersReload = _teacherRepo.GetAll().Select(t => new { Id = t.TeacherId, Name = $"{t.TeacherId} - {t.Name}" });
            ViewBag.Teachers = new SelectList(teachersReload, "Id", "Name");

            return View(model);
        }

        // 3. Sửa lớp (Giao diện)
        [HttpGet]
        public IActionResult Edit(string id)
        {
            var item = _classRepo.GetById(id);
            if (item == null) return NotFound();

            ViewBag.Subjects = new SelectList(_subjectRepo.GetAll(), "SubjectId", "SubjectName", item.SubjectName);

            var teachers = _teacherRepo.GetAll().Select(t => new { Id = t.TeacherId, Name = $"{t.TeacherId} - {t.Name}" });
            ViewBag.Teachers = new SelectList(teachers, "Id", "Name", item.TeacherName);

            return View(item);
        }

        // 3. Sửa lớp (Xử lý)
        [HttpPost]
        public IActionResult Edit(ClassModel model)
        {
            if (ModelState.IsValid)
            {
                _classRepo.Update(model);
                return RedirectToAction("List");
            }

            ViewBag.Subjects = new SelectList(_subjectRepo.GetAll(), "SubjectId", "SubjectName", model.SubjectName);
            var teachers = _teacherRepo.GetAll().Select(t => new { Id = t.TeacherId, Name = $"{t.TeacherId} - {t.Name}" });
            ViewBag.Teachers = new SelectList(teachers, "Id", "Name", model.TeacherName);

            return View(model);
        }

        // 4. Xóa lớp
        public IActionResult Delete(string id)
        {
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
                
                // Cập nhật lại sỉ số lớp (Optional)
                UpdateClassCount(classId);
            }

            return RedirectToAction("ManageStudents", new { id = classId });
        }

        // 7. Xóa 1 sinh viên khỏi lớp
        public IActionResult RemoveStudentFromClass(string classId, string studentId)
        {
            _studentClassRepo.Remove(classId, studentId);
            // Cập nhật lại sỉ số lớp (Optional)
            UpdateClassCount(classId);

            return RedirectToAction("ManageStudents", new { id = classId });
        }

        // Hàm phụ: Cập nhật lại cột NumberOfStudents trong file classes.csv
        private void UpdateClassCount(string classId)
        {
            var currentClass = _classRepo.GetById(classId);
            if(currentClass != null)
            {
                var count = _studentClassRepo.GetByClassId(classId).Count;
                currentClass.NumberOfStudents = count;
                _classRepo.Update(currentClass);
            }
        }
    }
}