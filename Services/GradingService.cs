// /Services/GradingService.cs
using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Business.Interfaces;
using SIMS_FPT.Models;
using SIMS_FPT.Models.ViewModels;

namespace SIMS_FPT.Business.Services
{
    public class GradingService : IGradingService
    {
        private readonly IAssignmentRepository _assignmentRepo;
        private readonly ISubmissionRepository _submissionRepo;

        // Constructor Injection: The Service asks for the Repositories it needs
        public GradingService(IAssignmentRepository assignmentRepo, ISubmissionRepository submissionRepo)
        {
            _assignmentRepo = assignmentRepo;
            _submissionRepo = submissionRepo;
        }

        public void ProcessBulkGrades(BulkGradeViewModel model)
        {
            // 1. Update Publishing Status
            var assignment = _assignmentRepo.GetById(model.AssignmentId);
            if (assignment != null)
            {
                assignment.AreGradesPublished = model.IsPublished;
                _assignmentRepo.Update(assignment);
            }

            // 2. Process Grades
            foreach (var item in model.StudentGrades)
            {
                var submission = _submissionRepo.GetByStudentAndAssignment(item.StudentId, model.AssignmentId)
                                 ?? new SubmissionModel { StudentId = item.StudentId, AssignmentId = model.AssignmentId };

                submission.Grade = item.Grade;
                submission.TeacherComments = item.Feedback;

                // Add logic here easily later (e.g., Send Email Notification if Grade < 50)

                _submissionRepo.SaveSubmission(submission);
            }
        }
    }
}