namespace PerfumeStore.Web.Models.Domain;

public record CustomerOrderSummary(
    string OrderNumber,
    string Status,
    string PaymentStatus,
    string PaymentMethod,
    string DeliveryOption,
    int EstimatedDays,
    int ItemCount,
    decimal Total,
    DateTime CreatedAt,
    string? PaymentReceiptUrl,
    string? PaymentReference);
