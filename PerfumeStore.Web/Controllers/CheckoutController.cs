using Microsoft.AspNetCore.Mvc;
using PerfumeStore.Web.Infrastructure;
using PerfumeStore.Web.Models.ViewModels;
using PerfumeStore.Web.Services;

namespace PerfumeStore.Web.Controllers;

[Route("checkout")]
public class CheckoutController : Controller
{
    private const string CartSessionKey = "checkout-cart";
    private readonly CheckoutService _checkoutService;

    public CheckoutController(CheckoutService checkoutService)
    {
        _checkoutService = checkoutService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var vm = await _checkoutService.BuildCheckoutAsync(GetCart(), null, null, cancellationToken);
        return View(vm);
    }

    [HttpPost("cart/add")]
    public IActionResult AddToCart([FromForm] int productId, [FromForm] int quantity = 1, [FromForm] string? returnUrl = null)
    {
        var cart = GetCart();
        var existingLine = cart.FirstOrDefault(item => item.ProductId == productId);

        if (existingLine is null)
        {
            cart.Add(new CheckoutLine
            {
                ProductId = productId,
                Quantity = Math.Clamp(quantity, 1, 99)
            });
        }
        else
        {
            existingLine.Quantity = Math.Clamp(existingLine.Quantity + quantity, 1, 99);
        }

        SaveCart(cart);
        TempData["CartMessage"] = "Item added to your cart.";
        return LocalRedirect(SanitizeReturnUrl(returnUrl));
    }

    [HttpPost("cart/update")]
    public IActionResult UpdateCart([FromForm] int productId, [FromForm] int quantity)
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
                line.Quantity = Math.Clamp(quantity, 1, 99);
                TempData["CartMessage"] = "Cart updated.";
            }

            SaveCart(cart);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("cart/remove")]
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
    public IActionResult ClearCart()
    {
        HttpContext.Session.Remove(CartSessionKey);
        TempData["CartMessage"] = "Your cart has been cleared.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("place-order")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PlaceOrder(CheckoutRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var vm = await _checkoutService.BuildCheckoutAsync(
                request.Items,
                request.PaymentMethodId,
                request.DeliveryOptionId,
                cancellationToken);
            vm.Request.CustomerName = request.CustomerName;
            vm.Request.CustomerEmail = request.CustomerEmail;
            vm.Request.ShippingAddress = request.ShippingAddress;
            return View("Index", vm);
        }

        var result = await _checkoutService.PlaceOrderAsync(request, cancellationToken);
        TempData["CheckoutResult"] = result.Message;
        TempData["CheckoutIsSuccess"] = result.IsSuccess;
        TempData["OrderNumber"] = result.OrderNumber;
        if (result.IsSuccess)
        {
            HttpContext.Session.Remove(CartSessionKey);
        }
        return RedirectToAction(nameof(Index));
    }

    private List<CheckoutLine> GetCart() =>
        HttpContext.Session.GetJson<List<CheckoutLine>>(CartSessionKey) ?? new List<CheckoutLine>();

    private void SaveCart(List<CheckoutLine> cart)
    {
        if (cart.Count == 0)
        {
            HttpContext.Session.Remove(CartSessionKey);
            return;
        }

        HttpContext.Session.SetJson(CartSessionKey, cart);
    }

    private string SanitizeReturnUrl(string? returnUrl) =>
        !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? returnUrl
            : Url.Action(nameof(Index), "Checkout") ?? "/checkout";
}
