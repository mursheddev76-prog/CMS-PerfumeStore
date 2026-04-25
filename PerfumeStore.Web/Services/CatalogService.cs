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
        string? username,
        CancellationToken cancellationToken)
    {
        var allProducts = await _repository.GetAllProductsAsync(cancellationToken);
        var wishlist = await GetWishlistProductIdsAsync(username, cancellationToken);
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
            IsNewArrival = p.IsFeatured,
            IsWishlisted = wishlist.Contains(p.Id),
            StockQuantity = p.StockQuantity
        }).ToList();

        var categories = allProducts.Select(p => p.CategoryName).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(c => c).ToList();

        return (items, categories);
    }

    public async Task<ProductDetailsViewModel?> GetProductDetailsAsync(int id, string? username, CancellationToken cancellationToken)
    {
        var product = await _repository.GetProductByIdAsync(id, cancellationToken);
        if (product is null)
        {
            return null;
        }

        var wishlist = await GetWishlistProductIdsAsync(username, cancellationToken);
        var related = (await _repository.GetAllProductsAsync(cancellationToken))
            .Where(item => item.Id != id && item.CategoryId == product.CategoryId)
            .Take(4)
            .Select(item => new ProductCardViewModel
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                Category = item.CategoryName,
                ImageUrl = item.ImageUrl,
                Price = item.Price,
                DiscountPrice = item.DiscountPrice,
                IsNewArrival = item.IsFeatured,
                IsWishlisted = wishlist.Contains(item.Id),
                StockQuantity = item.StockQuantity
            })
            .ToList();

        return new ProductDetailsViewModel
        {
            Product = new ProductCardViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Category = product.CategoryName,
                ImageUrl = product.ImageUrl,
                Price = product.Price,
                DiscountPrice = product.DiscountPrice,
                IsNewArrival = product.IsFeatured && product.IsTrending,
                IsWishlisted = wishlist.Contains(product.Id),
                StockQuantity = product.StockQuantity
            },
            RelatedProducts = related
        };
    }

    private async Task<HashSet<int>> GetWishlistProductIdsAsync(string? username, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return new HashSet<int>();
        }

        var ids = await _repository.GetWishlistProductIdsAsync(username, cancellationToken);
        return ids.ToHashSet();
    }
}
