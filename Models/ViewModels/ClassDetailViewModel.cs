// File: SIMS_FPT/Models/ViewModels/ClassDetailViewModel.cs
namespace SIMS_FPT.Models.ViewModels
{
    public class ClassDetailViewModel
    {
        public ClassModel? Class { get; set; }
        public List<ClassSubjectViewModel> SubjectTeachers { get; set; } = new List<ClassSubjectViewModel>();
    }
}
