using CsvHelper.Configuration.Attributes;
using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIMS_FPT.Models
{
    public class StudentCSVModel
    {

        [Name("student_id")]
        public string StudentId { get; set; }

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

        [Name("religion")]
        public string? Religion { get; set; }

        [Name("joining_date")]
        public DateTime AdmissionDate { get; set; }

        [Name("mobile_number")]
        public string? MobileNumber { get; set; }

        [Name("admission_number")]
        public string? AdmissionNumber { get; set; }

        [Name("section")]
        public string? Section { get; set; }

        [Name("email")]
        public string? Email { get; set; }

        [Name("image_path")]
        public string? ImagePath { get; set; }

        // --- Địa chỉ ---
        [Name("address")]
        public string? Address { get; set; }

        [Name("permanent_address")]
        public string? PermanentAddress { get; set; }

        // --- Thông tin phụ huynh ---
        [Name("father_name")]
        public string? FatherName { get; set; }
        [Name("father_occupation")]
        public string? FatherOccupation { get; set; }
        [Name("father_mobile")]
        public string? FatherMobile { get; set; }
        [Name("father_email")]
        public string? FatherEmail { get; set; }

        [Name("mother_name")]
        public string? MotherName { get; set; }
        [Name("mother_occupation")]
        public string? MotherOccupation { get; set; }
        [Name("mother_mobile")]
        public string? MotherMobile { get; set; }
        [Name("mother_email")]
        public string? MotherEmail { get; set; }

        // --- Các thuộc tính hỗ trợ (Không lưu vào CSV) ---
        [Ignore]
        public string FullName => $"{FirstName} {LastName}";

        [Ignore]
        [NotMapped]
        public IFormFile? StudentImageFile { get; set; } // Hứng file từ Form upload
    }
}