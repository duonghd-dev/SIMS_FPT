using NUnit.Framework;
using Moq;
using Microsoft.AspNetCore.Mvc;
using SIMS_FPT.Areas.Admin.Controllers;
using SIMS_FPT.Data.Interfaces;

namespace SIMS_FPT.Tests
{
    [TestFixture]
    public class SubjectControllerTests
    {
        private Mock<ISubjectRepository> _mockRepo = null!;
        private Mock<IDepartmentRepository> _mockDeptRepo = null!;
        private Mock<ITeacherRepository> _mockTeacherRepo = null!;
        private SubjectController _controller = null!;

        [SetUp]
        public void Setup()
        {
            _mockRepo = new Mock<ISubjectRepository>();
            _mockDeptRepo = new Mock<IDepartmentRepository>();
            _mockTeacherRepo = new Mock<ITeacherRepository>();
            _controller = new SubjectController(_mockRepo.Object, _mockDeptRepo.Object, _mockTeacherRepo.Object);
        }

        // TC03: Delete Course by ID
        [Test]
        public void TC03_DeleteSubject_CallsRepositoryDelete()
        {
            // Arrange
            string subjectId = "SUB001";

            // Act
            var result = _controller.Delete(subjectId) as RedirectToActionResult;

            // Assert
            Assert.That(result!, Is.Not.Null);
            Assert.That(result!.ActionName, Is.EqualTo("List"));
            _mockRepo.Verify(r => r.Delete(subjectId), Times.Once);
        }
    }
}