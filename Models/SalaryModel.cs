using CsvHelper.Configuration.Attributes;
using System;

namespace SIMS_FPT.Models
{
    public class SalaryModel
    {
        [Name("salary_id")]
        public string SalaryId { get; set; }

        [Name("teacher_id")]
        public string TeacherId { get; set; }

        [Name("month")]
        public int Month { get; set; }

        [Name("year")]
        public int Year { get; set; }

        [Name("basic_salary")]
        public decimal BasicSalary { get; set; }

        [Name("allowance")]
        public decimal Allowance { get; set; }

        [Name("bonus")]
        public decimal Bonus { get; set; }

        [Name("deduction")]
        public decimal Deduction { get; set; }

        [Name("net_salary")]
        public decimal NetSalary { get; set; }

        [Name("payment_date")]
        public DateTime? PaymentDate { get; set; }

        [Name("status")]
        public string Status { get; set; }
    }
}