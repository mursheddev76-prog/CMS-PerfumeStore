using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PerfumeStore.Web.Data.Repositories;
using PerfumeStore.Web.Services;

namespace PerfumeStore.Web.Controllers;

[Authorize(Roles = "customer")]
[Route("account")]
public class CustomerController : Controller
{
    private readonly CustomerService _customerService;
    private readonly ICommerceRepository _repository;

    public CustomerController(CustomerService customerService, ICommerceRepository repository)
    {
        _customerService = customerService;
        _repository = repository;
    }

    [HttpGet("")]
    [HttpGet("dashboard")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var model = await _customerService.BuildDashboardAsync(User.Identity?.Name ?? string.Empty, cancellationToken);
        if (model is null)
        {
            return RedirectToAction("Login", "Account", new { type = "customer" });
        }

        return View(model);
    }

    [HttpPost("wishlist/add")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddToWishlist([FromForm] int productId, [FromForm] string? returnUrl, CancellationToken cancellationToken)
    {
        await _repository.AddWishlistItemAsync(User.Identity?.Name ?? string.Empty, productId, cancellationToken);
        TempData["CustomerMessage"] = "Added to your wishlist.";
        return LocalRedirect(SanitizeReturnUrl(returnUrl));
    }

    [HttpPost("wishlist/remove")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveFromWishlist([FromForm] int productId, [FromForm] string? returnUrl, CancellationToken cancellationToken)
    {
        await _repository.RemoveWishlistItemAsync(User.Identity?.Name ?? string.Empty, productId, cancellationToken);
        TempData["CustomerMessage"] = "Removed from your wishlist.";
        return LocalRedirect(SanitizeReturnUrl(returnUrl));
    }

    private string SanitizeReturnUrl(string? returnUrl) =>
        !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? returnUrl
            : Url.Action(nameof(Index), "Customer") ?? "/account";
}
