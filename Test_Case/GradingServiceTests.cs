using NUnit.Framework;
using Moq;
using SIMS_FPT.Business.Services;
using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Models;
using SIMS_FPT.Models.ViewModels;
using System.Collections.Generic;

namespace SIMS_FPT.Tests
{
    [TestFixture]
    public class GradingServiceTests
    {
        private Mock<IAssignmentRepository> _assignRepo;
        private Mock<ISubmissionRepository> _subRepo;
        private Mock<IStudentRepository> _studentRepo;
        private Mock<IStudentClassRepository> _studentClassRepo;
        private GradingService _service;

        [SetUp]
        public void Setup()
        {
            _assignRepo = new Mock<IAssignmentRepository>();
            _subRepo = new Mock<ISubmissionRepository>();
            _studentRepo = new Mock<IStudentRepository>();
            _studentClassRepo = new Mock<IStudentClassRepository>();

            _service = new GradingService(
                _assignRepo.Object, _subRepo.Object,
                _studentRepo.Object, _studentClassRepo.Object);
        }

        // TC05: Grade Calculation (Processing/Saving)
        [Test]
        public void TC05_ProcessGrades_UpdatesSubmissionRepository()
        {
            // Arrange
            var model = new BulkGradeViewModel
            {
                AssignmentId = "ASM01",
                IsPublished = true,
                StudentGrades = new List<StudentGradeItem>
                {
                    new StudentGradeItem { StudentId = "ST01", Grade = 8.5, Feedback = "Good job" }
                }
            };
            _assignRepo.Setup(r => r.GetById("ASM01")).Returns(new AssignmentModel());
            _subRepo.Setup(r => r.GetByStudentAndAssignment("ST01", "ASM01"))
                    .Returns(new SubmissionModel { StudentId = "ST01", AssignmentId = "ASM01" });
            // Act
            _service.ProcessGrades(model);
            // Assert
            // Verify SaveSubmission was called with grade 8.5
            _subRepo.Verify(r => r.SaveSubmission(It.Is<SubmissionModel>(
                s => s.StudentId == "ST01" &&
                     s.Grade == 8.5 &&
                     s.TeacherComments == "Good job"
            )), Times.Once);

            _assignRepo.Verify(r => r.Update(It.Is<AssignmentModel>(a => a.AreGradesPublished == true)), Times.Once);
        }
    }
}