using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Models;
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

        // =====================================================
        // GET: Login
        // =====================================================
        public IActionResult Login()
        {
            // Nếu user đã đăng nhập -> tự động redirect theo role
            if (User.Identity?.IsAuthenticated == true)
            {
                var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
                return RedirectToRoleDashboard(roleClaim);
            }

            return View();
        }

        // =====================================================
        // POST: Login
        // =====================================================
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Please enter username and password!";
                return View();
            }

            var user = _userRepo.Login(username, password);

            if (user == null)
            {
                ViewBag.Error = "Invalid username or password!";
                return View();
            }

            // ====== Setup Claims ======
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim("Username", user.Username)
            };

            var identity = new ClaimsIdentity(claims, "CookieAuth");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("CookieAuth", principal);

            // → QUAN TRỌNG: dùng role của user vừa đăng nhập
            return RedirectToRoleDashboard(user.Role);
        }

        // =====================================================
        // GET: Register
        // =====================================================
        public IActionResult Register()
        {
            return View();
        }

        // =====================================================
        // POST: Register
        // =====================================================
        [HttpPost]
        public IActionResult Register(Users user)
        {
            if (!ModelState.IsValid)
                return View(user);

            if (_userRepo.UsernameExists(user.Username))
            {
                ViewBag.Error = "Username already exists!";
                return View(user);
            }

            if (string.IsNullOrEmpty(user.Role))
                user.Role = "Student";

            _userRepo.AddUser(user);

            return RedirectToAction("Login");
        }

        // =====================================================
        // Logout
        // =====================================================
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("CookieAuth");
            return RedirectToAction("Index", "Home");
        }

        // =====================================================
        // Access Denied
        // =====================================================
        public IActionResult AccessDenied()
        {
            return View();
        }

        // =====================================================
        // Helper: Redirect user theo Role 
        // =====================================================
        private IActionResult RedirectToRoleDashboard(string? role)
        {
            if (string.IsNullOrEmpty(role))
                return RedirectToAction("Index", "Home");

            return role switch
            {
                "Admin" => RedirectToAction("Dashboard", "Home", new { area = "Admin" }),
                "Instructor" => RedirectToAction("Dashboard", "Home", new { area = "Instructor" }),
                "Student" => RedirectToAction("Dashboard", "Home", new { area = "Student" }),
                _ => RedirectToAction("Index", "Home")
            };
        }

    }
}
