using CsvHelper.Configuration.Attributes;
using System;
using System.ComponentModel.DataAnnotations; // Thêm thư viện này để hiển thị tên đẹp hơn

namespace SIMS_FPT.Models
{
    public class ExpenseModel
    {
        [Name("expense_id")]
        public string ExpenseId { get; set; }

        [Name("item_name")]
        [Display(Name = "Item Name")]
        public string ItemName { get; set; }

        [Name("quantity")]
        public int Quantity { get; set; }

        [Name("unit_price")]
        [Display(Name = "Unit Price")]
        public decimal UnitPrice { get; set; }

        [Name("total_amount")]
        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }

        [Name("source")]
        public string Source { get; set; }

        [Name("purchase_date")]
        [Display(Name = "Date")]
        public DateTime PurchaseDate { get; set; }

        [Name("purchased_by")]
        [Display(Name = "Purchased By")]
        public string PurchasedBy { get; set; }
    }
}