using SIMS_FPT.Models;
using System.Collections.Generic;

namespace SIMS_FPT.Data.Interfaces
{
    public interface IAssignmentRepository
    {
        List<AssignmentModel> GetAll();
        List<AssignmentModel> GetBySubject(string subjectId);
        AssignmentModel GetById(string id);
        void Add(AssignmentModel model);
        void Update(AssignmentModel model);
        void Delete(string id);
    }

    public interface ISubmissionRepository
    {
        List<SubmissionModel> GetByAssignmentId(string assignmentId);
        SubmissionModel GetByStudentAndAssignment(string studentId, string assignmentId);
        void SaveSubmission(SubmissionModel model); // Handles Add or Update logic
        void UpdateGrades(List<SubmissionModel> submissions); // For Bulk Entry
    }
}