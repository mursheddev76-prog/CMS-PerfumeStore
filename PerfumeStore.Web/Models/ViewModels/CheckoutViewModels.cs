using System.ComponentModel.DataAnnotations;
using PerfumeStore.Web.Models.Domain;

namespace PerfumeStore.Web.Models.ViewModels;

public class CheckoutPageViewModel
{
    public IReadOnlyCollection<ProductCardViewModel> CartProducts { get; init; } = Array.Empty<ProductCardViewModel>();
    public IReadOnlyCollection<PaymentMethod> PaymentMethods { get; init; } = Array.Empty<PaymentMethod>();
    public IReadOnlyCollection<DeliveryOption> DeliveryOptions { get; init; } = Array.Empty<DeliveryOption>();
    public decimal Subtotal { get; init; }
    public decimal EstimatedTotal { get; init; }

    public CheckoutRequest Request { get; set; } = new();
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

