using SIMS_FPT.Models;
using System.Collections.Generic;

namespace SIMS_FPT.Data.Interfaces
{
    public interface IEnrollmentRepository
    {
        void AddEnrollment(Enrollment enrollment);
        List<Enrollment> GetEnrollmentsByStudentId(int studentId);
        bool IsEnrolled(int studentId, int classId); // Kiểm tra xem đã đăng ký chưa
    }
}