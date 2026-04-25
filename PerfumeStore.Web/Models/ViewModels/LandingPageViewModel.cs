namespace PerfumeStore.Web.Models.ViewModels;

public class LandingPageViewModel
{
    public HeroSectionViewModel Hero { get; init; } = new();
    public IReadOnlyCollection<ProductCardViewModel> Featured { get; init; } = Array.Empty<ProductCardViewModel>();
    public IReadOnlyCollection<ProductCardViewModel> Trending { get; init; } = Array.Empty<ProductCardViewModel>();
    public IReadOnlyCollection<string> CustomerHighlights { get; init; } = Array.Empty<string>();
    public IReadOnlyCollection<DeliveryBadgeViewModel> FulfillmentBadges { get; init; } = Array.Empty<DeliveryBadgeViewModel>();
}

public class HeroSectionViewModel
{
    public string Title { get; init; } = "Signature scents crafted for every mood.";
    public string Subtitle { get; init; } = "Discover artisanal fragrances curated by master perfumers.";
    public string BackgroundImageUrl { get; init; } = "/images/hero-default.jpg";
    public string PrimaryCtaText { get; init; } = "Shop New Arrivals";
    public string PrimaryCtaLink { get; init; } = "/catalog";
    public string SecondaryCtaText { get; init; } = "Build Your Set";
    public string SecondaryCtaLink { get; init; } = "/checkout";
}

public class ProductCardViewModel
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string ImageUrl { get; init; } = "/images/product-placeholder.jpg";
    public decimal Price { get; init; }
    public decimal? DiscountPrice { get; init; }
    public bool IsNewArrival { get; init; }
    public bool IsWishlisted { get; init; }
    public int StockQuantity { get; init; }
    public bool IsOutOfStock => StockQuantity <= 0;
}

public class DeliveryBadgeViewModel
{
    public string Title { get; init; } = string.Empty;
    public string Caption { get; init; } = string.Empty;
    public string Icon { get; init; } = "bi-truck";
}

public class ProductDetailsViewModel
{
    public ProductCardViewModel Product { get; init; } = new();
    public IReadOnlyCollection<ProductCardViewModel> RelatedProducts { get; init; } = Array.Empty<ProductCardViewModel>();
}
