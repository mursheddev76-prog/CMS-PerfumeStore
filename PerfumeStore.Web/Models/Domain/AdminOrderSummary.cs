namespace PerfumeStore.Web.Models.Domain;

public record AdminOrderSummary(
    string OrderNumber,
    string CustomerName,
    string CustomerEmail,
    string ShippingAddress,
    string Status,
    string PaymentStatus,
    string PaymentMethod,
    int PaymentMethodId,
    string PaymentType,
    string DeliveryOption,
    int ItemCount,
    decimal Total,
    DateTime CreatedAt,
    string? PaymentReceiptUrl,
    string? PaymentReference,
    string? PaymentReviewNotes);
