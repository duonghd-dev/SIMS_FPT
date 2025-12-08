using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SIMS_FPT.Models
{
    public class DepartmentModel
    {
        public string DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public string HeadOfDepartment { get; set; }
        public DateTime StartDate { get; set; }
        public int NoOfStudents { get; set; }
    }
}