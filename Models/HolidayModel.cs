using System;

namespace SIMS_FPT.Models
{
    public class HolidayModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; } // Public Holiday, College Holiday...
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}