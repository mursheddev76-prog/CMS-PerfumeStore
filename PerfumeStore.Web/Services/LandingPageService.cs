using PerfumeStore.Web.Data.Repositories;
using PerfumeStore.Web.Models.Domain;
using PerfumeStore.Web.Models.ViewModels;

namespace PerfumeStore.Web.Services;

public class LandingPageService
{
    private static readonly string[] DefaultHighlights =
    {
        "Carbon-neutral fulfillment within 48h in most metros.",
        "Complimentary gift wrapping and handwritten note.",
        "All fragrances vegan, cruelty free, IFRA certified."
    };

    private readonly ICommerceRepository _repository;

    public LandingPageService(ICommerceRepository repository)
    {
        _repository = repository;
    }

    public async Task<LandingPageViewModel> BuildLandingPageAsync(string? username, CancellationToken cancellationToken)
    {
        var hero = await _repository.GetHeroContentAsync(cancellationToken);
        var featured = await _repository.GetFeaturedProductsAsync(cancellationToken);
        var trending = await _repository.GetTrendingProductsAsync(cancellationToken);
        var deliveryOptions = await _repository.GetDeliveryOptionsAsync(cancellationToken);
        var wishlist = await GetWishlistProductIdsAsync(username, cancellationToken);

        return new LandingPageViewModel
        {
            Hero = new HeroSectionViewModel
            {
                Title = hero.Title,
                Subtitle = hero.Subtitle,
                BackgroundImageUrl = hero.BackgroundImageUrl,
                PrimaryCtaText = hero.PrimaryCtaText,
                PrimaryCtaLink = hero.PrimaryCtaLink,
                SecondaryCtaText = hero.SecondaryCtaText,
                SecondaryCtaLink = hero.SecondaryCtaLink
            },
            Featured = featured.Select(product => ToProductCard(product, wishlist)).ToList(),
            Trending = trending.Select(product => ToProductCard(product, wishlist)).ToList(),
            CustomerHighlights = DefaultHighlights,
            FulfillmentBadges = deliveryOptions.Where(o => o.IsActive).Select(opt => new DeliveryBadgeViewModel
            {
                Title = opt.Name,
                Caption = $"{opt.Description} · {opt.EstimatedDays} day(s)",
                Icon = opt.EstimatedDays <= 2 ? "bi-lightning-charge" : "bi-truck"
            }).ToList()
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

    private static ProductCardViewModel ToProductCard(Product product, HashSet<int> wishlist) => new()
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
    };
}
