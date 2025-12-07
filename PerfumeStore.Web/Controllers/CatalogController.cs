using Microsoft.AspNetCore.Mvc;
using PerfumeStore.Web.Services;

namespace PerfumeStore.Web.Controllers;

[Route("catalog")]
public class CatalogController : Controller
{
    private readonly CatalogService _catalogService;

    public CatalogController(CatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] string? category, [FromQuery] string? q, CancellationToken cancellationToken)
    {
        var (products, categories) = await _catalogService.SearchAsync(category, q, cancellationToken);
        ViewData["CurrentCategory"] = category ?? "all";
        ViewData["Query"] = q ?? string.Empty;
        ViewData["Categories"] = categories;
        return View(products);
    }
}

