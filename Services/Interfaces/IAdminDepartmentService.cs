using SIMS_FPT.Models;
using System.Collections.Generic;

namespace SIMS_FPT.Services.Interfaces
{
    public interface IAdminDepartmentService
    {
        List<DepartmentModel> GetAllDepartments();
        DepartmentModel? GetDepartmentById(string id);
        (bool Success, string Message) AddDepartment(DepartmentModel model);
        (bool Success, string Message) UpdateDepartment(DepartmentModel model);
        (bool Success, string Message) DeleteDepartment(string id);
    }
}
