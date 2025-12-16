using System.Collections.Generic;
using SIMS_FPT.Models;

namespace SIMS_FPT.Models.ViewModels
{
    public class MyCourseViewModel
    {
        public ClassModel ClassInfo { get; set; } = new ClassModel();
        public List<CourseMaterialModel> Materials { get; set; } = new List<CourseMaterialModel>();
    }
}
