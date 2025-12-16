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
        private readonly IStudentRepository _studentRepo;
        private readonly IStudentClassRepository _studentClassRepo; // [1] Add this

        // [2] Update Constructor
        public GradingService(IAssignmentRepository assignmentRepo,
                              ISubmissionRepository submissionRepo,
                              IStudentRepository studentRepo,
                              IStudentClassRepository studentClassRepo)
        {
            _assignmentRepo = assignmentRepo;
            _submissionRepo = submissionRepo;
            _studentRepo = studentRepo;
            _studentClassRepo = studentClassRepo;
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

        public BulkGradeViewModel? PrepareGradingView(string assignmentId, string currentTeacherId)
        {
            var assignment = _assignmentRepo.GetById(assignmentId);
            if (assignment == null) return null;

            if (assignment.TeacherId != currentTeacherId) return null;

            // [3] FIX: Get students from the specific CLASS, not the whole subject
            // Old incorrect line: var students = _studentRepo.GetBySubject(assignment.SubjectId);

            var enrollments = _studentClassRepo.GetByClassId(assignment.ClassId);
            var submissions = _submissionRepo.GetByAssignmentId(assignmentId);

            var model = new BulkGradeViewModel
            {
                AssignmentId = assignment.AssignmentId,
                AssignmentTitle = assignment.Title,
                MaxPoints = assignment.MaxPoints,
                IsPublished = assignment.AreGradesPublished,
                StudentGrades = new List<StudentGradeItem>()
            };

            // [4] Iterate through the enrolled students
            foreach (var enrollment in enrollments)
            {
                var studentInfo = _studentRepo.GetById(enrollment.StudentId);
                if (studentInfo == null) continue; // Skip if student not found in main DB

                var sub = submissions.FirstOrDefault(x => x.StudentId == enrollment.StudentId);

                model.StudentGrades.Add(new StudentGradeItem
                {
                    StudentId = studentInfo.StudentId,
                    StudentName = studentInfo.FullName,
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