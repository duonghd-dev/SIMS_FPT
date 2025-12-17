using SIMS_FPT.Models;
using SIMS_FPT.Models.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SIMS_FPT.Services.Interfaces
{
    public interface IInstructorAssignmentService
    {
        List<AssignmentModel> GetTeacherAssignments(string teacherId);
        AssignmentModel? GetAssignmentById(string assignmentId, string teacherId);
        (bool Success, string Message) CreateAssignment(AssignmentModel model, string teacherId);
        (bool Success, string Message) UpdateAssignment(AssignmentModel model, string teacherId);
        (bool Success, string Message) DeleteAssignment(string assignmentId, string teacherId);
        bool TeacherOwnsClass(string classId, string teacherId);
        string GenerateAssignmentId();
        string? GetFirstSubjectForClass(string classId);
    }
}
