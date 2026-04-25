using Microsoft.AspNetCore.Mvc;
using PerfumeStore.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using PerfumeStore.Web.Data.Repositories;
using Microsoft.AspNetCore.Authorization;
using PerfumeStore.Web.Infrastructure;

namespace PerfumeStore.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly ICommerceRepository _commerceRepository;

        public AccountController(ICommerceRepository commerceRepository)
        {
            _commerceRepository = commerceRepository;
        }

        [HttpGet]
        public IActionResult Login(string type = "customer")
        {
            var model = new LoginViewModel
            {
                LoginType = string.Equals(type, "admin", StringComparison.OrdinalIgnoreCase) ? "admin" : "customer"
            };

            // Route to different login views based on type (admin or customer)
            if (model.LoginType == "admin")
            {
                return View("Login", model);
            }
            return View("CustomerLogin", model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, CancellationToken cancellationToken)
        {
            if (ModelState.IsValid)
            {
                var normalizedUsername = NormalizeUsername(model.Username);
                if (string.IsNullOrWhiteSpace(normalizedUsername) || string.IsNullOrWhiteSpace(model.Password))
                {
                    ModelState.AddModelError(string.Empty, "Username and password are required.");
                    return View(model.LoginType == "admin" ? "Login" : "CustomerLogin", model);
                }

                var user = await _commerceRepository.GetUserByUsernameAsync(normalizedUsername, cancellationToken);

                if (user != null &&
                    !string.IsNullOrWhiteSpace(user.Username) &&
                    !string.IsNullOrWhiteSpace(user.Role) &&
                    !string.IsNullOrWhiteSpace(user.PasswordHash) &&
                    BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Username),
                        new Claim(ClaimTypes.Role, user.Role),
                        new Claim(ClaimTypes.Email, user.Username),
                        new Claim(ClaimTypes.GivenName, user.FullName ?? user.Username),
                    };

                    var claimsIdentity = new ClaimsIdentity(
                        claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties();

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme, 
                        new ClaimsPrincipal(claimsIdentity), 
                        authProperties);

                    if (AppRoles.StaffRoles.Contains(user.Role))
                    {
                        return RedirectToAction("Index", "Admin");
                    }
                    else
                    {
                        return RedirectToAction("Index", "Customer");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(model.LoginType == "admin" ? "Login" : "CustomerLogin", model);
                }
            }
            return View(model.LoginType == "admin" ? "Login" : "CustomerLogin", model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var normalizedEmail = NormalizeUsername(model.Email);
            if (string.IsNullOrWhiteSpace(normalizedEmail))
            {
                ModelState.AddModelError(nameof(model.Email), "A valid email address is required.");
                return View(model);
            }

            model.Email = normalizedEmail;

            var existingUser = await _commerceRepository.GetUserByUsernameAsync(normalizedEmail, cancellationToken);
            if (existingUser is not null)
            {
                ModelState.AddModelError(nameof(model.Email), "An account with this email already exists.");
                return View(model);
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);
            await _commerceRepository.CreateUserAsync(model.FullName.Trim(), normalizedEmail, passwordHash, cancellationToken);

            TempData["AuthMessage"] = "Your account has been created. Please sign in to continue.";
            return RedirectToAction(nameof(Login), new { type = "customer" });
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Landing");
        }

        private static string NormalizeUsername(string? username) =>
            string.IsNullOrWhiteSpace(username)
                ? string.Empty
                : username.Trim().ToLowerInvariant();
    }
}
