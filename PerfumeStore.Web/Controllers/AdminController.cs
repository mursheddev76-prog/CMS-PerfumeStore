using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PerfumeStore.Web.Models.ViewModels;
using PerfumeStore.Web.Services;

namespace PerfumeStore.Web.Controllers;

[Route("admin")]
[Authorize(Policy = "Admin")]
public class AdminController : Controller
{
    private readonly AdminService _adminService;

    public AdminController(AdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(
        [FromQuery] int? categoryId,
        [FromQuery] int? productId,
        [FromQuery] int? paymentMethodId,
        [FromQuery] int? deliveryOptionId,
        CancellationToken cancellationToken)
    {
        var vm = await _adminService.BuildDashboardAsync(categoryId, productId, paymentMethodId, deliveryOptionId, cancellationToken);
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
    public async Task<IActionResult> SaveProduct(ProductInput ProductForm, IFormFile? ProductImage, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return await ReturnDashboardWithErrorsAsync(cancellationToken, productForm: ProductForm);
        }

        // If user attached a file, save it (store under wwwroot/uploads/products)
        if (ProductImage != null && ProductImage.Length > 0)
        {
            var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/products");
            if (!Directory.Exists(uploadFolder))
                Directory.CreateDirectory(uploadFolder);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ProductImage.FileName);
            var filePath = Path.Combine(uploadFolder, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await ProductImage.CopyToAsync(stream, cancellationToken);
            }

            // Update image URL to saved path
            ProductForm.ImageUrl = "/uploads/products/" + fileName;
        }

        var categories = (await _adminService.BuildDashboardAsync(cancellationToken)).Categories;
        var categoryName = categories.FirstOrDefault(c => c.Id == ProductForm.CategoryId)?.Name ?? "Uncategorized";
        await _adminService.SaveProductAsync(ProductForm, categoryName, cancellationToken);
        TempData["AdminMessage"] = ProductForm.Id == 0 ? "Product created." : "Product updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("categories")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveCategory([Bind(Prefix = "CategoryForm")] CategoryInput input, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return await ReturnDashboardWithErrorsAsync(cancellationToken, categoryForm: input);
        }

        await _adminService.SaveCategoryAsync(input, cancellationToken);
        TempData["AdminMessage"] = input.Id == 0 ? "Category created." : "Category updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("payments")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SavePayment([Bind(Prefix = "PaymentForm")] PaymentMethodInput input, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return await ReturnDashboardWithErrorsAsync(cancellationToken, paymentForm: input);
        }

        await _adminService.SavePaymentMethodAsync(input, cancellationToken);
        TempData["AdminMessage"] = input.Id == 0 ? "Payment method created." : "Payment method updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("delivery")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveDelivery([Bind(Prefix = "DeliveryForm")] DeliveryOptionInput input, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return await ReturnDashboardWithErrorsAsync(cancellationToken, deliveryForm: input);
        }

        await _adminService.SaveDeliveryOptionAsync(input, cancellationToken);
        TempData["AdminMessage"] = input.Id == 0 ? "Delivery option created." : "Delivery option updated.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<IActionResult> ReturnDashboardWithErrorsAsync(
        CancellationToken cancellationToken,
        CategoryInput? categoryForm = null,
        ProductInput? productForm = null,
        PaymentMethodInput? paymentForm = null,
        DeliveryOptionInput? deliveryForm = null)
    {
        var vm = await _adminService.BuildDashboardAsync(cancellationToken);
        if (categoryForm is not null)
        {
            vm = _adminService.WithCategoryForm(vm, categoryForm);
        }

        if (productForm is not null)
        {
            vm = _adminService.WithProductForm(vm, productForm);
        }

        if (paymentForm is not null)
        {
            vm = _adminService.WithPaymentForm(vm, paymentForm);
        }

        if (deliveryForm is not null)
        {
            vm = _adminService.WithDeliveryForm(vm, deliveryForm);
        }

        return View("Index", vm);
    }
}
