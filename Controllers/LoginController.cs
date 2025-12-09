using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using SIMS_Project.Interface;
using SIMS_Project.Models;
using System.Collections.Generic;
using System.Security.Claims;

namespace SIMS_Project.Controllers
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
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            var user = _userRepo.Login(username, password);

            if (user != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.FullName),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                };

                var identity = new ClaimsIdentity(claims, "CookieAuth");
                var principal = new ClaimsPrincipal(identity);

                HttpContext.SignInAsync("CookieAuth", principal);

                if (user.Role == "Admin")
                    return RedirectToAction("Dashboard", "Home", new { area = "Admin" });

                if (user.Role == "Instructor")
                    return RedirectToAction("Dashboard", "Home", new { area = "Instructor" });

                if (user.Role == "Student")
                    return RedirectToAction("Dashboard", "Home", new { area = "Student" });

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Sai tên đăng nhập hoặc mật khẩu!";
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(Login user)
        {
            if (ModelState.IsValid)
            {
                if (_userRepo.UsernameExists(user.Username))
                {
                    ViewBag.Error = "Username already exists!";
                    return View(user);
                }

                _userRepo.AddUser(user);
                return RedirectToAction("Login");
            }
            return View(user);
        }

        public IActionResult Logout()
        {
            HttpContext.SignOutAsync("CookieAuth");
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied() => View();
    }
}
