using System.Collections.Generic;

namespace SIMS_FPT.Models.ViewModels
{
    public class ClassEnrollmentViewModel
    {
        // Thông tin lớp học hiện tại
        public ClassModel ClassInfo { get; set; }

        // Danh sách sinh viên ĐÃ vào lớp này
        public List<StudentCSVModel> EnrolledStudents { get; set; } = new List<StudentCSVModel>();

        // Danh sách sinh viên CHƯA vào lớp này (để Admin tích chọn thêm vào)
        public List<StudentCSVModel> AvailableStudents { get; set; } = new List<StudentCSVModel>();
    }
}