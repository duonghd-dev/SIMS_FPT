using CsvHelper.Configuration.Attributes;
using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIMS_FPT.Models
{
    public class StudentCSVModel
    {
        [Name("first_name")]
        public string FirstName { get; set; }

        [Name("last_name")]
        public string LastName { get; set; }

        [Name("class_name")]
        public string ClassName { get; set; }

        [Name("gender")]
        public string Gender { get; set; }

        [Name("date_of_birth")]
        public DateTime DateOfBirth { get; set; }

        [Name("email")]
        public string? Email { get; set; }

        [Name("major")]
        public string? Major { get; set; }

        [Name("image_path")]
        public string? ImagePath { get; set; }

        // --- Các thuộc tính hỗ trợ (Không lưu vào CSV) ---
        [Ignore]
        public string FullName => $"{FirstName} {LastName}";

        [Ignore]
        [NotMapped]
        public IFormFile? StudentImageFile { get; set; } // Hứng file từ Form upload
    }
}