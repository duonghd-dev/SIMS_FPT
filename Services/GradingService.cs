using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Business.Interfaces;
using SIMS_FPT.Models;
using SIMS_FPT.Models.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace SIMS_FPT.Business.Services
{
    public class GradingService : IGradingService
    {
        private readonly IAssignmentRepository _assignmentRepo;
        private readonly ISubmissionRepository _submissionRepo;
        private readonly IStudentRepository _studentRepo; // Added dependency

        // Update Constructor to inject StudentRepository
        public GradingService(IAssignmentRepository assignmentRepo,
                              ISubmissionRepository submissionRepo,
                              IStudentRepository studentRepo)
        {
            _assignmentRepo = assignmentRepo;
            _submissionRepo = submissionRepo;
            _studentRepo = studentRepo;
        }

        public void ProcessGrades(BulkGradeViewModel model)
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

                _submissionRepo.SaveSubmission(submission);
            }
        }

        // [NEW] Logic moved from Controller
        public BulkGradeViewModel PrepareGradingView(string assignmentId, string currentTeacherId)
        {
            var assignment = _assignmentRepo.GetById(assignmentId);
            if (assignment == null) return null;

            // Security check passed from controller, or check here
            if (assignment.TeacherId != currentTeacherId) return null;

            var students = _studentRepo.GetBySubject(assignment.SubjectId);
            var submissions = _submissionRepo.GetByAssignmentId(assignmentId);

            var model = new BulkGradeViewModel
            {
                AssignmentId = assignment.AssignmentId,
                AssignmentTitle = assignment.Title,
                MaxPoints = assignment.MaxPoints,
                IsPublished = assignment.AreGradesPublished,
                StudentGrades = new List<StudentGradeItem>()
            };

            foreach (var s in students)
            {
                var sub = submissions.FirstOrDefault(x => x.StudentId == s.StudentId);
                model.StudentGrades.Add(new StudentGradeItem
                {
                    StudentId = s.StudentId,
                    StudentName = s.FullName,
                    HasSubmitted = sub != null && !string.IsNullOrEmpty(sub.FilePath),
                    SubmissionFilePath = sub?.FilePath,
                    Grade = sub?.Grade,
                    Feedback = sub?.TeacherComments
                });
            }

            return model;
        }
    }
}