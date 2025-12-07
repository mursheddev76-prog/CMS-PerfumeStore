namespace PerfumeStore.Web.Models.Domain;

public record Product(
    int Id,
    string Name,
    string Description,
    decimal Price,
    decimal? DiscountPrice,
    string ImageUrl,
    bool IsFeatured,
    bool IsTrending,
    int CategoryId,
    string CategoryName,
    int StockQuantity);

