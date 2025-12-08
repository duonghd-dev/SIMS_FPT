using CsvHelper;
using CsvHelper.Configuration;
using SIMS_Project.Interface;
using SIMS_Project.Models;
using System.Globalization;
using System.IO;

namespace SIMS_Project.Data
{
    public class LoginRepository : IUserRepository
    {
        private readonly string _filePath;

        public LoginRepository()
        {
            // 1. Lấy thư mục gốc của Web
            var currentDir = Directory.GetCurrentDirectory();

            _filePath = Path.Combine(currentDir, "CSV_DATA", "users.csv");
        }

        private List<Login> GetAllUsers()
        {
            if (!File.Exists(_filePath)) return new List<Login>();

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null
            };

            using (var reader = new StreamReader(_filePath))
            using (var csv = new CsvReader(reader, config))
            {
                return csv.GetRecords<Login>().ToList();
            }
        }

        public List<Login> GetInstructors()
        {
            return GetAllUsers().Where(u => u.Role == "Instructor").ToList();
        }

        public Login? GetUserById(int id)
        {
            return GetAllUsers().FirstOrDefault(u => u.Id == id);
        }

        public Login? Login(string username, string password)
        {
            // Debug: In ra đường dẫn file đang dùng để kiểm tra
            Console.WriteLine($"--- DEBUG LOGIN ---");
            Console.WriteLine($"Đang đọc file tại: {_filePath}");

            if (!File.Exists(_filePath))
            {
                Console.WriteLine("LỖI: Không tìm thấy file users.csv tại đường dẫn trên!");
                return null;
            }

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null
            };

            using (var reader = new StreamReader(_filePath))
            using (var csv = new CsvReader(reader, config))
            {
                try
                {
                    var users = csv.GetRecords<Login>().ToList();
                    Console.WriteLine($"Đã đọc được {users.Count} user.");

                    // Tìm user khớp
                    var user = users.FirstOrDefault(u => u.Username == username && u.Password == password);

                    if (user == null)
                        Console.WriteLine($"KẾT QUẢ: Sai thông tin! Nhập: '{username}'/'{password}'");
                    else
                        Console.WriteLine("KẾT QUẢ: Đăng nhập thành công!");

                    return user;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi khi đọc CSV: {ex.Message}");
                    return null;
                }
            }
        }

        public void AddUser(Login newUser)
        {
            var users = GetAllUsers();
            newUser.Id = users.Any() ? users.Max(u => u.Id) + 1 : 1;

            bool append = File.Exists(_filePath);
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = !append
            };

            var directory = Path.GetDirectoryName(_filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using (var stream = File.Open(_filePath, FileMode.Append))
            using (var writer = new StreamWriter(stream))
            using (var csv = new CsvWriter(writer, config))
            {
                csv.WriteRecord(newUser);
                csv.NextRecord();
            }
        }

        public bool UsernameExists(string username)
        {
            if (!File.Exists(_filePath)) return false;

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null
            };

            using (var reader = new StreamReader(_filePath))
            using (var csv = new CsvReader(reader, config))
            {
                var users = csv.GetRecords<Login>().ToList();
                return users.Any(u => u.Username == username);
            }
        }
    }
}