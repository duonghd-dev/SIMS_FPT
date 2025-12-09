namespace SIMS_FPT.Models
{
    public class Users
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;

        // Lưu chuỗi hashed (hoặc plaintext tạm thời khi chưa migrate)
        public string Password { get; set; } = string.Empty;

        // Giá trị: "Plain" (mật khẩu chưa được hash), "PBKDF2" (PasswordHasher), ...
        public string HashAlgorithm { get; set; } = "Plain";

        public string Role { get; set; } = string.Empty;    // Admin, Instructor, Student
        public string FullName { get; set; } = string.Empty;
    }
}
