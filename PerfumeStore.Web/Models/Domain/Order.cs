namespace PerfumeStore.Web.Models.Domain;

public record Order(
    int Id,
    string OrderNumber,
    int CustomerId,
    string CustomerEmail,
    string CustomerName,
    string ShippingAddress,
    int PaymentMethodId,
    int DeliveryOptionId,
    decimal Subtotal,
    decimal DeliveryFee,
    decimal ProcessingFee,
    decimal Total,
    IReadOnlyCollection<OrderItem> Items,
    DateTime CreatedAt);

public record OrderItem(
    int ProductId,
    int Quantity,
    decimal UnitPrice);

