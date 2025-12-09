using SIMS_FPT.Models;
using SIMS_Project.Models;

namespace SIMS_Project.Interface
{
    public interface IUserRepository
    {
        Login? Login(string username, string password);
        void AddUser(Login user);
        bool UsernameExists(string username);

        List<Login> GetInstructors();
        Login? GetUserById(int id);

        // CSV sync helpers
        void AddTeacherUser(TeacherCSVModel teacher);
        void UpdateUserFromTeacher(TeacherCSVModel teacher, string oldUsername = null);
        void DeleteUserByUsername(string username);
    }
}
