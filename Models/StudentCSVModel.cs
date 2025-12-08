using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace SIMS_FPT.Models
{
    public class StudentCSVModel
    {
        public string StudentId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}";
        public string ClassName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string Religion { get; set; }
        public DateTime JoiningDate { get; set; }
        public string MobileNumber { get; set; }
        public string AdmissionNumber { get; set; }
        public string Section { get; set; }
        
        public string ImagePath { get; set; }
        
        [NotMapped]
        public IFormFile StudentImageFile { get; set; }

        // Thông tin Cha
        public string FatherName { get; set; }
        public string FatherOccupation { get; set; }
        public string FatherMobile { get; set; }
        public string FatherEmail { get; set; }

        // Thông tin Mẹ
        public string MotherName { get; set; }
        public string MotherOccupation { get; set; }
        public string MotherMobile { get; set; }
        public string MotherEmail { get; set; }

        public string Address { get; set; } 
        public string PermanentAddress { get; set; } 
    }
}