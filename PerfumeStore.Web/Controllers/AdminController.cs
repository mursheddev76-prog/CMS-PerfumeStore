using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using PerfumeStore.Web.Infrastructure;
using PerfumeStore.Web.Models.ViewModels;
using PerfumeStore.Web.Services;

namespace PerfumeStore.Web.Controllers;

[Route("admin")]
[Authorize(Policy = "StaffPortal")]
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
        [FromQuery] int? userId,
        [FromQuery] string? orderQuery,
        [FromQuery] string? status,
        [FromQuery] string? paymentStatus,
        [FromQuery] int? orderPaymentMethodId,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        CancellationToken cancellationToken)
    {
        var vm = await _adminService.BuildDashboardAsync(
            categoryId,
            productId,
            paymentMethodId,
            deliveryOptionId,
            userId,
            new AdminOrderFiltersInput
            {
                Query = orderQuery,
                Status = status,
                PaymentStatus = paymentStatus,
                PaymentMethodId = orderPaymentMethodId,
                DateFrom = dateFrom,
                DateTo = dateTo
            },
            cancellationToken);
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

    [HttpPost("orders/review")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateOrderStatus([Bind(Prefix = "OrderReviewForm")] OrderReviewInput input, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return await ReturnDashboardWithErrorsAsync(cancellationToken, orderStatusForm: input);
        }

        await _adminService.UpdateOrderReviewAsync(input, cancellationToken);
        TempData["AdminMessage"] = $"Order {input.OrderNumber} updated with {input.Status} / {input.PaymentStatus}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("users")]
    [Authorize(Policy = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveUser([Bind(Prefix = "UserForm")] UserManagementInput input, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(input.Password) && input.Id == 0)
        {
            ModelState.AddModelError("UserForm.Password", "Password is required for new users.");
        }

        if (!string.IsNullOrWhiteSpace(input.Password) && input.Password.Length < 8)
        {
            ModelState.AddModelError("UserForm.Password", "Password must be at least 8 characters.");
        }

        if (!ModelState.IsValid)
        {
            return await ReturnDashboardWithErrorsAsync(cancellationToken, userForm: input);
        }

        await _adminService.SaveUserAsync(input, cancellationToken);
        TempData["AdminMessage"] = input.Id == 0 ? "User created." : "User updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("categories/{id:int}/toggle")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleCategory(int id, [FromForm] bool isActive, CancellationToken cancellationToken)
    {
        await _adminService.SetCategoryActiveAsync(id, isActive, cancellationToken);
        TempData["AdminMessage"] = $"Category {(isActive ? "activated" : "moved to inactive")}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("categories/{id:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategory(int id, CancellationToken cancellationToken)
    {
        return await ExecuteProtectedDeleteAsync(
            () => _adminService.DeleteCategoryAsync(id, cancellationToken),
            "Category deleted.",
            "This category is linked to products. Mark it inactive instead of deleting it.");
    }

    [HttpPost("products/{id:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteProduct(int id, CancellationToken cancellationToken)
    {
        return await ExecuteProtectedDeleteAsync(
            () => _adminService.DeleteProductAsync(id, cancellationToken),
            "Product deleted.",
            "This product is already used in orders or wishlists, so it cannot be deleted safely.");
    }

    [HttpPost("payments/{id:int}/toggle")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TogglePayment(int id, [FromForm] bool isActive, CancellationToken cancellationToken)
    {
        await _adminService.SetPaymentMethodActiveAsync(id, isActive, cancellationToken);
        TempData["AdminMessage"] = $"Payment method {(isActive ? "activated" : "moved to inactive")}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("payments/{id:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePayment(int id, CancellationToken cancellationToken)
    {
        return await ExecuteProtectedDeleteAsync(
            () => _adminService.DeletePaymentMethodAsync(id, cancellationToken),
            "Payment method deleted.",
            "This payment method is already used by orders. Mark it inactive instead of deleting it.");
    }

    [HttpPost("delivery/{id:int}/toggle")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleDelivery(int id, [FromForm] bool isActive, CancellationToken cancellationToken)
    {
        await _adminService.SetDeliveryOptionActiveAsync(id, isActive, cancellationToken);
        TempData["AdminMessage"] = $"Delivery option {(isActive ? "activated" : "moved to inactive")}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("delivery/{id:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteDelivery(int id, CancellationToken cancellationToken)
    {
        return await ExecuteProtectedDeleteAsync(
            () => _adminService.DeleteDeliveryOptionAsync(id, cancellationToken),
            "Delivery option deleted.",
            "This delivery option is already used by orders. Mark it inactive instead of deleting it.");
    }

    [HttpPost("users/{id:int}/toggle")]
    [Authorize(Policy = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleUser(int id, [FromForm] bool isActive, CancellationToken cancellationToken)
    {
        await _adminService.SetUserActiveAsync(id, isActive, cancellationToken);
        TempData["AdminMessage"] = $"User {(isActive ? "activated" : "deactivated")}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("users/{id:int}/delete")]
    [Authorize(Policy = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(int id, CancellationToken cancellationToken)
    {
        return await ExecuteProtectedDeleteAsync(
            () => _adminService.DeleteUserAsync(id, cancellationToken),
            "User deleted.",
            "This user could not be deleted because related records still depend on it.");
    }

    private async Task<IActionResult> ReturnDashboardWithErrorsAsync(
        CancellationToken cancellationToken,
        CategoryInput? categoryForm = null,
        ProductInput? productForm = null,
        PaymentMethodInput? paymentForm = null,
        DeliveryOptionInput? deliveryForm = null,
        OrderReviewInput? orderStatusForm = null,
        UserManagementInput? userForm = null)
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

        if (orderStatusForm is not null)
        {
            vm = _adminService.WithOrderStatusForm(vm, orderStatusForm);
        }

        if (userForm is not null)
        {
            vm = _adminService.WithUserForm(vm, userForm);
        }

        return View("Index", vm);
    }

    private async Task<IActionResult> ExecuteProtectedDeleteAsync(
        Func<Task> deleteAction,
        string successMessage,
        string foreignKeyMessage)
    {
        try
        {
            await deleteAction();
            TempData["AdminMessage"] = successMessage;
            TempData["AdminMessageIsError"] = false;
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.ForeignKeyViolation)
        {
            TempData["AdminMessage"] = foreignKeyMessage;
            TempData["AdminMessageIsError"] = true;
        }

        return RedirectToAction(nameof(Index));
    }
}
