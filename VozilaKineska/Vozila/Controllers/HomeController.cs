using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vozila.Services.Interfaces;
using Vozila.ViewModels.Models;

namespace Vozila.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUserService _userService;
        private readonly ITansporterService _transporterService;

        public HomeController( IUserService userService, ITansporterService transporterService)
        {
            _userService = userService;
            _transporterService = transporterService;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                // Try Admin/User authentication first (FullName-based)
                var adminUser = await _userService.ValidateUser(model);
                if (adminUser != null)
                {
                    // Store user info in session
                    HttpContext.Session.SetInt32("UserId", adminUser.Id);
                    HttpContext.Session.SetString("UserName", adminUser.FullName);
                    HttpContext.Session.SetString("Role", adminUser.RoleName);
                    HttpContext.Session.SetString("UserType", "Admin");

                    // Role-based redirect
                    if (adminUser.RoleName == "Admin")
                        return RedirectToAction("Index", "Admin");

                    return RedirectToAction("Index", "Home");
                }

                // Try Transporter authentication (Email-based)
                var transporter = await _transporterService.LoginTransporterAsync(model.Email, model.Password);
                if (transporter != null)
                {
                    // Store transporter info in session
                    HttpContext.Session.SetInt32("TransporterId", transporter.Id);
                    HttpContext.Session.SetString("UserName", transporter.CompanyName);
                    HttpContext.Session.SetString("Role", "Transporter");
                    HttpContext.Session.SetString("UserType", "Transporter");

                    return RedirectToAction("Index", "Transporter");
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }
            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Home");
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}
