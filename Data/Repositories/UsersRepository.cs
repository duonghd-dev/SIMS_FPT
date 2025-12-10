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
                return csv.GetRecords<Users>().ToList();
            }
            catch
            {
                return new List<Users>();
            }
        }

        private void WriteAllUsersInternal(IEnumerable<Users> users)
        {
            var dir = Path.GetDirectoryName(_filePath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            using var writer = new StreamWriter(_filePath, false);
            using var csv = new CsvWriter(writer, _config);
            csv.WriteRecords(users);
        }

        // --- LOGIN BẰNG EMAIL ---
        public Users? Login(string email, string password)
        {
            var users = ReadAllUsersInternal();

            // Tìm theo Email
            var user = users.FirstOrDefault(u => u.Email.Trim().Equals(email.Trim(), StringComparison.OrdinalIgnoreCase));
            if (user == null) return null;

            if (string.Equals(user.HashAlgorithm, "PBKDF2", StringComparison.OrdinalIgnoreCase))
            {
                if (PasswordHasherHelper.Verify(password, user.Password)) return user;
            }
            else if (string.IsNullOrEmpty(user.HashAlgorithm) || user.HashAlgorithm == "Plain")
            {
                if (user.Password == password)
                {
                    user.Password = PasswordHasherHelper.Hash(password);
                    user.HashAlgorithm = "PBKDF2";
                    WriteAllUsersInternal(users);
                    return user;
                }
            }
            return null;
        }

        public void AddUser(Users newUser)
        {
            var users = ReadAllUsersInternal();
            newUser.Id = users.Any() ? users.Max(u => u.Id) + 1 : 1;

            // Luôn hash pass khi tạo mới
            if (newUser.HashAlgorithm == "Plain" || string.IsNullOrEmpty(newUser.HashAlgorithm))
            {
                newUser.Password = PasswordHasherHelper.Hash(newUser.Password);
                newUser.HashAlgorithm = "PBKDF2";
            }

            users.Add(newUser);
            WriteAllUsersInternal(users);
        }

        // Đổi tên hàm check trùng từ UsernameExists -> EmailExists
        public bool EmailExists(string email)
        {
            return ReadAllUsersInternal().Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }

        // Các hàm phụ trợ
        public List<Users> GetInstructors() => ReadAllUsersInternal().Where(u => u.Role == "Instructor").ToList();

        public Users? GetUserById(int id) => ReadAllUsersInternal().FirstOrDefault(u => u.Id == id);

        public void DeleteUserByEmail(string email)
        {
            if (string.IsNullOrEmpty(email)) return;
            var users = ReadAllUsersInternal();
            var newList = users.Where(u => !u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)).ToList();
            if (newList.Count != users.Count) WriteAllUsersInternal(newList);
        }

        public bool UsernameExists(string u) => EmailExists(u); // Alias tạm thời
        public void AddTeacherUser(TeacherCSVModel t) { }
        public void UpdateUserFromTeacher(TeacherCSVModel t, string? old) { }
        public void DeleteUserByUsername(string u) => DeleteUserByEmail(u);
    }
}