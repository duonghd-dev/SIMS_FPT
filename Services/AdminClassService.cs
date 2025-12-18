using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Models;
using SIMS_FPT.Models.ViewModels;
using SIMS_FPT.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SIMS_FPT.Services
{
    public class AdminClassService : IAdminClassService
    {
        private readonly IClassRepository _classRepo;
        private readonly IClassSubjectRepository _classSubjectRepo;
        private readonly ISubjectRepository _subjectRepo;
        private readonly ITeacherRepository _teacherRepo;
        private readonly IStudentRepository _studentRepo;
        private readonly IStudentClassRepository _studentClassRepo;

        public AdminClassService(
            IClassRepository classRepo,
            IClassSubjectRepository classSubjectRepo,
            ISubjectRepository subjectRepo,
            ITeacherRepository teacherRepo,
            IStudentRepository studentRepo,
            IStudentClassRepository studentClassRepo)
        {
            _classRepo = classRepo;
            _classSubjectRepo = classSubjectRepo;
            _subjectRepo = subjectRepo;
            _teacherRepo = teacherRepo;
            _studentRepo = studentRepo;
            _studentClassRepo = studentClassRepo;
        }

        public List<ClassDetailViewModel> GetAllClassesWithDetails()
        {
            var classes = _classRepo.GetAll();
            var classSubjects = _classSubjectRepo.GetAll();
            var subjects = _subjectRepo.GetAll();
            var teachers = _teacherRepo.GetAll();

            return classes.Select(c => new ClassDetailViewModel
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
        }

        public ClassDetailViewModel? GetClassWithDetails(string classId)
        {
            var classModel = _classRepo.GetById(classId);
            if (classModel == null) return null;

            var classSubjects = _classSubjectRepo.GetByClassId(classId);
            var subjects = _subjectRepo.GetAll();
            var teachers = _teacherRepo.GetAll();

            return new ClassDetailViewModel
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
        }

        public ClassEnrollmentViewModel? GetClassEnrollment(string classId)
        {
            var classInfo = _classRepo.GetById(classId);
            if (classInfo == null) return null;

            var enrolledRelations = _studentClassRepo.GetByClassId(classId);
            var enrolledStudentIds = enrolledRelations.Select(x => x.StudentId).ToList();
            var allStudents = _studentRepo.GetAll();

            return new ClassEnrollmentViewModel
            {
                ClassInfo = classInfo,
                EnrolledStudents = allStudents.Where(s => enrolledStudentIds.Contains(s.StudentId)).ToList(),
                AvailableStudents = allStudents.Where(s => !enrolledStudentIds.Contains(s.StudentId)).ToList()
            };
        }

        public (bool Success, string Message) AddClass(ClassModel classModel, List<string> subjectIds, List<string> teacherIds)
        {
            try
            {
                if (classModel == null)
                    return (false, "Class model cannot be null");

                if (string.IsNullOrEmpty(classModel.ClassName))
                    return (false, "Class name is required");

                if (string.IsNullOrEmpty(classModel.Semester))
                    return (false, "Semester is required");

                if (subjectIds == null || !subjectIds.Any() || teacherIds == null || !teacherIds.Any())
                    return (false, "At least one Subject-Teacher pair is required");

                // Validate subject-teacher pairs
                var allSubjects = _subjectRepo.GetAll()
                    .Where(s => !string.IsNullOrWhiteSpace(s.SubjectId))
                    .ToDictionary(s => s.SubjectId!, s => s);
                var allTeachers = _teacherRepo.GetAll()
                    .Where(t => !string.IsNullOrWhiteSpace(t.TeacherId))
                    .ToDictionary(t => t.TeacherId!, t => t);

                for (int i = 0; i < subjectIds.Count && i < teacherIds.Count; i++)
                {
                    var sid = subjectIds[i];
                    var tid = teacherIds[i];

                    if (!allSubjects.ContainsKey(sid))
                        return (false, $"Subject '{sid}' not found");
                    if (!allTeachers.ContainsKey(tid))
                        return (false, $"Teacher '{tid}' not found");

                    var subject = allSubjects[sid];
                    var teacher = allTeachers[tid];

                    // Teacher must be assigned to subject
                    var subjectTeacherIds = (subject.TeacherIds ?? string.Empty)
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Trim())
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);

                    if (!subjectTeacherIds.Contains(tid))
                        return (false, $"Teacher '{tid}' is not assigned to Subject '{sid}'");

                    // Department consistency check: subject & teacher must belong to class department
                    var sDept = subject.DepartmentId ?? string.Empty;
                    var tDept = teacher.DepartmentId ?? string.Empty;
                    if (!string.Equals(sDept, classModel.DepartmentId, StringComparison.OrdinalIgnoreCase))
                        return (false, $"Subject '{sid}' does not belong to Department '{classModel.DepartmentId}'");
                    if (!string.Equals(tDept, classModel.DepartmentId, StringComparison.OrdinalIgnoreCase))
                        return (false, $"Teacher '{tid}' does not belong to Department '{classModel.DepartmentId}'");
                }

                // Ensure subjects belong to selected Department
                foreach (var sid in subjectIds)
                {
                    var subject = allSubjects[sid];
                    var sDept = subject.DepartmentId ?? string.Empty;
                    if (!string.Equals(sDept, classModel.DepartmentId ?? string.Empty, StringComparison.OrdinalIgnoreCase))
                        return (false, $"Subject '{sid}' does not belong to Department '{classModel.DepartmentId}'");
                }

                // Add class
                _classRepo.Add(classModel);

                // Add class subjects
                for (int i = 0; i < subjectIds.Count && i < teacherIds.Count; i++)
                {
                    var classSubject = new ClassSubjectModel
                    {
                        ClassId = classModel.ClassId,
                        SubjectId = subjectIds[i],
                        TeacherId = teacherIds[i]
                    };
                    _classSubjectRepo.Add(classSubject);
                }

                return (true, "Class added successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error adding class: {ex.Message}");
            }
        }

        public (bool Success, string Message) UpdateClass(ClassModel classModel, List<string> subjectIds, List<string> teacherIds)
        {
            try
            {
                if (classModel == null)
                    return (false, "Class model cannot be null");

                if (string.IsNullOrEmpty(classModel.ClassName))
                    return (false, "Class name is required");

                if (string.IsNullOrEmpty(classModel.Semester))
                    return (false, "Semester is required");

                if (subjectIds == null || !subjectIds.Any() || teacherIds == null || !teacherIds.Any())
                    return (false, "At least one Subject-Teacher pair is required");

                // Validate subject-teacher pairs
                var allSubjects = _subjectRepo.GetAll()
                    .Where(s => !string.IsNullOrWhiteSpace(s.SubjectId))
                    .ToDictionary(s => s.SubjectId!, s => s);
                var allTeachers = _teacherRepo.GetAll()
                    .Where(t => !string.IsNullOrWhiteSpace(t.TeacherId))
                    .ToDictionary(t => t.TeacherId!, t => t);

                for (int i = 0; i < subjectIds.Count && i < teacherIds.Count; i++)
                {
                    var sid = subjectIds[i];
                    var tid = teacherIds[i];

                    if (!allSubjects.ContainsKey(sid))
                        return (false, $"Subject '{sid}' not found");
                    if (!allTeachers.ContainsKey(tid))
                        return (false, $"Teacher '{tid}' not found");

                    var subject = allSubjects[sid];
                    var teacher = allTeachers[tid];

                    var subjectTeacherIds = (subject.TeacherIds ?? string.Empty)
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Trim())
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);

                    if (!subjectTeacherIds.Contains(tid))
                        return (false, $"Teacher '{tid}' is not assigned to Subject '{sid}'");

                    var sDept = subject.DepartmentId ?? string.Empty;
                    var tDept = teacher.DepartmentId ?? string.Empty;
                    if (!string.Equals(sDept, tDept, StringComparison.OrdinalIgnoreCase))
                        return (false, $"Teacher '{tid}' does not belong to Subject '{sid}' department");
                }

                // Update class
                _classRepo.Update(classModel);

                // Remove old subject-teacher mappings
                _classSubjectRepo.DeleteByClassId(classModel.ClassId ?? string.Empty);

                // Add new subject-teacher mappings
                for (int i = 0; i < subjectIds.Count && i < teacherIds.Count; i++)
                {
                    var classSubject = new ClassSubjectModel
                    {
                        ClassId = classModel.ClassId,
                        SubjectId = subjectIds[i],
                        TeacherId = teacherIds[i]
                    };
                    _classSubjectRepo.Add(classSubject);
                }

                return (true, "Class updated successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error updating class: {ex.Message}");
            }
        }

        public (bool Success, string Message) DeleteClass(string classId)
        {
            try
            {
                // Delete student enrollments
                var enrollments = _studentClassRepo.GetByClassId(classId);
                foreach (var enrollment in enrollments)
                {
                    _studentClassRepo.Remove(classId, enrollment.StudentId ?? string.Empty);
                }

                // Delete class subjects
                _classSubjectRepo.DeleteByClassId(classId);

                // Delete class
                _classRepo.Delete(classId);

                return (true, "Class deleted successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error deleting class: {ex.Message}");
            }
        }

        public (bool Success, string Message) AddStudentsToClass(string classId, List<string> studentIds)
        {
            try
            {
                if (string.IsNullOrEmpty(classId))
                    return (false, "Class ID cannot be empty");

                if (studentIds == null || !studentIds.Any())
                    return (false, "At least one student must be selected");

                var addedCount = 0;
                foreach (var studentId in studentIds)
                {
                    if (!_studentClassRepo.IsEnrolled(classId, studentId))
                    {
                        var enrollment = new StudentClassModel
                        {
                            ClassId = classId,
                            StudentId = studentId,
                            JoinedDate = DateTime.Now
                        };
                        _studentClassRepo.Add(enrollment);
                        addedCount++;
                    }
                }

                // Update class student count
                var currentClass = _classRepo.GetById(classId);
                if (currentClass != null)
                {
                    currentClass.NumberOfStudents = _studentClassRepo.GetByClassId(classId).Count;
                    _classRepo.Update(currentClass);
                }

                return (true, $"{addedCount} student(s) added to class");
            }
            catch (Exception ex)
            {
                return (false, $"Error adding students: {ex.Message}");
            }
        }

        public (bool Success, string Message) RemoveStudentFromClass(string classId, string studentId)
        {
            try
            {
                if (string.IsNullOrEmpty(classId) || string.IsNullOrEmpty(studentId))
                    return (false, "Class ID and Student ID cannot be empty");

                _studentClassRepo.Remove(classId, studentId);

                // Update class student count
                var currentClass = _classRepo.GetById(classId);
                if (currentClass != null)
                {
                    currentClass.NumberOfStudents = _studentClassRepo.GetByClassId(classId).Count;
                    _classRepo.Update(currentClass);
                }

                return (true, "Student removed from class");
            }
            catch (Exception ex)
            {
                return (false, $"Error removing student: {ex.Message}");
            }
        }
    }
}
