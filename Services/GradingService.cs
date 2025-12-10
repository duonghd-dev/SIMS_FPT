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

        public GradingService(IAssignmentRepository assignmentRepo, ISubmissionRepository submissionRepo)
        {
            _assignmentRepo = assignmentRepo;
            _submissionRepo = submissionRepo;
        }

        public void ProcessGrades(BulkGradeViewModel model)
        {
            // 1. Update Publishing Status (if instructor toggled "Publish Grades")
            var assignment = _assignmentRepo.GetById(model.AssignmentId);
            if (assignment != null)
            {
                assignment.AreGradesPublished = model.IsPublished;
                _assignmentRepo.Update(assignment);
            }

            // 2. Process Grades for each student
            foreach (var item in model.StudentGrades)
            {
                // Find existing submission or create a blank one (for grading without file)
                var submission = _submissionRepo.GetByStudentAndAssignment(item.StudentId, model.AssignmentId)
                                 ?? new SubmissionModel { StudentId = item.StudentId, AssignmentId = model.AssignmentId };

                submission.Grade = item.Grade;
                submission.TeacherComments = item.Feedback;

                _submissionRepo.SaveSubmission(submission);
            }
        }
    }
}