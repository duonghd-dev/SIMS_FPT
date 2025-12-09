using CsvHelper;
using CsvHelper.Configuration;
using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Helpers;
using SIMS_FPT.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace SIMS_FPT.Data.Repositories
{
    public class UsersRepository : IUserRepository
    {
        private readonly string _filePath;
        private readonly CsvConfiguration _config;

        public UsersRepository()
        {
            _filePath = Path.Combine(Directory.GetCurrentDirectory(), "CSV_DATA", "users.csv");

            _config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                HeaderValidated = null
            };
        }

        private List<Users> ReadAllUsersInternal()
        {
            if (!File.Exists(_filePath)) return new List<Users>();

            try
            {
                using var reader = new StreamReader(_filePath);
                using var csv = new CsvReader(reader, _config);
                var records = csv.GetRecords<Users>().ToList();
                return records ?? new List<Users>();
            }
            catch
            {
                return new List<Users>();
            }
        }

        private void WriteAllUsersInternal(IEnumerable<Users> users)
        {
            var dir = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            using var writer = new StreamWriter(_filePath, false);
            using var csv = new CsvWriter(writer, _config);

            csv.WriteHeader<Users>();
            csv.NextRecord();
            csv.WriteRecords(users);
        }

        // ------------------------------
        // Implement Interface
        // ------------------------------
        public Users? Login(string username, string password)
        {
            var users = ReadAllUsersInternal();

            var user = users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
            if (user == null) return null;

            // If already hashed with PBKDF2 (PasswordHasher), verify accordingly
            if (!string.IsNullOrEmpty(user.HashAlgorithm) && user.HashAlgorithm.Equals("PBKDF2", StringComparison.OrdinalIgnoreCase))
            {
                if (PasswordHasherHelper.Verify(password, user.Password))
                    return user;

                return null;
            }

            // If password stored in plain text (legacy), compare and upgrade to hashed
            if (string.IsNullOrEmpty(user.HashAlgorithm) || user.HashAlgorithm.Equals("Plain", StringComparison.OrdinalIgnoreCase))
            {
                if (user.Password == password)
                {
                    // Upgrade: hash and set HashAlgorithm
                    user.Password = PasswordHasherHelper.Hash(password);
                    user.HashAlgorithm = "PBKDF2";
                    WriteAllUsersInternal(users);
                    return user;
                }

                return null;
            }

            // Unknown algorithm -> reject (or implement other branches)
            return null;
        }

        public void AddUser(Users newUser)
        {
            var users = ReadAllUsersInternal();

            // assign id
            newUser.Id = users.Any() ? users.Max(u => u.Id) + 1 : 1;

            // Hash password before saving
            newUser.Password = PasswordHasherHelper.Hash(newUser.Password);
            newUser.HashAlgorithm = "PBKDF2";

            users.Add(newUser);
            WriteAllUsersInternal(users);
        }

        public bool UsernameExists(string username)
        {
            return ReadAllUsersInternal().Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        }

        public List<Users> GetInstructors()
        {
            return ReadAllUsersInternal()
                .Where(u => u.Role.Equals("Instructor", StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public Users? GetUserById(int id)
        {
            return ReadAllUsersInternal().FirstOrDefault(u => u.Id == id);
        }

        // ------------------------------
        // Sync TeacherCSV
        // ------------------------------
        public void AddTeacherUser(TeacherCSVModel teacher)
        {
            if (teacher == null) return;

            var users = ReadAllUsersInternal();

            if (users.Any(u => u.Username.Equals(teacher.Username, StringComparison.OrdinalIgnoreCase)))
                return;

            var newUser = new Users
            {
                Id = users.Any() ? users.Max(u => u.Id) + 1 : 1,
                Email = teacher.Email ?? string.Empty,
                Username = teacher.Username ?? string.Empty,
                Password = PasswordHasherHelper.Hash(teacher.Password ?? string.Empty),
                HashAlgorithm = "PBKDF2",
                FullName = teacher.Name ?? string.Empty,
                Role = "Instructor"
            };

            users.Add(newUser);
            WriteAllUsersInternal(users);
        }

        public void UpdateUserFromTeacher(TeacherCSVModel teacher, string? oldUsername = null)
        {
            if (teacher == null) return;

            var users = ReadAllUsersInternal();

            Users? existing = null;

            if (!string.IsNullOrEmpty(oldUsername))
                existing = users.FirstOrDefault(u => u.Username.Equals(oldUsername, StringComparison.OrdinalIgnoreCase));

            existing ??= users.FirstOrDefault(u => u.Username.Equals(teacher.Username, StringComparison.OrdinalIgnoreCase));

            if (existing != null)
            {
                existing.Username = teacher.Username ?? existing.Username;
                existing.Email = teacher.Email ?? existing.Email;
                existing.Password = PasswordHasherHelper.Hash(teacher.Password ?? string.Empty);
                existing.HashAlgorithm = "PBKDF2";
                existing.FullName = teacher.Name ?? existing.FullName;
                existing.Role = "Instructor";

                WriteAllUsersInternal(users);
            }
            else
            {
                AddTeacherUser(teacher);
            }
        }

        public void DeleteUserByUsername(string username)
        {
            if (string.IsNullOrEmpty(username)) return;

            var users = ReadAllUsersInternal();
            var newList = users.Where(u => !u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)).ToList();

            if (newList.Count != users.Count())
                WriteAllUsersInternal(newList);
        }
    }
}
