using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIMS_FPT.Models
{
    public class TeacherCSVModel
    {
        public string TeacherId { get; set; }
        public string Name { get; set; }
        public string Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Mobile { get; set; }
        public DateTime JoiningDate { get; set; }
        public string Qualification { get; set; }
        public string Experience { get; set; }
        
        // Login Details
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        
        // Address Details
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string Country { get; set; }

        public string ImagePath { get; set; }

        [NotMapped]
        public IFormFile TeacherImageFile { get; set; }
    }
}