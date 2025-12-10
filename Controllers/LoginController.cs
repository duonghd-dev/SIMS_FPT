using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using SIMS_FPT.Data.Interfaces;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SIMS_FPT.Controllers
{
    public class LoginController : Controller
    {
        private readonly IUserRepository _userRepo;

        public LoginController(IUserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        public IActionResult Login()
        {
            // Nếu đã đăng nhập thì chuyển hướng luôn
            if (User.Identity?.IsAuthenticated == true)
            {
                var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
                return RedirectToRoleDashboard(roleClaim);
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Please enter email and password!";
                return View();
            }

            var user = _userRepo.Login(email, password);

            if (user == null)
            {
                ViewBag.Error = "Invalid email or password!";
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                // Quan trọng: Lưu LinkedId để biết tài khoản này gắn với Teacher/Student nào
                new Claim("LinkedId", user.LinkedId ?? "")
            };

            var identity = new ClaimsIdentity(claims, "CookieAuth");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("CookieAuth", principal);

            return RedirectToRoleDashboard(user.Role);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("CookieAuth");
            return RedirectToAction("Login"); // Logout xong quay về trang Login
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        private IActionResult RedirectToRoleDashboard(string? role)
        {
            if (string.IsNullOrEmpty(role)) return RedirectToAction("Login");

            return role switch
            {
                "Admin" => RedirectToAction("Dashboard", "Home", new { area = "Admin" }),
                "Instructor" => RedirectToAction("Dashboard", "Home", new { area = "Instructor" }),
                "Student" => RedirectToAction("Dashboard", "Home", new { area = "Student" }),
                _ => RedirectToAction("Login")
            };
        }
    }
}