using SIMS_Project.Models;
using System.Collections.Generic;

namespace SIMS_Project.Interface
{
    public interface IUserRepository
    {
        // Chỉ khai báo MỘT lần cho mỗi hàm
        Login? Login(string username, string password);
        void AddUser(Login user);
        bool UsernameExists(string username);

        List<Login> GetInstructors();
        Login? GetUserById(int id);
    }
}