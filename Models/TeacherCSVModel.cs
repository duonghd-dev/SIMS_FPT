using CsvHelper.Configuration.Attributes;
using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIMS_FPT.Models
{
    public class TeacherCSVModel
    {
        [Name("teacher_id")]
        public string TeacherId { get; set; }

        [Name("name")]
        public string Name { get; set; }

        [Name("department_id")]
        public string? DepartmentId { get; set; }

        [Name("gender")]
        public string Gender { get; set; }

        [Name("date_of_birth")]
        public DateTime? DateOfBirth { get; set; }

        [Name("mobile")]
        public string Mobile { get; set; }

        [Name("joining_date")]
        public DateTime? JoiningDate { get; set; }

        [Name("qualification")]
        public string Qualification { get; set; }

        [Name("experience")]
        public string? Experience { get; set; }

        // --- Login Details (Map với CSV) ---
        [Name("username")]
        public string? Username { get; set; }

        [Name("email")]
        public string Email { get; set; }

        [Name("password")]
        public string? Password { get; set; }

        // --- Address Info ---
        [Name("address")]
        public string? Address { get; set; }

        [Name("city")]
        public string? City { get; set; }

        [Name("state")]
        public string? State { get; set; }

        [Name("country")]
        public string? Country { get; set; }

        [Name("image")] // Cột trong CSV là 'image'
        public string? ImagePath { get; set; }

        // --- Helper Properties (Không lưu vào CSV) ---
        [Ignore]
        [NotMapped]
        public IFormFile? TeacherImageFile { get; set; }
    }
}