using Microsoft.AspNetCore.Mvc;
using PerfumeStore.Web.Models.ViewModels;
using PerfumeStore.Web.Services;

namespace PerfumeStore.Web.Controllers;

[Route("checkout")]
public class CheckoutController : Controller
{
    private readonly CheckoutService _checkoutService;

    public CheckoutController(CheckoutService checkoutService)
    {
        _checkoutService = checkoutService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var vm = await _checkoutService.BuildCheckoutAsync(cancellationToken);
        return View(vm);
    }

    [HttpPost("place-order")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PlaceOrder(CheckoutRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var vm = await _checkoutService.BuildCheckoutAsync(cancellationToken);
            vm.Request = request;
            return View("Index", vm);
        }

        var result = await _checkoutService.PlaceOrderAsync(request, cancellationToken);
        TempData["CheckoutResult"] = result.Message;
        TempData["CheckoutIsSuccess"] = result.IsSuccess;
        TempData["OrderNumber"] = result.OrderNumber;
        return RedirectToAction(nameof(Index));
    }
}

