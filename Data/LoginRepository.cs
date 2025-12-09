using CsvHelper;
using CsvHelper.Configuration;
using SIMS_Project.Interface;
using SIMS_Project.Models;
using SIMS_FPT.Models;
using System.Globalization;
using System.IO;

namespace SIMS_Project.Data
{
    public class LoginRepository : IUserRepository
    {
        private readonly string _filePath;
        private readonly CsvConfiguration _csvConfig;

        public LoginRepository()
        {
            var currentDir = Directory.GetCurrentDirectory();
            var folder = Path.Combine(currentDir, "CSV_DATA");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            _filePath = Path.Combine(folder, "users.csv");

            _csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null
            };
        }

        private List<Login> ReadAllUsersInternal()
        {
            if (!File.Exists(_filePath)) return new List<Login>();

            using (var reader = new StreamReader(_filePath))
            using (var csv = new CsvReader(reader, _csvConfig))
            {
                try
                {
                    return csv.GetRecords<Login>().ToList();
                }
                catch
                {
                    return new List<Login>();
                }
            }
        }

        private void WriteAllUsersInternal(IEnumerable<Login> users)
        {
            // Ensure directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);

            using (var writer = new StreamWriter(_filePath, false))
            using (var csv = new CsvWriter(writer, _csvConfig))
            {
                csv.WriteHeader<Login>();
                csv.NextRecord();
                csv.WriteRecords(users);
            }
        }

        public List<Login> GetInstructors()
        {
            return ReadAllUsersInternal().Where(u => u.Role == "Instructor").ToList();
        }

        public Login? GetUserById(int id)
        {
            return ReadAllUsersInternal().FirstOrDefault(u => u.Id == id);
        }

        public Login? Login(string username, string password)
        {
            var users = ReadAllUsersInternal();
            return users.FirstOrDefault(u =>
                string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase)
                && u.Password == password);
        }

        public void AddUser(Login newUser)
        {
            var users = ReadAllUsersInternal();
            newUser.Id = users.Any() ? users.Max(u => u.Id) + 1 : 1;
            users.Add(newUser);
            WriteAllUsersInternal(users);
        }

        public bool UsernameExists(string username)
        {
            var users = ReadAllUsersInternal();
            return users.Any(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));
        }

        // --- Sync helpers for Teacher CSV ---

        public void AddTeacherUser(TeacherCSVModel teacher)
        {
            var users = ReadAllUsersInternal();

            // avoid duplicate username
            if (users.Any(u => string.Equals(u.Username, teacher.Username, StringComparison.OrdinalIgnoreCase)))
                return;

            var newUser = new Login
            {
                Id = users.Any() ? users.Max(u => u.Id) + 1 : 1,
                Email = teacher.Email,
                Username = teacher.Username,
                Password = teacher.Password,
                Role = "Instructor",
                FullName = teacher.Name
            };

            users.Add(newUser);
            WriteAllUsersInternal(users);
        }

        public void UpdateUserFromTeacher(TeacherCSVModel teacher, string oldUsername = null)
        {
            var users = ReadAllUsersInternal();
            // Find by oldUsername (if provided) or by new username
            var existing = (oldUsername != null)
                ? users.FirstOrDefault(u => string.Equals(u.Username, oldUsername, StringComparison.OrdinalIgnoreCase))
                : users.FirstOrDefault(u => string.Equals(u.Username, teacher.Username, StringComparison.OrdinalIgnoreCase));

            if (existing != null)
            {
                existing.Username = teacher.Username;
                existing.Email = teacher.Email;
                existing.Password = teacher.Password;
                existing.FullName = teacher.Name;
                existing.Role = "Instructor";
                WriteAllUsersInternal(users);
            }
            else
            {
                // If not found, add as new
                AddTeacherUser(teacher);
            }
        }

        public void DeleteUserByUsername(string username)
        {
            var users = ReadAllUsersInternal();
            var newList = users.Where(u => !string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase)).ToList();
            WriteAllUsersInternal(newList);
        }
    }
}
