using Microsoft.AspNetCore.Mvc;
using PerfumeStore.Web.Infrastructure;
using PerfumeStore.Web.Models.ViewModels;
using PerfumeStore.Web.Services;
using PerfumeStore.Web.Data.Repositories;
using Npgsql;

namespace PerfumeStore.Web.Controllers;

[Route("checkout")]
public class CheckoutController : Controller
{
    private readonly CheckoutService _checkoutService;
    private readonly ICommerceRepository _repository;

    public CheckoutController(CheckoutService checkoutService, ICommerceRepository repository)
    {
        _checkoutService = checkoutService;
        _repository = repository;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var user = await GetCurrentUserAsync(cancellationToken);
        var vm = await _checkoutService.BuildCheckoutAsync(
            GetCart(),
            null,
            null,
            user?.FullName,
            user?.Username,
            cancellationToken);
        return View(vm);
    }

    [HttpPost("cart/add")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddToCart([FromForm] int productId, [FromForm] int quantity = 1, [FromForm] string? returnUrl = null, CancellationToken cancellationToken = default)
    {
        var product = await _repository.GetProductByIdAsync(productId, cancellationToken);
        if (product is null)
        {
            TempData["CartMessage"] = "This product is no longer available.";
            return LocalRedirect(SanitizeReturnUrl(returnUrl));
        }

        if (product.StockQuantity <= 0)
        {
            TempData["CartMessage"] = $"{product.Name} is currently out of stock.";
            return LocalRedirect(SanitizeReturnUrl(returnUrl));
        }

        var cart = GetCart();
        var existingLine = cart.FirstOrDefault(item => item.ProductId == productId);
        var requestedQuantity = Math.Clamp(quantity, 1, 99);

        if (existingLine is null)
        {
            cart.Add(new CheckoutLine
            {
                ProductId = productId,
                Quantity = Math.Min(requestedQuantity, product.StockQuantity)
            });
        }
        else
        {
            existingLine.Quantity = Math.Min(Math.Clamp(existingLine.Quantity + requestedQuantity, 1, 99), product.StockQuantity);
        }

        SaveCart(cart);
        TempData["CartMessage"] = existingLine is null && requestedQuantity <= product.StockQuantity
            ? "Item added to your cart."
            : $"Cart updated. Available stock for {product.Name}: {product.StockQuantity}.";
        return LocalRedirect(SanitizeReturnUrl(returnUrl));
    }

    [HttpPost("cart/update")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateCart([FromForm] int productId, [FromForm] int quantity, CancellationToken cancellationToken)
    {
        var cart = GetCart();
        var line = cart.FirstOrDefault(item => item.ProductId == productId);
        if (line is not null)
        {
            if (quantity <= 0)
            {
                cart.Remove(line);
                TempData["CartMessage"] = "Item removed from your cart.";
            }
            else
            {
                var product = await _repository.GetProductByIdAsync(productId, cancellationToken);
                if (product is null || product.StockQuantity <= 0)
                {
                    cart.Remove(line);
                    TempData["CartMessage"] = "This item is no longer available and was removed from your cart.";
                }
                else
                {
                    line.Quantity = Math.Min(Math.Clamp(quantity, 1, 99), product.StockQuantity);
                    TempData["CartMessage"] = line.Quantity < quantity
                        ? $"Quantity adjusted to available stock for {product.Name}."
                        : "Cart updated.";
                }
            }

            SaveCart(cart);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("cart/remove")]
    [ValidateAntiForgeryToken]
    public IActionResult RemoveFromCart([FromForm] int productId)
    {
        var cart = GetCart();
        var line = cart.FirstOrDefault(item => item.ProductId == productId);
        if (line is not null)
        {
            cart.Remove(line);
            SaveCart(cart);
            TempData["CartMessage"] = "Item removed from your cart.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("cart/clear")]
    [ValidateAntiForgeryToken]
    public IActionResult ClearCart()
    {
        HttpContext.Session.Remove(GetCartSessionKey());
        TempData["CartMessage"] = "Your cart has been cleared.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("place-order")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PlaceOrder([Bind(Prefix = "Request")] CheckoutRequest request, IFormFile? paymentReceipt, CancellationToken cancellationToken)
    {
        if (request.Items.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "Your cart is empty. Add at least one product before completing payment.");
        }

        var selectedPaymentMethod = (await _repository.GetPaymentMethodsAsync(cancellationToken))
            .FirstOrDefault(method => method.Id == request.PaymentMethodId && method.IsActive);

        if (selectedPaymentMethod?.PaymentType == "manual" && selectedPaymentMethod.RequiresReceipt && (paymentReceipt is null || paymentReceipt.Length == 0))
        {
            ModelState.AddModelError(string.Empty, "Please upload your payment receipt for manual bank transfer.");
        }

        if (!ModelState.IsValid)
        {
            var currentUser = await GetCurrentUserAsync(cancellationToken);
            var vm = await _checkoutService.BuildCheckoutAsync(
                request.Items,
                request.PaymentMethodId,
                request.DeliveryOptionId,
                string.IsNullOrWhiteSpace(request.CustomerName) ? currentUser?.FullName : request.CustomerName,
                string.IsNullOrWhiteSpace(request.CustomerEmail) ? currentUser?.Username : request.CustomerEmail,
                cancellationToken);
            vm.Request.CustomerName = request.CustomerName;
            vm.Request.CustomerEmail = request.CustomerEmail;
            vm.Request.ShippingAddress = request.ShippingAddress;
            vm.Request.PaymentReference = request.PaymentReference;
            return View("Index", vm);
        }

        string? paymentReceiptUrl = null;
        if (paymentReceipt is not null && paymentReceipt.Length > 0)
        {
            var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/payment-receipts");
            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(paymentReceipt.FileName)}";
            var filePath = Path.Combine(uploadFolder, fileName);
            await using var stream = new FileStream(filePath, FileMode.Create);
            await paymentReceipt.CopyToAsync(stream, cancellationToken);
            paymentReceiptUrl = $"/uploads/payment-receipts/{fileName}";
        }

        CheckoutResultViewModel result;
        try
        {
            result = await _checkoutService.PlaceOrderAsync(request, paymentReceiptUrl, cancellationToken);
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.ForeignKeyViolation || ex.SqlState == PostgresErrorCodes.RaiseException)
        {
            result = new CheckoutResultViewModel
            {
                IsSuccess = false,
                Message = "We could not complete the payment because one of the selected checkout items changed. Please review your cart and try again."
            };
        }

        TempData["CheckoutResult"] = result.Message;
        TempData["CheckoutIsSuccess"] = result.IsSuccess;
        TempData["OrderNumber"] = result.OrderNumber;
        if (result.IsSuccess)
        {
            HttpContext.Session.Remove(GetCartSessionKey());
        }
        return RedirectToAction(nameof(Index));
    }

    private List<CheckoutLine> GetCart() =>
        HttpContext.Session.GetJson<List<CheckoutLine>>(GetCartSessionKey()) ?? new List<CheckoutLine>();

    private void SaveCart(List<CheckoutLine> cart)
    {
        var cartSessionKey = GetCartSessionKey();
        if (cart.Count == 0)
        {
            HttpContext.Session.Remove(cartSessionKey);
            return;
        }

        HttpContext.Session.SetJson(cartSessionKey, cart);
    }

    private string SanitizeReturnUrl(string? returnUrl) =>
        !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? returnUrl
            : Url.Action(nameof(Index), "Checkout") ?? "/checkout";

    private Task<PerfumeStore.Web.Models.Domain.User?> GetCurrentUserAsync(CancellationToken cancellationToken) =>
        User.Identity?.IsAuthenticated == true && !string.IsNullOrWhiteSpace(User.Identity?.Name)
            ? _repository.GetUserByUsernameAsync(User.Identity!.Name!, cancellationToken)
            : Task.FromResult<PerfumeStore.Web.Models.Domain.User?>(null);

    private string GetCartSessionKey() => CartSessionHelper.GetCartKey(User, HttpContext.Session);
}
