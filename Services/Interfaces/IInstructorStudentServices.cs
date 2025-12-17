using SIMS_FPT.Models;
using SIMS_FPT.Models.ViewModels;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace SIMS_FPT.Services.Interfaces
{
    public interface IInstructorCourseMaterialService
    {
        List<CourseMaterialModel> GetTeacherMaterials(string teacherId);
        (bool Success, string Message, string? FilePath) CreateMaterial(CourseMaterialModel model, IFormFile? uploadFile, string teacherId);
        (bool Success, string Message) DeleteMaterial(string materialId, string teacherId);
        List<string> GetTeacherSubjectIds(string teacherId);
        List<(string SubjectId, string DisplayText)> GetTeacherClassSubjectList(string teacherId);
        CourseMaterialModel GetMaterialById(string id);
        (bool success, string message) UpdateMaterial(CourseMaterialModel model, IFormFile? newFile, string teacherId);
    }

    public interface IInstructorStudentService
    {
        StudentProfileViewModel? GetStudentProfile(string studentId, string teacherId);
        // Added methods for Student Management
        InstructorStudentListViewModel GetManagedStudents(string teacherId, string? classId, string? searchTerm);
        bool RemoveStudentFromClass(string teacherId, string classId, string studentId);
    }

    public interface IStudentAssignmentService
    {
        List<StudentAssignmentViewModel> GetStudentAssignments(string studentId);
        StudentAssignmentViewModel? GetAssignmentForSubmission(string assignmentId, string studentId);
        (bool Success, string Message, string? FilePath) SubmitAssignment(string assignmentId, string studentId, IFormFile uploadFile);
        bool IsStudentEligible(string studentId, string assignmentId);
    }
}