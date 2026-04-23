using System.ComponentModel.DataAnnotations;
using PerfumeStore.Web.Models.Domain;

namespace PerfumeStore.Web.Models.ViewModels;

public class CheckoutPageViewModel
{
    public IReadOnlyCollection<CartItemViewModel> CartItems { get; set; } = Array.Empty<CartItemViewModel>();
    public IReadOnlyCollection<PaymentMethod> PaymentMethods { get; init; } = Array.Empty<PaymentMethod>();
    public IReadOnlyCollection<DeliveryOption> DeliveryOptions { get; init; } = Array.Empty<DeliveryOption>();
    public decimal Subtotal { get; init; }
    public decimal DeliveryFee { get; init; }
    public decimal ProcessingFee { get; init; }
    public decimal EstimatedTotal { get; init; }
    public int CartItemCount { get; init; }
    public bool HasCart => CartItems.Count > 0;

    public CheckoutRequest Request { get; set; } = new();
}

public class CartItemViewModel
{
    public int ProductId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string ImageUrl { get; init; } = string.Empty;
    public decimal UnitPrice { get; init; }
    public decimal? OriginalPrice { get; init; }
    public int Quantity { get; init; }
    public int StockQuantity { get; init; }
    public decimal LineTotal => UnitPrice * Quantity;
}

public class CheckoutRequest
{
    [Required, StringLength(80)]
    public string CustomerName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string CustomerEmail { get; set; } = string.Empty;

    [Required, StringLength(260)]
    public string ShippingAddress { get; set; } = string.Empty;

    [Required]
    public int PaymentMethodId { get; set; }

    [Required]
    public int DeliveryOptionId { get; set; }

    public List<CheckoutLine> Items { get; set; } = new();
}

public class CheckoutLine
{
    [Required]
    public int ProductId { get; set; }

    [Range(1, 99)]
    public int Quantity { get; set; } = 1;
}

public class CheckoutResultViewModel
{
    public bool IsSuccess { get; init; }
    public string Message { get; init; } = string.Empty;
    public string? OrderNumber { get; init; }
}
