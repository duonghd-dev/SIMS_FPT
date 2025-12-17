using System.Collections.Generic;

namespace SIMS_FPT.Models.ViewModels
{
    public class InstructorStudentListViewModel
    {
        public List<StudentListDto> Students { get; set; } = new List<StudentListDto>();
        public List<ClassFilterDto> Classes { get; set; } = new List<ClassFilterDto>();
        public string? SelectedClassId { get; set; }
        public string? SearchTerm { get; set; }
    }

    public class StudentListDto
    {
        public string StudentId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string ClassId { get; set; }
        public string ClassName { get; set; }
        public string AvatarUrl { get; set; }
    }

    public class ClassFilterDto
    {
        public string ClassId { get; set; }
        public string ClassName { get; set; }
    }
}