using CsvHelper.Configuration.Attributes;
using System;

namespace SIMS_FPT.Models
{
    public class FeeLegacyModel
    {
        [Name("id")]
        public string Id { get; set; }

        [Name("student_name")]
        public string StudentName { get; set; }

        [Name("gender")]
        public string Gender { get; set; }

        [Name("fees_type")]
        public string FeesType { get; set; }

        [Name("amount")]
        public decimal Amount { get; set; }

        [Name("paid_date")]
        public DateTime? PaidDate { get; set; }

        [Name("status")]
        public string Status { get; set; }
    }
}