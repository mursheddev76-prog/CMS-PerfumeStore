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
        var (products, categories) = await _catalogService.SearchAsync(category, q, User.Identity?.Name, cancellationToken);
        ViewData["CurrentCategory"] = category ?? "all";
        ViewData["Query"] = q ?? string.Empty;
        ViewData["Categories"] = categories;
        return View(products);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var model = await _catalogService.GetProductDetailsAsync(id, User.Identity?.Name, cancellationToken);
        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }
}
