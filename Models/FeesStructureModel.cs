using System;

namespace SIMS_FPT.Models
{
    public class FeesStructureModel
    {
        public string Id { get; set; }
        public string FeesName { get; set; }
        public string Class { get; set; } 
        public decimal Amount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}