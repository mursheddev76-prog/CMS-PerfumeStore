using Microsoft.AspNetCore.Mvc;
using PerfumeStore.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using PerfumeStore.Web.Data.Repositories;

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

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, CancellationToken cancellationToken)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrWhiteSpace(model.Password))
                {
                    ModelState.AddModelError(string.Empty, "Username and password are required.");
                    return View(model.LoginType == "admin" ? "Login" : "CustomerLogin", model);
                }

                var user = await _commerceRepository.GetUserByUsernameAsync(model.Username, cancellationToken);

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
                    };

                    var claimsIdentity = new ClaimsIdentity(
                        claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties();

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme, 
                        new ClaimsPrincipal(claimsIdentity), 
                        authProperties);

                    if (user.Role == "admin")
                    {
                        return RedirectToAction("Index", "Admin");
                    }
                    else
                    {
                        return RedirectToAction("Index", "Landing");
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

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Landing");
        }
    }
}
