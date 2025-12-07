using PerfumeStore.Web.Data.Repositories;
using PerfumeStore.Web.Models.ViewModels;

namespace PerfumeStore.Web.Services;

public class CatalogService
{
    private readonly ICommerceRepository _repository;

    public CatalogService(ICommerceRepository repository)
    {
        _repository = repository;
    }

    public async Task<(IReadOnlyCollection<ProductCardViewModel> Products, IReadOnlyCollection<string> Categories)> SearchAsync(
        string? category,
        string? query,
        CancellationToken cancellationToken)
    {
        var allProducts = await _repository.GetAllProductsAsync(cancellationToken);
        var filtered = allProducts.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(category) && !category.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            filtered = filtered.Where(p => p.CategoryName.Equals(category, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            filtered = filtered.Where(p =>
                p.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                p.Description.Contains(query, StringComparison.OrdinalIgnoreCase));
        }

        var items = filtered.Select(p => new ProductCardViewModel
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Category = p.CategoryName,
            ImageUrl = p.ImageUrl,
            Price = p.Price,
            DiscountPrice = p.DiscountPrice,
            IsNewArrival = p.IsFeatured
        }).ToList();

        var categories = allProducts.Select(p => p.CategoryName).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(c => c).ToList();

        return (items, categories);
    }
}

