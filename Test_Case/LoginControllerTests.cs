using NUnit.Framework;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using SIMS_FPT.Controllers;
using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using System.Threading.Tasks;
using System;
namespace SIMS_FPT.Tests
{
    [TestFixture]
    public class LoginControllerTests
    {
        private Mock<IUserRepository> _mockUserRepo = null!;
        private Mock<IStudentRepository> _mockStudentRepo = null!;
        private Mock<ITeacherRepository> _mockTeacherRepo = null!;
        private LoginController _controller = null!;
        [SetUp]
        public void Setup()
        {
            // 1. Mock Repository
            _mockUserRepo = new Mock<IUserRepository>();
            _mockStudentRepo = new Mock<IStudentRepository>();
            _mockTeacherRepo = new Mock<ITeacherRepository>();
            _controller = new LoginController(_mockUserRepo.Object, _mockStudentRepo.Object, _mockTeacherRepo.Object);

            // 2. Mock HttpContext and ServiceProvider
            var mockHttpContext = new Mock<HttpContext>();
            var serviceProvider = new Mock<IServiceProvider>();

            // --- A. Mock AuthenticationService (Required for Login/Logout) ---
            var authService = new Mock<IAuthenticationService>();
            serviceProvider.Setup(s => s.GetService(typeof(IAuthenticationService)))
                           .Returns(authService.Object);

            // --- B. Mock ITempDataDictionaryFactory (Required for Views/TempData) ---
            var tempDataFactory = new Mock<ITempDataDictionaryFactory>();
            var tempData = new Mock<ITempDataDictionary>();
            tempDataFactory.Setup(f => f.GetTempData(It.IsAny<HttpContext>()))
                           .Returns(tempData.Object);
            serviceProvider.Setup(s => s.GetService(typeof(ITempDataDictionaryFactory)))
                           .Returns(tempDataFactory.Object);

            // --- C. Mock IUrlHelperFactory (Required for Redirects) ---
            var urlHelperFactory = new Mock<IUrlHelperFactory>();
            var urlHelper = new Mock<IUrlHelper>();
            urlHelperFactory.Setup(f => f.GetUrlHelper(It.IsAny<ActionContext>()))
                            .Returns(urlHelper.Object);
            serviceProvider.Setup(s => s.GetService(typeof(IUrlHelperFactory)))
                           .Returns(urlHelperFactory.Object);
            // 3. Connect ServiceProvider to HttpContext
            mockHttpContext.Setup(x => x.RequestServices).Returns(serviceProvider.Object);
            // 4. Assign Context to Controller
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = mockHttpContext.Object
            };
            // 5. Explicitly assign properties to avoid lazy-loading issues in some base methods
            _controller.TempData = tempData.Object;
            _controller.Url = urlHelper.Object;
        }
        // TC01: Verify Account Authentication (Invalid Login)
        [Test]
        public async Task TC01_Login_WithInvalidCredentials_ReturnsViewWithError()
        {
            // Arrange
            string email = "wrong@test.com";
            string password = "wrong";
            _mockUserRepo.Setup(r => r.Login(email, password)).Returns((Users)null);
            var result = await _controller.Login(email, password) as ViewResult;
            // Assert
            Assert.That(result, Is.Not.Null, "Result should be a ViewResult");
            Assert.That(result!.ViewData["Error"], Is.EqualTo("Invalid email or password!"));
        }
        // TC07: User Role Authorization Check (Redirect Logic)
        [Test]
        public async Task TC07_Login_AsStudent_RedirectsToStudentDashboard()
        {
            // Arrange
            string email = "student@test.com";
            string password = "123";
            // Important: HashAlgorithm must match logic in Repo or be handled. 
            // Since we mocked Repo.Login to return this USER directly, the internal password check in Repo is bypassed.
            var studentUser = new Users { Email = email, Role = "Student", FullName = "Test Student" };
            _mockUserRepo.Setup(r => r.Login(email, password)).Returns(studentUser);
            // Act
            var result = await _controller.Login(email, password) as RedirectToActionResult;
            // Assert
            Assert.That(result, Is.Not.Null, "Result should be a RedirectToActionResult");
            Assert.That(result!.ActionName, Is.EqualTo("Dashboard"));
            Assert.That(result.ControllerName, Is.EqualTo("Home"));
            Assert.That(result.RouteValues!["area"], Is.EqualTo("Student"), "Student should be redirected to Student Area");
        }
    }
}