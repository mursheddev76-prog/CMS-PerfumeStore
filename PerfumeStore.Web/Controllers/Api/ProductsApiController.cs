using Microsoft.AspNetCore.Mvc;
using PerfumeStore.Web.Services;

namespace PerfumeStore.Web.Controllers.Api;

[ApiController]
[Route("api/products")]
public class ProductsApiController : ControllerBase
{
    private readonly CatalogService _catalogService;

    public ProductsApiController(CatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? category, [FromQuery] string? q, CancellationToken cancellationToken)
    {
        var (products, _) = await _catalogService.SearchAsync(category, q, cancellationToken);
        return Ok(products);
    }
}

