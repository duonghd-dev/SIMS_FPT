using CsvHelper.Configuration.Attributes;

namespace SIMS_FPT.Models
{
    public class Users
    {
        [Name("Id")]
        public int Id { get; set; }

        [Name("Email")]
        public string Email { get; set; }

        [Name("Password")]
        public string Password { get; set; }

        [Name("Role")]
        public string Role { get; set; }

        [Name("LinkedId")]
        public string LinkedId { get; set; }

        [Name("FullName")]
        public string FullName { get; set; }

        [Name("HashAlgorithm")]
        public string HashAlgorithm { get; set; } = "PBKDF2";
    }
}