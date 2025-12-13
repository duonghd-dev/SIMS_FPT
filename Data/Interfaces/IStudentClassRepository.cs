using SIMS_FPT.Models;
using System.Collections.Generic;

namespace SIMS_FPT.Data.Interfaces
{
    public interface IStudentClassRepository
    {
        void Add(StudentClassModel model);
        void Remove(string classId, string studentId); // Xóa sinh viên khỏi lớp
        List<StudentClassModel> GetByClassId(string classId); // Lấy danh sách ID sinh viên trong lớp
        List<StudentClassModel> GetByStudentId(string studentId); // Lấy danh sách lớp sinh viên đang học
        bool IsEnrolled(string classId, string studentId); // Kiểm tra đã tồn tại chưa
    // THÊM DÒNG NÀY:
        void DeleteByClassAndStudent(string classId, string studentId);
    }
}