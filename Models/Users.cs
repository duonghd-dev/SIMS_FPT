using CsvHelper.Configuration.Attributes;

namespace SIMS_FPT.Models
{
    public class Users
    {
        [Name("Id")]
        public int Id { get; set; }

        [Name("Email")]
        public string Email { get; set; } = string.Empty;

        [Name("Password")]
        public string Password { get; set; } = string.Empty;

        [Name("Role")]
        public string Role { get; set; } = string.Empty;

        [Name("LinkedId")]
        public string LinkedId { get; set; } = string.Empty;

        [Name("FullName")]
        public string FullName { get; set; } = string.Empty;

        [Name("HashAlgorithm")]
        public string HashAlgorithm { get; set; } = "PBKDF2";
    }
}