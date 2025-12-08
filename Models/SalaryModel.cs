using System;

namespace SIMS_FPT.Models
{
    public class SalaryModel
    {
        public string StaffId { get; set; }
        public string Name { get; set; }
        public string Gender { get; set; }
        public DateTime JoiningDate { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } // Paid / Unpaid
    }
}