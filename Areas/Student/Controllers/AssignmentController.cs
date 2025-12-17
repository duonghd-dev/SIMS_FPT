using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // Cần thêm để dùng SelectList
using SIMS_FPT.Services.Interfaces;
using SIMS_FPT.Data.Interfaces; // Cần thêm để dùng các Repository
using System.Security.Claims;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Hosting; // Cần thêm namespace này
using SIMS_FPT.Models;

namespace SIMS_FPT.Areas.Student.Controllers
{
    [Area("Student")]
    public class AssignmentController : Controller
    {
        private readonly IStudentAssignmentService _assignmentService;
        private readonly IWebHostEnvironment _webHostEnvironment; // Thêm biến môi trường

        // Khai báo thêm các Repository cần thiết
        private readonly IStudentClassRepository _studentClassRepo;
        private readonly IClassRepository _classRepo;
        private readonly ISubjectRepository _subjectRepo;
        private readonly IClassSubjectRepository _classSubjectRepo;

        // Cập nhật Constructor để Inject các Repository
        public AssignmentController(
            IStudentAssignmentService assignmentService,
            IStudentClassRepository studentClassRepo,
            IClassRepository classRepo,
            ISubjectRepository subjectRepo,
            IClassSubjectRepository classSubjectRepo,
            IWebHostEnvironment webHostEnvironment)
        {
            _assignmentService = assignmentService;
            _studentClassRepo = studentClassRepo;
            _classRepo = classRepo;
            _subjectRepo = subjectRepo;
            _classSubjectRepo = classSubjectRepo;
            _webHostEnvironment = webHostEnvironment;
        }

        private string CurrentStudentId
        {
            get
            {
                var linkedId = User.FindFirst("LinkedId")?.Value;
                if (!string.IsNullOrEmpty(linkedId)) return linkedId;
                return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity?.Name ?? string.Empty;
            }
        }

        // Cập nhật Action Index để nhận tham số lọc
        public IActionResult Index(string subjectId, string classId)
        {
            // 1. Lấy danh sách lớp mà sinh viên đang học
            var studentClasses = _studentClassRepo.GetByStudentId(CurrentStudentId);
            var studentClassIds = studentClasses.Select(sc => sc.ClassId).ToList();

            // 2. Lấy thông tin các môn học được dạy trong các lớp đó
            // Tìm tất cả các liên kết Class-Subject cho các lớp của sinh viên
            var relevantClassSubjects = new List<ClassSubjectModel>();
            foreach (var cid in studentClassIds)
            {
                //
                relevantClassSubjects.AddRange(_classSubjectRepo.GetByClassId(cid));
            }

            // --- XỬ LÝ DROPDOWN SUBJECT ---
            // Lọc ra danh sách Subject ID duy nhất từ các lớp sinh viên học
            var uniqueSubjectIds = relevantClassSubjects.Select(cs => cs.SubjectId).Distinct().ToList();
            var subjects = new List<SubjectModel>();
            foreach (var sid in uniqueSubjectIds)
            {
                var subj = _subjectRepo.GetById(sid);
                if (subj != null) subjects.Add(subj);
            }

            // Đổ dữ liệu vào ViewBag cho Dropdown Subject
            ViewBag.Subjects = new SelectList(subjects, "SubjectId", "SubjectName", subjectId);


            // --- XỬ LÝ DROPDOWN CLASS ---
            List<string> filteredClassIds = studentClassIds;

            // Nếu đã chọn Subject, chỉ lấy các lớp (trong số lớp sv học) có dạy môn này
            if (!string.IsNullOrEmpty(subjectId))
            {
                filteredClassIds = relevantClassSubjects
                    .Where(cs => cs.SubjectId == subjectId)
                    .Select(cs => cs.ClassId)
                    .Distinct()
                    .Intersect(studentClassIds) // Đảm bảo vẫn là lớp sinh viên đang học
                    .ToList();
            }

            // Lấy thông tin chi tiết Class để hiển thị tên lớp
            var classesToDisplay = new List<ClassModel>();
            foreach (var cid in filteredClassIds)
            {
                var cls = _classRepo.GetById(cid);
                if (cls != null) classesToDisplay.Add(cls);
            }

            // Đổ dữ liệu vào ViewBag cho Dropdown Class
            ViewBag.Classes = new SelectList(classesToDisplay, "ClassId", "ClassName", classId);


            // --- LẤY VÀ LỌC DANH SÁCH BÀI TẬP ---
            var viewModel = _assignmentService.GetStudentAssignments(CurrentStudentId);

            // 3. Lọc theo Subject nếu có chọn
            if (!string.IsNullOrEmpty(subjectId))
            {
                // Lưu ý: Cần đảm bảo AssignmentModel có SubjectId chính xác
                viewModel = viewModel.Where(x => x.Assignment.SubjectId == subjectId).ToList();
            }

            // 4. Lọc theo Class nếu có chọn
            if (!string.IsNullOrEmpty(classId))
            {
                viewModel = viewModel.Where(x => x.Assignment.ClassId == classId).ToList();
            }

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Submit(string id)
        {
            var vm = _assignmentService.GetAssignmentForSubmission(id, CurrentStudentId);
            if (vm == null)
                return RedirectToAction("AccessDenied", "Login", new { area = "" });

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Submit(string id, IFormFile uploadFile)
        {
            // 1. Lấy thông tin bài tập trước để kiểm tra hạn nộp
            var vm = _assignmentService.GetAssignmentForSubmission(id, CurrentStudentId);
            if (vm == null) return RedirectToAction("Index"); // Nếu không tìm thấy bài

            // 2. CHECK LOGIC: Kiểm tra thời gian (Server Side Validation)
            if (DateTime.Now > vm.Assignment.DueDate)
            {
                // Nếu quá hạn -> Thêm lỗi và trả về View ngay lập tức
                ModelState.AddModelError(string.Empty, "The submission deadline has passed. You can no longer submit this assignment.");
                return View(vm);
            }

            // 3. Logic cũ: Kiểm tra file rỗng
            if (uploadFile == null || uploadFile.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "Please select a file to upload.");
                return View(vm);
            }

            // 4. Xử lý lưu file (như cũ)
            var (success, message, filePath) = _assignmentService.SubmitAssignment(id, CurrentStudentId, uploadFile);
            if (success)
            {
                TempData["Success"] = message;
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError(string.Empty, message);
                return View(vm);
            }
        }

        // 1. ACTION DOWNLOAD FILE (Logic giống giáo viên)
        [HttpGet]
        public IActionResult Download(string id)
        {
            // Lấy thông tin bài tập/bài nộp của sinh viên
            var vm = _assignmentService.GetAssignmentForSubmission(id, CurrentStudentId);

            if (vm == null || !vm.HasSubmission || string.IsNullOrEmpty(vm.Submission.FilePath))
            {
                TempData["Error"] = "File not found.";
                return RedirectToAction("Submit", new { id = id });
            }

            // Xử lý đường dẫn file
            // DB lưu dạng: "/submissions/SE001/file.pdf" -> Cần map sang đường dẫn ổ cứng
            string relativePath = vm.Submission.FilePath.TrimStart('/');
            string physicalPath = Path.Combine(_webHostEnvironment.WebRootPath, relativePath);

            if (!System.IO.File.Exists(physicalPath))
            {
                TempData["Error"] = "File submission is missing on server.";
                return RedirectToAction("Submit", new { id = id });
            }

            // Trả về file cho trình duyệt tải xuống
            byte[] fileBytes = System.IO.File.ReadAllBytes(physicalPath);
            string fileName = Path.GetFileName(physicalPath);
            
            // Dùng application/octet-stream để buộc tải xuống thay vì mở trên tab mới
            return File(fileBytes, "application/octet-stream", fileName);
        }

        // 2. ACTION DELETE SUBMISSION
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteSubmission(string id)
        {
            var studentId = CurrentStudentId;
            var vm = _assignmentService.GetAssignmentForSubmission(id, studentId);

            if (vm == null || !vm.HasSubmission)
            {
                return RedirectToAction("Index");
            }

            // --- QUAN TRỌNG: KIỂM TRA ĐIỂM SỐ ---
            // Nếu đã có điểm (HasGrade = true) -> KHÔNG CHO XÓA
            if (vm.HasGrade) 
            {
                TempData["Error"] = "You cannot delete this submission because it has already been graded by the teacher.";
                return RedirectToAction("Submit", new { id = id });
            }

            // --- LOGIC XÓA FILE TRÊN Ổ CỨNG ---
            try 
            {
                if (!string.IsNullOrEmpty(vm.Submission.FilePath))
                {
                    string relativePath = vm.Submission.FilePath.TrimStart('/');
                    string physicalPath = Path.Combine(_webHostEnvironment.WebRootPath, relativePath);
                    if (System.IO.File.Exists(physicalPath))
                    {
                        System.IO.File.Delete(physicalPath);
                    }
                }
            }
            catch (Exception ex)
            {
                // Ghi log lỗi nếu cần, nhưng vẫn tiếp tục xóa trong DB để dữ liệu nhất quán
            }

            // --- GỌI SERVICE ĐỂ XÓA TRONG DATABASE ---
            // Bạn cần đảm bảo Service có hàm DeleteSubmission. 
            // Nếu chưa có, xem phần bổ sung Service bên dưới.
            bool deleteSuccess = _assignmentService.DeleteSubmission(id, studentId);

            if (deleteSuccess)
            {
                TempData["Success"] = "Submission deleted successfully.";
            }
            else
            {
                TempData["Error"] = "Could not delete submission record.";
            }

            return RedirectToAction("Submit", new { id = id });
        }
    }
}