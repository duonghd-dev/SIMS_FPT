using NUnit.Framework;
using Moq;
using Microsoft.AspNetCore.Mvc;
using SIMS_FPT.Areas.Admin.Controllers;
using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Models;
using System.Collections.Generic;

namespace SIMS_FPT.Tests
{
    [TestFixture]
    public class ClassControllerTests
    {
        private Mock<IClassRepository> _classRepo = null!;
        private Mock<ISubjectRepository> _subjectRepo = null!;
        private Mock<ITeacherRepository> _teacherRepo = null!;
        private Mock<IStudentRepository> _studentRepo = null!;
        private Mock<IStudentClassRepository> _studentClassRepo = null!;
        private Mock<IDepartmentRepository> _deptRepo = null!;
        private Mock<IClassSubjectRepository> _classSubjectRepo = null!;
        private ClassController _controller = null!;

        [SetUp]
        public void Setup()
        {
            _classRepo = new Mock<IClassRepository>();
            _subjectRepo = new Mock<ISubjectRepository>();
            _teacherRepo = new Mock<ITeacherRepository>();
            _studentRepo = new Mock<IStudentRepository>();
            _studentClassRepo = new Mock<IStudentClassRepository>();
            _deptRepo = new Mock<IDepartmentRepository>();
            _classSubjectRepo = new Mock<IClassSubjectRepository>();

            _controller = new ClassController(
                _classRepo.Object,
                _subjectRepo.Object,
                _teacherRepo.Object,
                _deptRepo.Object,
                _studentRepo.Object,
                _studentClassRepo.Object,
                _classSubjectRepo.Object);
        }

        // TC04: Course Enrollment Verification
        [Test]
        public void TC04_AddStudentsToClass_CreatesEnrollmentRecords()
        {
            // Arrange
            string classId = "SE1601";
            var selectedStudents = new List<string> { "ST01", "ST02" };

            // Setup mocks
            _studentClassRepo.Setup(r => r.IsEnrolled(classId, It.IsAny<string>())).Returns(false);
            _classRepo.Setup(r => r.GetById(classId)).Returns(new ClassModel { ClassId = classId });
            _studentClassRepo.Setup(r => r.GetByClassId(classId)).Returns(new List<StudentClassModel>());

            // Act
            var result = _controller.AddStudentsToClass(classId, selectedStudents) as RedirectToActionResult;

            // Assert
            Assert.That(result!, Is.Not.Null);
            Assert.That(result!.ActionName, Is.EqualTo("ManageStudents"));

            _studentClassRepo.Verify(r => r.Add(It.Is<StudentClassModel>(s => s.ClassId == classId && s.StudentId == "ST01")), Times.Once);
            _studentClassRepo.Verify(r => r.Add(It.Is<StudentClassModel>(s => s.ClassId == classId && s.StudentId == "ST02")), Times.Once);
        }
    }
}