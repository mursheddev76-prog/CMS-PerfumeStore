namespace PerfumeStore.Web.Models.ViewModels;

public class CustomerDashboardViewModel
{
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public IReadOnlyCollection<CustomerOrderViewModel> Orders { get; init; } = Array.Empty<CustomerOrderViewModel>();
    public IReadOnlyCollection<ProductCardViewModel> Wishlist { get; init; } = Array.Empty<ProductCardViewModel>();
}

public class CustomerOrderViewModel
{
    public string OrderNumber { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string PaymentStatus { get; init; } = string.Empty;
    public string PaymentMethod { get; init; } = string.Empty;
    public string DeliveryOption { get; init; } = string.Empty;
    public int EstimatedDays { get; init; }
    public int ItemCount { get; init; }
    public decimal Total { get; init; }
    public DateTime CreatedAt { get; init; }
    public string? PaymentReference { get; init; }
    public string? PaymentReceiptUrl { get; init; }
}
