using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Models;
using SIMS_FPT.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SIMS_FPT.Services
{
    public class AdminInstructorService : IAdminInstructorService
    {
        private readonly ITeacherRepository _repo;
        private readonly TeacherService _teacherService;

        public AdminInstructorService(ITeacherRepository repo, TeacherService teacherService)
        {
            _repo = repo;
            _teacherService = teacherService;
        }

        public List<TeacherCSVModel> GetAllInstructors() => _repo.GetAll();

        public TeacherCSVModel? GetInstructorById(string id) => _repo.GetById(id);

        public async Task<(bool Success, string Message)> AddInstructor(TeacherCSVModel model)
        {
            try
            {
                await _teacherService.Add(model);
                return (true, "Instructor added successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> UpdateInstructor(TeacherCSVModel model)
        {
            try
            {
                await _teacherService.Update(model);
                return (true, "Instructor updated successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> DeleteInstructor(string id)
        {
            try
            {
                _teacherService.Delete(id);
                return (true, "Instructor deleted successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }
    }

    public class AdminStudentService : IAdminStudentService
    {
        private readonly IStudentRepository _repo;
        private readonly StudentService _studentService;

        public AdminStudentService(IStudentRepository repo, StudentService studentService)
        {
            _repo = repo;
            _studentService = studentService;
        }

        public List<StudentCSVModel> GetAllStudents(string? searchTerm = null)
        {
            var data = _repo.GetAll();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                var keyword = searchTerm.Trim().ToLower();
                data = data.Where(s =>
                    (s.StudentId != null && s.StudentId.ToLower().Contains(keyword)) ||
                    (s.FullName != null && s.FullName.ToLower().Contains(keyword))
                ).ToList();
            }

            return data;
        }

        public StudentCSVModel? GetStudentById(string id) => _repo.GetById(id);

        public async Task<(bool Success, string Message)> AddStudent(StudentCSVModel model)
        {
            try
            {
                await _studentService.Add(model);
                return (true, "Student added successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> UpdateStudent(StudentCSVModel model)
        {
            try
            {
                await _studentService.Update(model);
                return (true, "Student updated successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> DeleteStudent(string id)
        {
            try
            {
                _repo.Delete(id);
                return (true, "Student deleted successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }
    }
}
