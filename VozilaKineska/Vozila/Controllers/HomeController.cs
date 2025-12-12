using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Vozila.Services.Interfaces;
using Vozila.ViewModels.Models;
using Vozila.ViewModels.ModelsTransporter;

namespace Vozila.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUserService _userService;
        private readonly ITransporterService _transporterService;
        private readonly ILogger<HomeController> _logger;

        public HomeController( IUserService userService, ITransporterService transporterService, ILogger<HomeController> logger)
        {
            _userService = userService;
            _transporterService = transporterService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login(string userType = null)
        {
            var model = new LoginViewModel();
            ViewData["UserType"] = userType ?? "company"; // default to company login
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [ActionName("Login")]
        public async Task<IActionResult> LoginPost(LoginViewModel model, string userType = "company", string returnUrl = null)
        {
            ViewData["UserType"] = userType;
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (userType == "transporter")
            {
                // Handle transporter login
                var transporter = await _transporterService.LoginTransporterAsync(model.Email, model.Password);
                if (transporter != null)
                {
                    await SignInTransporterAsync(transporter);
                    _logger.LogInformation("Transporter {Email} logged in.", model.Email);
                    return RedirectToAction("Dashboard", "Transporter");
                }

                ModelState.AddModelError(string.Empty, "Invalid transporter login attempt.");
                return View(model);
            }
            else
            {
                // Handle company/admin login
                var user = await _userService.ValidateUser(model);
                if (user != null)
                {
                    await SignInUserAsync(user);
                    _logger.LogInformation("User {FullName} logged in.", user.FullName);

                    // Role-based redirect
                    if (user.RoleName == "Admin")
                    {
                        return RedirectToAction("Index", "Admin");
                    }

                    return RedirectToLocal(returnUrl);
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginTransporter(LoginViewModel model, string returnUrl = null)
        {
            // Dedicated transporter login endpoint
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Login", new { userType = "transporter" });
            }

            var transporter = await _transporterService.LoginTransporterAsync(model.Email, model.Password);
            if (transporter == null)
            {
                TempData["ErrorMessage"] = "Invalid transporter login attempt.";
                return RedirectToAction("Login", new { userType = "transporter" });
            }

            await SignInTransporterAsync(transporter);
            _logger.LogInformation("Transporter {Email} logged in via dedicated endpoint.", model.Email);

            return RedirectToAction("Dashboard", "Transporter");
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginCompany(LoginViewModel model, string returnUrl = null)
        {
            // Dedicated company login endpoint
            if (!ModelState.IsValid)
            {
                return View("Login", model);
            }

            var user = await _userService.ValidateUser(model);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View("Login", model);
            }

            await SignInUserAsync(user);
            _logger.LogInformation("Company user {FullName} logged in.", user.FullName);

            if (user.RoleName == "Admin")
            {
                return RedirectToAction("Index", "Admin");
            }

            return RedirectToLocal(returnUrl);
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            _logger.LogInformation("User logged out.");
            return RedirectToAction("Index", "Home");
        }
        private async Task SignInTransporterAsync(TransporterVM transporter)
        {
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, transporter.Id.ToString()),
            new Claim(ClaimTypes.Name, transporter.CompanyName),
            new Claim(ClaimTypes.Email, transporter.Email),
            new Claim(ClaimTypes.Role, "Transporter"),
            new Claim("UserType", "Transporter"),
            new Claim("CompanyName", transporter.CompanyName)
        };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            // Store in session for backward compatibility
            HttpContext.Session.SetInt32("TransporterId", transporter.Id);
            HttpContext.Session.SetString("UserName", transporter.CompanyName);
            HttpContext.Session.SetString("Role", "Transporter");
            HttpContext.Session.SetString("UserType", "Transporter");
        }

        private async Task SignInUserAsync(UserVM user)
        {
                    var claims = new List<Claim>
            {
                new Claim(type: ClaimTypes.NameIdentifier, value: user.Id.ToString()),
                new Claim(type: ClaimTypes.Name, value: user.FullName),
                new Claim(type: ClaimTypes.Email, value: user.Email),
                new Claim(type: ClaimTypes.Role, value: user.RoleName),
                new Claim(type: "UserType", value: "Company"),
                new Claim(type: "UserId", value: user.Id.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            // Store in session for backward compatibility
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserName", user.FullName);
            HttpContext.Session.SetString("Role", user.RoleName);
            HttpContext.Session.SetString("UserType", "Company");
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Transporter"))
                {
                    return RedirectToAction("Dashboard", "Transporter");
                }
                else if (User.IsInRole("Admin"))
                {
                    return RedirectToAction("Index", "Admin");
                }
            }
            return View();
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
