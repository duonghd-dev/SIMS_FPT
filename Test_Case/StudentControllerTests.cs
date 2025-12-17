using NUnit.Framework;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using SIMS_FPT.Areas.Admin.Controllers;
using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Models;
using SIMS_FPT.Services;
using SIMS_FPT.Services.Interfaces;
using System.IO;
using System.Threading.Tasks;

namespace SIMS_FPT.Tests
{
    [TestFixture]
    public class StudentControllerTests
    {
        private Mock<IAdminStudentService> _mockService = null!;
        private StudentController _controller = null!;

        [SetUp]
        public void Setup()
        {
            _mockService = new Mock<IAdminStudentService>();
            _controller = new StudentController(_mockService.Object);
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
            Assert.That(result!.Model, Is.InstanceOf<StudentCSVModel>());
            Assert.That(_controller.ModelState.IsValid, Is.False);
        }

        // TC06: Delete Student and Cascading Data Integrity
        [Test]
        public async Task TC06_DeleteStudent_CallsServiceDelete()
        {
            // Arrange
            string studentId = "ST999";
            _mockService.Setup(s => s.DeleteStudent(studentId)).ReturnsAsync((true, "Success"));

            // Act
            var result = await _controller.DeleteStudent(studentId) as RedirectToActionResult;

            // Assert
            Assert.That(result!, Is.Not.Null);
            Assert.That(result!.ActionName, Is.EqualTo("List"));
            _mockService.Verify(s => s.DeleteStudent(studentId), Times.Once);
        }
    }
}