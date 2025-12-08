using System;

namespace SIMS_FPT.Models
{
    public class ExpenseModel
    {
        public string Id { get; set; }
        public string ItemName { get; set; }
        public string ItemQuality { get; set; }
        public decimal Amount { get; set; }
        public string PurchaseSource { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string PurchaseBy { get; set; }
    }
}