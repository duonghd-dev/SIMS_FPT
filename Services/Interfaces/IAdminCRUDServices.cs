using SIMS_FPT.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SIMS_FPT.Services.Interfaces
{
    public interface IAdminInstructorService
    {
        List<TeacherCSVModel> GetAllInstructors();
        TeacherCSVModel? GetInstructorById(string id);
        Task<(bool Success, string Message)> AddInstructor(TeacherCSVModel model);
        Task<(bool Success, string Message)> UpdateInstructor(TeacherCSVModel model);
        Task<(bool Success, string Message)> DeleteInstructor(string id);
    }

    public interface IAdminStudentService
    {
        List<StudentCSVModel> GetAllStudents(string? searchTerm = null);
        StudentCSVModel? GetStudentById(string id);
        Task<(bool Success, string Message)> AddStudent(StudentCSVModel model);
        Task<(bool Success, string Message)> UpdateStudent(StudentCSVModel model);
        Task<(bool Success, string Message)> DeleteStudent(string id);
    }
}
