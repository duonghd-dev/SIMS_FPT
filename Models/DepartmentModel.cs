using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SIMS_FPT.Models
{
    public class DepartmentModel
    {
        [Name("department_id")]
        [Required(ErrorMessage = "Department ID is required.")]
        public string? DepartmentId { get; set; }

        [Name("department_name")]
        [Required(ErrorMessage = "Department Name is required.")]
        public string? DepartmentName { get; set; }

        [Name("head_of_department")]
        [Required(ErrorMessage = "Head of Department is required.")]
        public string? HeadOfDepartment { get; set; }

        [Name("start_date")]
        public DateTime? StartDate { get; set; }

        [Name("no_of_students")]
        [Range(0, int.MaxValue, ErrorMessage = "Number of students cannot be negative.")]
        public int NoOfStudents { get; set; }
    }

    // ViewModel dùng cho trang Detail
    public class DepartmentDetailViewModel
    {
        public DepartmentModel Department { get; set; }
        public List<TeacherCSVModel> Teachers { get; set; }

        // --- BỔ SUNG THUỘC TÍNH NÀY ĐỂ SỬA LỖI ---
        public List<SubjectModel> Subjects { get; set; }
    }
}