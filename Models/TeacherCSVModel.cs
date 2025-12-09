using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIMS_FPT.Models
{
    public class TeacherCSVModel
    {
        public string TeacherId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; } = DateTime.MinValue;
        public string Mobile { get; set; } = string.Empty;
        public DateTime JoiningDate { get; set; } = DateTime.MinValue;
        public string Qualification { get; set; } = string.Empty;
        public string Experience { get; set; } = string.Empty;

        // Login
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        // Address
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;

        public string ImagePath { get; set; } = string.Empty;

        [NotMapped]
        public IFormFile? TeacherImageFile { get; set; }
    }
}
