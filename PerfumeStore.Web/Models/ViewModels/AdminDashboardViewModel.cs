using System.ComponentModel.DataAnnotations;
using PerfumeStore.Web.Infrastructure;
using PerfumeStore.Web.Models.Domain;

namespace PerfumeStore.Web.Models.ViewModels;

public class AdminDashboardViewModel
{
    public AdminDashboardStats Stats { get; set; } = new(0, 0, 0, 0, 0);
    public IReadOnlyCollection<Category> Categories { get; set; } = Array.Empty<Category>();
    public IReadOnlyCollection<PaymentMethod> PaymentMethods { get; set; } = Array.Empty<PaymentMethod>();
    public IReadOnlyCollection<DeliveryOption> DeliveryOptions { get; set; } = Array.Empty<DeliveryOption>();
    public IReadOnlyCollection<Product> Products { get; set; } = Array.Empty<Product>();
    public IReadOnlyCollection<AdminOrderSummaryViewModel> RecentOrders { get; set; } = Array.Empty<AdminOrderSummaryViewModel>();
    public IReadOnlyCollection<AdminCustomerSummaryViewModel> TopCustomers { get; set; } = Array.Empty<AdminCustomerSummaryViewModel>();
    public IReadOnlyCollection<AppUserSummary> Users { get; set; } = Array.Empty<AppUserSummary>();
    public HeroSectionInput Hero { get; set; } = new();
    public CategoryInput CategoryForm { get; set; } = new();
    public ProductInput ProductForm { get; set; } = new();
    public PaymentMethodInput PaymentForm { get; set; } = new();
    public DeliveryOptionInput DeliveryForm { get; set; } = new();
    public OrderReviewInput OrderReviewForm { get; set; } = new();
    public UserManagementInput UserForm { get; set; } = new();
    public AdminOrderFiltersInput OrderFilters { get; set; } = new();
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

    [Required]
    public string PaymentType { get; set; } = "manual";

    [StringLength(80)]
    public string? PartnerName { get; set; }

    [Range(0, 50)]
    public decimal ProcessingFee { get; set; }

    public bool SupportsInstallments { get; set; }
    public bool IsActive { get; set; } = true;

    [StringLength(120)]
    public string? AccountTitle { get; set; }

    [StringLength(80)]
    public string? AccountNumber { get; set; }

    [StringLength(80)]
    public string? BankName { get; set; }

    [StringLength(80)]
    public string? Iban { get; set; }

    [StringLength(500)]
    public string? Instructions { get; set; }

    public bool RequiresReceipt { get; set; } = true;
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

public class CategoryInput
{
    public int Id { get; set; }

    [Required, StringLength(80)]
    public string Name { get; set; } = string.Empty;

    [StringLength(255)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}

public class OrderReviewInput
{
    [Required]
    public string OrderNumber { get; set; } = string.Empty;

    [Required]
    public string Status { get; set; } = "Processing";

    [Required]
    public string PaymentStatus { get; set; } = "Pending Review";

    [StringLength(300)]
    public string? PaymentReviewNotes { get; set; }
}

public class AdminOrderSummaryViewModel
{
    public string OrderNumber { get; init; } = string.Empty;
    public string CustomerName { get; init; } = string.Empty;
    public string CustomerEmail { get; init; } = string.Empty;
    public string ShippingAddress { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string PaymentStatus { get; init; } = string.Empty;
    public string PaymentMethod { get; init; } = string.Empty;
    public int PaymentMethodId { get; init; }
    public string PaymentType { get; init; } = string.Empty;
    public string DeliveryOption { get; init; } = string.Empty;
    public int ItemCount { get; init; }
    public decimal Total { get; init; }
    public DateTime CreatedAt { get; init; }
    public string? PaymentReceiptUrl { get; init; }
    public string? PaymentReference { get; init; }
    public string? PaymentReviewNotes { get; init; }
}

public class AdminCustomerSummaryViewModel
{
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public int OrderCount { get; init; }
    public decimal LifetimeValue { get; init; }
    public int WishlistCount { get; init; }
    public DateTime? LastOrderAt { get; init; }
}

public class AdminOrderFiltersInput
{
    public string? Query { get; set; }
    public string? Status { get; set; }
    public string? PaymentStatus { get; set; }
    public int? PaymentMethodId { get; set; }
    [DataType(DataType.Date)]
    public DateTime? DateFrom { get; set; }
    [DataType(DataType.Date)]
    public DateTime? DateTo { get; set; }
}

public class UserManagementInput
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(80)]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = AppRoles.Customer;

    public bool IsActive { get; set; } = true;

    [StringLength(100)]
    public string? Password { get; set; }
}
