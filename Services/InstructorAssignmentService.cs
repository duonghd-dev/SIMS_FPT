using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Models;
using SIMS_FPT.Models.ViewModels;
using SIMS_FPT.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SIMS_FPT.Services
{
    public class InstructorAssignmentService : IInstructorAssignmentService
    {
        private readonly IAssignmentRepository _assignmentRepo;
        private readonly IClassSubjectRepository _classSubjectRepo;
        private readonly IClassRepository _classRepo;

        public InstructorAssignmentService(
            IAssignmentRepository assignmentRepo,
            IClassSubjectRepository classSubjectRepo,
            IClassRepository classRepo)
        {
            _assignmentRepo = assignmentRepo;
            _classSubjectRepo = classSubjectRepo;
            _classRepo = classRepo;
        }

        public List<AssignmentModel> GetTeacherAssignments(string teacherId)
        {
            return _assignmentRepo.GetAll()
                .Where(a => a.TeacherId == teacherId)
                .ToList();
        }

        public AssignmentModel? GetAssignmentById(string assignmentId, string teacherId)
        {
            var assignment = _assignmentRepo.GetById(assignmentId);
            if (assignment != null && assignment.TeacherId == teacherId)
                return assignment;
            return null;
        }

        public bool TeacherOwnsClass(string classId, string teacherId)
        {
            var targetClass = _classRepo.GetById(classId);
            if (targetClass == null) return false;

            return _classSubjectRepo.GetAll()
                .Any(cs => cs.ClassId == classId && cs.TeacherId == teacherId);
        }

        public string GenerateAssignmentId()
        {
            var allAssignments = _assignmentRepo.GetAll();
            int nextIdNumber = 1;
            foreach (var assign in allAssignments)
            {
                if (!string.IsNullOrEmpty(assign.AssignmentId) && assign.AssignmentId.StartsWith("ASM-"))
                {
                    string numberPart = assign.AssignmentId.Substring(4);
                    if (int.TryParse(numberPart, out int currentNum))
                    {
                        if (currentNum >= nextIdNumber) nextIdNumber = currentNum + 1;
                    }
                }
            }
            return $"ASM-{nextIdNumber:D3}";
        }

        public string? GetFirstSubjectForClass(string classId)
        {
            var firstSubject = _classSubjectRepo.GetByClassId(classId).FirstOrDefault();
            return firstSubject?.SubjectId;
        }

        public (bool Success, string Message) CreateAssignment(AssignmentModel model, string teacherId)
        {
            try
            {
                if (!TeacherOwnsClass(model.ClassId!, teacherId))
                    return (false, "Invalid class selected or you are not the teacher.");

                model.AssignmentId = GenerateAssignmentId();
                model.TeacherId = teacherId;
                model.SubjectId = GetFirstSubjectForClass(model.ClassId!) ?? "";

                _assignmentRepo.Add(model);
                return (true, "Assignment created successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }

        public (bool Success, string Message) UpdateAssignment(AssignmentModel model, string teacherId)
        {
            try
            {
                var existing = _assignmentRepo.GetById(model.AssignmentId);
                if (existing == null)
                    return (false, "Assignment not found");

                if (existing.TeacherId != null && existing.TeacherId != teacherId)
                    return (false, "Access denied");

                if (!TeacherOwnsClass(model.ClassId!, teacherId))
                    return (false, "Invalid class selected");

                model.TeacherId = existing.TeacherId ?? teacherId;
                model.AreGradesPublished = existing.AreGradesPublished;
                model.SubjectId = GetFirstSubjectForClass(model.ClassId!) ?? existing.SubjectId ?? "";

                _assignmentRepo.Update(model);
                return (true, "Assignment updated successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }

        public (bool Success, string Message) DeleteAssignment(string assignmentId, string teacherId)
        {
            try
            {
                var assignment = _assignmentRepo.GetById(assignmentId);
                if (assignment == null)
                    return (false, "Assignment not found");

                if (assignment.TeacherId != teacherId)
                    return (false, "Access denied: You can only delete your own assignments");

                _assignmentRepo.Delete(assignmentId);
                return (true, "Assignment deleted successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error deleting assignment: {ex.Message}");
            }
        }
    }
}
