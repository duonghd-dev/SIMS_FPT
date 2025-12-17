using SIMS_FPT.Models;
using SIMS_FPT.Models.ViewModels;
using System.Collections.Generic;

namespace SIMS_FPT.Services.Interfaces
{
    public interface IAdminClassService
    {
        /// <summary>
        /// Get all classes with their subject-teacher mappings for list view
        /// </summary>
        List<ClassDetailViewModel> GetAllClassesWithDetails();

        /// <summary>
        /// Get a single class with its subject-teacher mappings for edit/detail view
        /// </summary>
        ClassDetailViewModel? GetClassWithDetails(string classId);

        /// <summary>
        /// Get class enrollment information
        /// </summary>
        ClassEnrollmentViewModel? GetClassEnrollment(string classId);

        /// <summary>
        /// Add a new class with subject-teacher mappings
        /// </summary>
        (bool Success, string Message) AddClass(ClassModel classModel, List<string> subjectIds, List<string> teacherIds);

        /// <summary>
        /// Update class information and subject-teacher mappings
        /// </summary>
        (bool Success, string Message) UpdateClass(ClassModel classModel, List<string> subjectIds, List<string> teacherIds);

        /// <summary>
        /// Delete a class and its related records
        /// </summary>
        (bool Success, string Message) DeleteClass(string classId);

        /// <summary>
        /// Add students to a class
        /// </summary>
        (bool Success, string Message) AddStudentsToClass(string classId, List<string> studentIds);

        /// <summary>
        /// Remove a student from a class
        /// </summary>
        (bool Success, string Message) RemoveStudentFromClass(string classId, string studentId);
    }
}
