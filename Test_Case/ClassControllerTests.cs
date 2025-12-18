using NUnit.Framework;
using Moq;
using Microsoft.AspNetCore.Mvc;
using SIMS_FPT.Areas.Admin.Controllers;
using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Models;
using SIMS_FPT.Services.Interfaces;
using System.Collections.Generic;

namespace SIMS_FPT.Tests
{
    [TestFixture]
    public class ClassControllerTests
    {
        private Mock<IAdminClassService> _classService = null!;
        private Mock<IClassRepository> _classRepo = null!;
        private Mock<ISubjectRepository> _subjectRepo = null!;
        private Mock<ITeacherRepository> _teacherRepo = null!;
        private Mock<IDepartmentRepository> _deptRepo = null!;
        private ClassController _controller = null!;

        [SetUp]
        public void Setup()
        {
            _classService = new Mock<IAdminClassService>();
            _classRepo = new Mock<IClassRepository>();
            _subjectRepo = new Mock<ISubjectRepository>();
            _teacherRepo = new Mock<ITeacherRepository>();
            _deptRepo = new Mock<IDepartmentRepository>();

            _controller = new ClassController(
                _classService.Object,
                _classRepo.Object,
                _subjectRepo.Object,
                _teacherRepo.Object,
                _deptRepo.Object);
        }

        // TC04: Course Enrollment Verification
        [Test]
        public void TC04_AddStudentsToClass_CreatesEnrollmentRecords()
        {
            // Arrange
            string classId = "SE1601";
            var selectedStudents = new List<string> { "ST01", "ST02" };

            // Setup mocks
            _classService.Setup(s => s.AddStudentsToClass(classId, selectedStudents))
                .Returns((true, "Students added successfully"));

            // Act
            var result = _controller.AddStudentsToClass(classId, selectedStudents) as RedirectToActionResult;

            // Assert
            Assert.That(result!, Is.Not.Null);
            Assert.That(result!.ActionName, Is.EqualTo("ManageStudents"));
            _classService.Verify(s => s.AddStudentsToClass(classId, selectedStudents), Times.Once);
        }
    }
}