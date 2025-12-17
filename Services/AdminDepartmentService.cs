using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Models;
using SIMS_FPT.Services.Interfaces;
using System;
using System.Collections.Generic;

namespace SIMS_FPT.Services
{
    public class AdminDepartmentService : IAdminDepartmentService
    {
        private readonly IDepartmentRepository _deptRepo;

        public AdminDepartmentService(IDepartmentRepository deptRepo)
        {
            _deptRepo = deptRepo;
        }

        public List<DepartmentModel> GetAllDepartments() => _deptRepo.GetAll();

        public DepartmentModel? GetDepartmentById(string id) => _deptRepo.GetById(id);

        public (bool Success, string Message) AddDepartment(DepartmentModel model)
        {
            try
            {
                if (model == null)
                    return (false, "Department model cannot be null");

                if (string.IsNullOrWhiteSpace(model.DepartmentId))
                    return (false, "Department ID is required");

                if (string.IsNullOrWhiteSpace(model.DepartmentName))
                    return (false, "Department Name is required");

                if (model.NoOfStudents < 0)
                    return (false, "Number of students cannot be negative");

                // Check duplicate
                if (_deptRepo.GetById(model.DepartmentId) != null)
                    return (false, "Department ID already exists");

                _deptRepo.Add(model);
                return (true, "Department added successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error adding department: {ex.Message}");
            }
        }

        public (bool Success, string Message) UpdateDepartment(DepartmentModel model)
        {
            try
            {
                if (model == null)
                    return (false, "Department model cannot be null");

                if (string.IsNullOrWhiteSpace(model.DepartmentId))
                    return (false, "Department ID is required");

                if (model.NoOfStudents < 0)
                    return (false, "Number of students cannot be negative");

                _deptRepo.Update(model);
                return (true, "Department updated successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error updating department: {ex.Message}");
            }
        }

        public (bool Success, string Message) DeleteDepartment(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    return (false, "Department ID cannot be empty");

                _deptRepo.Delete(id);
                return (true, "Department deleted successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error deleting department: {ex.Message}");
            }
        }
    }
}
