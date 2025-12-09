using SIMS_FPT.Models;
using System.Collections.Generic;

namespace SIMS_FPT.Data.Interfaces
{
    public interface IUserRepository
    {
        Users? Login(string username, string password);
        void AddUser(Users newUser);
        bool UsernameExists(string username);

        List<Users> GetInstructors();
        Users? GetUserById(int id);

        void AddTeacherUser(TeacherCSVModel teacher);
        void UpdateUserFromTeacher(TeacherCSVModel teacher, string? oldUsername = null);
        void DeleteUserByUsername(string username);
    }
}
