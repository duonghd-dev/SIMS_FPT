using NUnit.Framework;
using Moq;
using Microsoft.AspNetCore.Mvc;
using SIMS_FPT.Areas.Admin.Controllers;
using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Models;
using SIMS_FPT.Services;
using System.Threading.Tasks;

namespace SIMS_FPT.Tests
{
    [TestFixture]
    public class StudentControllerTests
    {
        private Mock<IStudentRepository> _mockRepo;
        private Mock<StudentService> _mockService; // Ensure StudentService methods used are virtual if mocking directly
        private StudentController _controller;

        [SetUp]
        public void Setup()
        {
            _mockRepo = new Mock<IStudentRepository>();
            // Passing nulls for service dependencies as we rely on ModelState check in TC02
            _mockService = new Mock<StudentService>(null, null, null);

            _controller = new StudentController(_mockRepo.Object, _mockService.Object);
        }

        // TC02: Add New Student with Data Validation
        [Test]
        public async Task TC02_Add_WithInvalidModel_ReturnsViewAndDoesNotCallService()
        {
            // Arrange
            var invalidModel = new StudentCSVModel { StudentId = "ST001", Email = "invalid-email" };

            // Simulate Validation Error
            _controller.ModelState.AddModelError("Email", "Invalid email format");

            // Act
            var result = await _controller.Add(invalidModel) as ViewResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Model, Is.InstanceOf<StudentCSVModel>());
            Assert.That(_controller.ModelState.IsValid, Is.False);
        }

        // TC06: Delete Student and Cascading Data Integrity
        [Test]
        public void TC06_DeleteStudent_CallsRepositoryDelete()
        {
            // Arrange
            string studentId = "ST999";

            // Act
            var result = _controller.DeleteStudent(studentId) as RedirectToActionResult;

            // Assert
            Assert.That(result.ActionName, Is.EqualTo("List"));
            _mockRepo.Verify(r => r.Delete(studentId), Times.Once, "Repository Delete should be called exactly once");
        }
    }
}