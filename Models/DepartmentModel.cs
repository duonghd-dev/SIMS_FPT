using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;

namespace SIMS_FPT.Models
{
    public class DepartmentModel
    {
        [Name("department_id")]
        public string DepartmentId { get; set; }

        [Name("department_name")]
        public string DepartmentName { get; set; }

        [Name("head_of_department")]
        public string HeadOfDepartment { get; set; }

        [Name("start_date")]
        public DateTime? StartDate { get; set; }

        [Name("no_of_students")]
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