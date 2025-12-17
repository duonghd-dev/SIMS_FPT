using NUnit.Framework;
using Moq;
using Microsoft.AspNetCore.Mvc;
using SIMS_FPT.Areas.Admin.Controllers;
using SIMS_FPT.Services.Interfaces;

namespace SIMS_FPT.Tests
{
    [TestFixture]
    public class SubjectControllerTests
    {
        private Mock<IAdminSubjectService> _mockService = null!;
        private SubjectController _controller = null!;

        [SetUp]
        public void Setup()
        {
            _mockService = new Mock<IAdminSubjectService>();
            _controller = new SubjectController(_mockService.Object, null!, null!);
        }

        // TC03: Delete Subject by ID
        [Test]
        public void TC03_DeleteSubject_CallsServiceDelete()
        {
            // Arrange
            string subjectId = "SUB001";
            _mockService.Setup(s => s.DeleteSubject(subjectId)).Returns((true, "Deleted"));

            // Act
            var result = _controller.Delete(subjectId) as RedirectToActionResult;

            // Assert
            Assert.That(result!, Is.Not.Null);
            Assert.That(result!.ActionName, Is.EqualTo("List"));
            _mockService.Verify(s => s.DeleteSubject(subjectId), Times.Once);
        }
    }
}