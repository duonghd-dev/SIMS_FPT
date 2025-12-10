using SIMS_FPT.Models;
using System.Collections.Generic;

namespace SIMS_FPT.Data.Interfaces
{
    public interface ISubmissionRepository
    {
        List<SubmissionModel> GetByAssignmentId(string assignmentId);
        SubmissionModel GetByStudentAndAssignment(string studentId, string assignmentId);
        void SaveSubmission(SubmissionModel model);

        // Fix for CS0535: Ensure this is defined here
        void UpdateGrades(List<SubmissionModel> submissions);
    }
}