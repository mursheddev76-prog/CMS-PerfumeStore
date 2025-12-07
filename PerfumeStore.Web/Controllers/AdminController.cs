using Microsoft.AspNetCore.Mvc;
using PerfumeStore.Web.Models.ViewModels;
using PerfumeStore.Web.Services;
using System.Reflection;

namespace PerfumeStore.Web.Controllers;

[Route("admin")]
public class AdminController : Controller
{
    private readonly AdminService _adminService;

    public AdminController(AdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var vm = await _adminService.BuildDashboardAsync(cancellationToken);
        return View(vm);
    }

    [HttpPost("hero")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveHero(HeroSectionInput Hero, IFormFile HeroBackgroundImage, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return await ReturnDashboardWithErrorsAsync(cancellationToken);
        }
        // 1. If user attached a file, save it
        if (HeroBackgroundImage != null && HeroBackgroundImage.Length > 0)
        {
            // Create upload folder if not exists
            var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/hero");
            if (!Directory.Exists(uploadFolder))
                Directory.CreateDirectory(uploadFolder);
            // Unique file name
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(HeroBackgroundImage.FileName);

            // Full server path
            var filePath = Path.Combine(uploadFolder, fileName);
            // Store file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await HeroBackgroundImage.CopyToAsync(stream);
            }

            // Set URL to be saved in database
            Hero.BackgroundImageUrl = "/uploads/hero/" + fileName;

        }

        await _adminService.SaveHeroAsync(Hero, cancellationToken);
        TempData["AdminMessage"] = "Hero section updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("products")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveProduct(ProductInput input, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return await ReturnDashboardWithErrorsAsync(cancellationToken);
        }

        var categories = (await _adminService.BuildDashboardAsync(cancellationToken)).Categories;
        var categoryName = categories.FirstOrDefault(c => c.Id == input.CategoryId)?.Name ?? "Uncategorized";
        await _adminService.SaveProductAsync(input, categoryName, cancellationToken);
        TempData["AdminMessage"] = "Product saved.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("payments")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SavePayment(PaymentMethodInput input, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return await ReturnDashboardWithErrorsAsync(cancellationToken);
        }

        await _adminService.SavePaymentMethodAsync(input, cancellationToken);
        TempData["AdminMessage"] = "Payment method saved.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("delivery")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveDelivery(DeliveryOptionInput input, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return await ReturnDashboardWithErrorsAsync(cancellationToken);
        }

        await _adminService.SaveDeliveryOptionAsync(input, cancellationToken);
        TempData["AdminMessage"] = "Delivery option saved.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<IActionResult> ReturnDashboardWithErrorsAsync(CancellationToken cancellationToken)
    {
        var vm = await _adminService.BuildDashboardAsync(cancellationToken);
        return View("Index", vm);
    }
}

