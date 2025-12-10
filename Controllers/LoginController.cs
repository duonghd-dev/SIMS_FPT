using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using SIMS_FPT.Data.Interfaces;
using System;
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

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            // If already logged in, redirect immediately to the correct dashboard
            if (User.Identity?.IsAuthenticated == true)
            {
                var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
                return RedirectToRoleDashboard(roleClaim);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password, bool rememberMe = false, string returnUrl = null)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Please enter email and password!";
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }

            // 1. Validate User
            var user = _userRepo.Login(email, password);
            if (user == null)
            {
                ViewBag.Error = "Invalid email or password!";
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }

            // 2. Create Claims (UserInfo)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim("LinkedId", user.LinkedId ?? "")
            };

            var identity = new ClaimsIdentity(claims, "CookieAuth");
            var principal = new ClaimsPrincipal(identity);

            // 3. Handle "Remember Me" Persistence
            var authProperties = new AuthenticationProperties
            {
                // This keeps the user logged in if the checkbox was checked
                IsPersistent = rememberMe,
                AllowRefresh = true,
                ExpiresUtc = rememberMe ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddMinutes(30)
            };

            await HttpContext.SignInAsync("CookieAuth", principal, authProperties);


            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return RedirectToRoleDashboard(user.Role);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("CookieAuth");
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        // --- CORE REDIRECTION LOGIC ---
        private IActionResult RedirectToRoleDashboard(string? role)
        {
            if (string.IsNullOrEmpty(role)) return RedirectToAction("Login");

            // This switch statement directs the user to the correct Area
            return role switch
            {
                // Goes to /Admin/Home/Dashboard
                "Admin" => RedirectToAction("Dashboard", "Home", new { area = "Admin" }),

                // Goes to /Instructor/Home/Dashboard
                "Instructor" => RedirectToAction("Dashboard", "Home", new { area = "Instructor" }),

                // Goes to /Student/Home/Dashboard
                "Student" => RedirectToAction("Dashboard", "Home", new { area = "Student" }),

                _ => RedirectToAction("Login")
            };
        }
    }
}