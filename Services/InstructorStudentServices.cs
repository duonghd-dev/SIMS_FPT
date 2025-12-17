using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Models;
using SIMS_FPT.Models.ViewModels;
using SIMS_FPT.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SIMS_FPT.Services
{
    public class InstructorCourseMaterialService : IInstructorCourseMaterialService
    {
        private readonly ICourseMaterialRepository _materialRepo;
        private readonly ISubjectRepository _subjectRepo;
        private readonly IClassRepository _classRepo;
        private readonly IClassSubjectRepository _classSubjectRepo;
        private readonly IWebHostEnvironment _env;

        public InstructorCourseMaterialService(
            ICourseMaterialRepository materialRepo,
            ISubjectRepository subjectRepo,
            IClassRepository classRepo,
            IClassSubjectRepository classSubjectRepo,
            IWebHostEnvironment env)
        {
            _materialRepo = materialRepo;
            _subjectRepo = subjectRepo;
            _classRepo = classRepo;
            _classSubjectRepo = classSubjectRepo;
            _env = env;
        }

        public List<string> GetTeacherSubjectIds(string teacherId)
        {
            return _classSubjectRepo.GetAll()
                .Where(cs => cs.TeacherId == teacherId)
                .Select(cs => cs.SubjectId)
                .Distinct()
                .ToList();
        }

        public List<CourseMaterialModel> GetTeacherMaterials(string teacherId)
        {
            var allowedSubjectIds = GetTeacherSubjectIds(teacherId);
            return _materialRepo.GetAll()
                .Where(m => allowedSubjectIds.Contains(m.SubjectId))
                .OrderByDescending(m => m.UploadDate)
                .ToList();
        }

        public List<(string SubjectId, string DisplayText)> GetTeacherClassSubjectList(string teacherId)
        {
            var teacherClassIds = _classSubjectRepo.GetAll()
                .Where(cs => cs.TeacherId == teacherId)
                .Select(cs => cs.ClassId)
                .Distinct()
                .ToList();

            var teacherClasses = _classRepo.GetAll()
                .Where(c => teacherClassIds.Contains(c.ClassId))
                .ToList();

            var result = new List<(string SubjectId, string DisplayText)>();

            foreach (var c in teacherClasses)
            {
                var classSubjects = _classSubjectRepo.GetByClassId(c.ClassId!);
                foreach (var cs in classSubjects)
                {
                    var subject = _subjectRepo.GetById(cs.SubjectId!);
                    if (subject != null)
                    {
                        result.Add((cs.SubjectId!, $"{c.ClassName ?? c.ClassId ?? "Unknown"} - {subject.SubjectName ?? subject.SubjectId ?? "Unknown"}"));
                    }
                }
            }

            return result;
        }

        public (bool Success, string Message, string? FilePath) CreateMaterial(CourseMaterialModel model, IFormFile? uploadFile, string teacherId)
        {
            try
            {
                var allowedSubjectIds = GetTeacherSubjectIds(teacherId);
                if (!allowedSubjectIds.Contains(model.SubjectId))
                    return (false, "You are not authorized to add materials for this subject.", null);

                if (uploadFile == null && string.IsNullOrWhiteSpace(model.VideoUrl))
                    return (false, "Please upload a file or provide a video link.", null);

                if (uploadFile != null)
                {
                    var ext = Path.GetExtension(uploadFile.FileName).ToLowerInvariant();
                    var allowed = new[] { ".pdf", ".ppt", ".pptx" };
                    if (!allowed.Contains(ext))
                        return (false, "Only PDF, PPT, or PPTX files are allowed.", null);

                    var dir = Path.Combine(_env.WebRootPath, "materials", model.SubjectId ?? "general");
                    if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(uploadFile.FileName)}";
                    var filePath = Path.Combine(dir, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        uploadFile.CopyTo(stream);
                    }

                    model.FilePath = Path.Combine("materials", model.SubjectId ?? "general", fileName).Replace("\\", "/");
                }

                model.UploadDate = DateTime.Now;
                if (string.IsNullOrEmpty(model.MaterialId)) model.MaterialId = Guid.NewGuid().ToString();

                _materialRepo.Add(model);
                return (true, "Material saved successfully.", model.FilePath);
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}", null);
            }
        }

        public (bool Success, string Message) DeleteMaterial(string materialId, string teacherId)
        {
            try
            {
                var mat = _materialRepo.GetById(materialId);
                if (mat == null)
                    return (false, "Material not found");

                var allowedSubjectIds = GetTeacherSubjectIds(teacherId);
                if (!allowedSubjectIds.Contains(mat.SubjectId))
                    return (false, "You are not authorized to delete this material.");

                if (!string.IsNullOrEmpty(mat.FilePath))
                {
                    var phys = Path.Combine(_env.WebRootPath, mat.FilePath.TrimStart('/', '\\'));
                    if (System.IO.File.Exists(phys))
                    {
                        System.IO.File.Delete(phys);
                    }
                }

                _materialRepo.Delete(materialId);
                return (true, "Material deleted successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }
    }

    public class InstructorStudentService : IInstructorStudentService
    {
        private readonly IStudentRepository _studentRepo;
        private readonly IAssignmentRepository _assignmentRepo;
        private readonly ISubmissionRepository _submissionRepo;

        public InstructorStudentService(
            IStudentRepository studentRepo,
            IAssignmentRepository assignmentRepo,
            ISubmissionRepository submissionRepo)
        {
            _studentRepo = studentRepo;
            _assignmentRepo = assignmentRepo;
            _submissionRepo = submissionRepo;
        }

        public StudentProfileViewModel? GetStudentProfile(string studentId, string teacherId)
        {
            var student = _studentRepo.GetById(studentId);
            if (student == null) return null;

            var teacherAssignments = _assignmentRepo.GetAll().Where(a => a.TeacherId == teacherId).ToList();
            if (!teacherAssignments.Any()) return null;

            var history = teacherAssignments.Select(a =>
            {
                var sub = _submissionRepo.GetByStudentAndAssignment(studentId, a.AssignmentId ?? "");
                return new AssignmentHistoryItem
                {
                    AssignmentId = a.AssignmentId ?? "",
                    AssignmentTitle = a.Title ?? "Untitled",
                    SubjectId = a.SubjectId ?? "",
                    Grade = sub?.Grade ?? null,
                    MaxPoints = a.MaxPoints,
                    TeacherComments = sub?.TeacherComments ?? ""
                };
            }).ToList();

            double totalScore = history.Where(h => h.Grade.HasValue).Sum(h => h.Grade!.Value);
            double totalMax = history.Sum(h => h.MaxPoints);
            double avgPercent = totalMax > 0 ? (totalScore / totalMax) * 100 : 0;

            return new StudentProfileViewModel
            {
                Student = student,
                AssignmentHistory = history,
                AverageScorePercent = avgPercent
            };
        }
    }

    public class StudentAssignmentService : IStudentAssignmentService
    {
        private readonly IAssignmentRepository _assignmentRepo;
        private readonly ISubmissionRepository _submissionRepo;
        private readonly IClassRepository _classRepo;
        private readonly IStudentClassRepository _studentClassRepo;
        private readonly IWebHostEnvironment _env;

        public StudentAssignmentService(
            IAssignmentRepository assignmentRepo,
            ISubmissionRepository submissionRepo,
            IClassRepository classRepo,
            IStudentClassRepository studentClassRepo,
            IWebHostEnvironment env)
        {
            _assignmentRepo = assignmentRepo;
            _submissionRepo = submissionRepo;
            _classRepo = classRepo;
            _studentClassRepo = studentClassRepo;
            _env = env;
        }

        public bool IsStudentEligible(string studentId, string assignmentId)
        {
            var assignment = _assignmentRepo.GetById(assignmentId);
            if (assignment == null || string.IsNullOrEmpty(studentId)) return false;

            if (!string.IsNullOrEmpty(assignment.ClassId))
            {
                return _studentClassRepo.IsEnrolled(assignment.ClassId, studentId);
            }

            var enrollments = _studentClassRepo.GetByStudentId(studentId);
            var enrolledClassIds = enrollments.Select(e => e.ClassId).ToList();
            return enrolledClassIds.Contains(assignment.ClassId);
        }

        public List<StudentAssignmentViewModel> GetStudentAssignments(string studentId)
        {
            var allAssignments = _assignmentRepo.GetAll();
            var visibleAssignments = allAssignments
                .Where(a => IsStudentEligible(studentId, a.AssignmentId ?? ""))
                .ToList();

            return visibleAssignments
                .Select(a =>
                {
                    var classInfo = _classRepo.GetById(a.ClassId);
                    var submission = _submissionRepo.GetByStudentAndAssignment(studentId, a.AssignmentId);

                    if (submission != null && !a.AreGradesPublished)
                    {
                        submission.Grade = null;
                        submission.TeacherComments = null;
                    }

                    return new StudentAssignmentViewModel
                    {
                        Assignment = a,
                        Submission = submission,
                        ClassName = classInfo?.ClassName ?? "Unknown Class"
                    };
                })
                .OrderByDescending(x => x.Assignment.DueDate)
                .ToList();
        }

        public StudentAssignmentViewModel? GetAssignmentForSubmission(string assignmentId, string studentId)
        {
            var assignment = _assignmentRepo.GetById(assignmentId);
            if (assignment == null || !IsStudentEligible(studentId, assignmentId))
                return null;

            var submission = _submissionRepo.GetByStudentAndAssignment(studentId, assignmentId);

            return new StudentAssignmentViewModel
            {
                Assignment = assignment,
                Submission = submission
            };
        }

        public (bool Success, string Message, string? FilePath) SubmitAssignment(string assignmentId, string studentId, IFormFile uploadFile)
        {
            try
            {
                var assignment = _assignmentRepo.GetById(assignmentId);
                if (assignment == null)
                    return (false, "Assignment not found", null);

                if (!IsStudentEligible(studentId, assignmentId))
                    return (false, "Access denied", null);

                if (uploadFile == null || uploadFile.Length == 0)
                    return (false, "Please select a file to upload.", null);

                var ext = Path.GetExtension(uploadFile.FileName)?.ToLowerInvariant();
                var allowed = assignment.AllowedFileTypes;
                if (!string.IsNullOrWhiteSpace(allowed) && allowed.Trim() != "*")
                {
                    var allowedList = allowed
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Trim().ToLowerInvariant())
                        .ToList();

                    var safeExt = ext ?? string.Empty;
                    if (string.IsNullOrEmpty(safeExt) || !allowedList.Contains(safeExt))
                        return (false, $"File type {safeExt} is not allowed for this assignment.", null);
                }

                var classSegment = assignment.ClassId ?? "unknown-class";
                var uploadsFolder = Path.Combine(_env.WebRootPath, "submissions", classSegment, assignment.AssignmentId);
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                var extension = Path.GetExtension(uploadFile.FileName);
                var fileName = $"{studentId}_{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                var existing = _submissionRepo.GetByStudentAndAssignment(studentId, assignmentId);
                if (existing != null && !string.IsNullOrEmpty(existing.FilePath))
                {
                    var existingPhysical = Path.Combine(_env.WebRootPath, existing.FilePath.TrimStart('/', '\\'));
                    if (System.IO.File.Exists(existingPhysical))
                    {
                        System.IO.File.Delete(existingPhysical);
                    }
                }

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    uploadFile.CopyTo(stream);
                }

                var submission = existing ?? new SubmissionModel { AssignmentId = assignmentId, StudentId = studentId };
                submission.FilePath = Path.Combine("submissions", classSegment, assignment.AssignmentId, fileName).Replace("\\", "/");
                submission.SubmissionDate = DateTime.Now;

                if (existing != null)
                {
                    submission.Grade = null;
                    submission.TeacherComments = null;
                }

                _submissionRepo.SaveSubmission(submission);
                return (true, "Submission uploaded successfully.", submission.FilePath);
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}", null);
            }
        }
    }
}
