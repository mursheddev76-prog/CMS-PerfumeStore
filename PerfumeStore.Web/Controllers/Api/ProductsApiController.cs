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
    public async Task<IActionResult> Get([FromQuery] int? id, [FromQuery] string? category, [FromQuery] string? q, CancellationToken cancellationToken)
    {
        var (products, _) = await _catalogService.SearchAsync(category, q, User.Identity?.Name, cancellationToken);
        if (id.HasValue)
        {
            var product = products.FirstOrDefault(item => item.Id == id.Value);
            return product is null ? NotFound() : Ok(product);
        }

        return Ok(products);
    }
}
