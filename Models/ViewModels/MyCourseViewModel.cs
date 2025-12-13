using System.Collections.Generic;
using SIMS_FPT.Models;

namespace SIMS_FPT.Models.ViewModels
{
    public class MyCourseViewModel
    {
        public ClassModel ClassInfo { get; set; }
        public List<CourseMaterialModel> Materials { get; set; }
    }
}
