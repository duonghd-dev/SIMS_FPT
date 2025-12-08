using System;

namespace SIMS_FPT.Models
{
    public class FeeModel
    {
        public string Id { get; set; } // Mã sinh viên
        public string StudentName { get; set; }
        public string Gender { get; set; }
        public string FeesType { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaidDate { get; set; }
        public string Status { get; set; } // Paid / Unpaid
    }
}