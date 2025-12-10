using CsvHelper.Configuration.Attributes;
using System;

namespace SIMS_FPT.Models
{
    public class FeeModel
    {
        [Name("payment_id")]
        public string PaymentId { get; set; }

        [Name("student_id")]
        public string StudentId { get; set; }

        [Name("fee_type_id")]
        public string FeeTypeId { get; set; }

        [Name("paid_date")]
        public DateTime? PaidDate { get; set; }

        [Name("status")]
        public string Status { get; set; }
    }
}