using System.ComponentModel.DataAnnotations;
using PerfumeStore.Web.Models.Domain;

namespace PerfumeStore.Web.Models.ViewModels;

public class AdminDashboardViewModel
{
    public AdminDashboardStats Stats { get; init; } = new(0, 0, 0, 0, 0);
    public IReadOnlyCollection<Category> Categories { get; init; } = Array.Empty<Category>();
    public IReadOnlyCollection<PaymentMethod> PaymentMethods { get; init; } = Array.Empty<PaymentMethod>();
    public IReadOnlyCollection<DeliveryOption> DeliveryOptions { get; init; } = Array.Empty<DeliveryOption>();
    public IReadOnlyCollection<Product> Products { get; init; } = Array.Empty<Product>();
    public HeroSectionInput Hero { get; init; } = new();
    public ProductInput ProductForm { get; init; } = new();
    public PaymentMethodInput PaymentForm { get; init; } = new();
    public DeliveryOptionInput DeliveryForm { get; init; } = new();
}

public class HeroSectionInput
{
    [Required, StringLength(80)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(160)]
    public string Subtitle { get; set; } = string.Empty;

    [Required, StringLength(256)]
    public string BackgroundImageUrl { get; set; } = "/images/hero-default.jpg";



    [Required, StringLength(40)]
    public string PrimaryCtaText { get; set; } = "Shop Now";

    [Required, StringLength(120)]
    public string PrimaryCtaLink { get; set; } = "/catalog";

    [StringLength(40)]
    public string SecondaryCtaText { get; set; } = "Gift Builder";

    [StringLength(120)]
    public string SecondaryCtaLink { get; set; } = "/checkout";
}

public class ProductInput
{
    public int Id { get; set; }

    [Required, StringLength(80)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(256)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public decimal Price { get; set; }

    public decimal? DiscountPrice { get; set; }

    [Required]
    public int CategoryId { get; set; }

    [Required]
    public int StockQuantity { get; set; }

    [Required, StringLength(256)]
    public string ImageUrl { get; set; } = "/images/product-placeholder.jpg";

    public bool IsFeatured { get; set; }
    public bool IsTrending { get; set; }
}

public class PaymentMethodInput
{
    public int Id { get; set; }

    [Required, StringLength(60)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(60)]
    public string Provider { get; set; } = string.Empty;

    [Range(0, 50)]
    public decimal ProcessingFee { get; set; }

    public bool SupportsInstallments { get; set; }
    public bool IsActive { get; set; } = true;
}

public class DeliveryOptionInput
{
    public int Id { get; set; }

    [Required, StringLength(60)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(160)]
    public string Description { get; set; } = string.Empty;

    [Range(0, 100)]
    public decimal Fee { get; set; }

    [Range(1, 30)]
    public int EstimatedDays { get; set; }

    public bool IsActive { get; set; } = true;
}

