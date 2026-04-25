using PerfumeStore.Web.Data.Repositories;
using PerfumeStore.Web.Models.Domain;
using PerfumeStore.Web.Models.ViewModels;

namespace PerfumeStore.Web.Services;

public class CheckoutService
{
    private readonly ICommerceRepository _repository;

    public CheckoutService(ICommerceRepository repository)
    {
        _repository = repository;
    }

    public async Task<CheckoutPageViewModel> BuildCheckoutAsync(
        IReadOnlyCollection<CheckoutLine>? cartLines,
        int? selectedPaymentMethodId,
        int? selectedDeliveryOptionId,
        string? customerName,
        string? customerEmail,
        CancellationToken cancellationToken)
    {
        var productsTask = _repository.GetAllProductsAsync(cancellationToken);
        var paymentsTask = _repository.GetPaymentMethodsAsync(cancellationToken);
        var deliveryTask = _repository.GetDeliveryOptionsAsync(cancellationToken);

        await Task.WhenAll(productsTask, paymentsTask, deliveryTask);

        var products = productsTask.Result;
        var cartItems = (cartLines ?? Array.Empty<CheckoutLine>())
            .Where(line => line.Quantity > 0)
            .Select(line => (line, product: products.FirstOrDefault(product => product.Id == line.ProductId)))
            .Where(tuple => tuple.product is not null)
            .Select(tuple =>
            {
                var product = tuple.product!;
                var unitPrice = product.DiscountPrice ?? product.Price;

                return new CartItemViewModel
                {
                    ProductId = product.Id,
                    Name = product.Name,
                    Category = product.CategoryName,
                    ImageUrl = product.ImageUrl,
                    UnitPrice = unitPrice,
                    OriginalPrice = product.DiscountPrice.HasValue ? product.Price : null,
                    Quantity = Math.Min(tuple.line.Quantity, Math.Max(product.StockQuantity, 1)),
                    StockQuantity = product.StockQuantity
                };
            })
            .ToList();

        var paymentMethods = paymentsTask.Result.Where(p => p.IsActive).ToList();
        var deliveryOptions = deliveryTask.Result.Where(d => d.IsActive).ToList();
        var paymentMethod = paymentMethods.FirstOrDefault(p => p.Id == selectedPaymentMethodId) ?? paymentMethods.FirstOrDefault();
        var deliveryOption = deliveryOptions.FirstOrDefault(d => d.Id == selectedDeliveryOptionId) ?? deliveryOptions.FirstOrDefault();

        var subtotal = cartItems.Sum(item => item.LineTotal);
        var deliveryFee = deliveryOption?.Fee ?? 0m;
        var processingFee = paymentMethod?.ProcessingFee ?? 0m;

        return new CheckoutPageViewModel
        {
            CartItems = cartItems,
            CartItemCount = cartItems.Sum(item => item.Quantity),
            PaymentMethods = paymentMethods,
            DeliveryOptions = deliveryOptions,
            Subtotal = subtotal,
            DeliveryFee = deliveryFee,
            ProcessingFee = processingFee,
            EstimatedTotal = subtotal + deliveryFee + processingFee,
            Request = new CheckoutRequest
            {
                CustomerName = customerName ?? string.Empty,
                CustomerEmail = customerEmail ?? string.Empty,
                PaymentMethodId = paymentMethod?.Id ?? 0,
                DeliveryOptionId = deliveryOption?.Id ?? 0,
                Items = cartItems.Select(item => new CheckoutLine
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
                }).ToList()
            }
        };
    }

    public async Task<CheckoutResultViewModel> PlaceOrderAsync(CheckoutRequest request, string? paymentReceiptUrl, CancellationToken cancellationToken)
    {
        var products = await _repository.GetAllProductsAsync(cancellationToken);
        var paymentOption = (await _repository.GetPaymentMethodsAsync(cancellationToken))
            .FirstOrDefault(p => p.Id == request.PaymentMethodId && p.IsActive);
        var deliveryOption = (await _repository.GetDeliveryOptionsAsync(cancellationToken))
            .FirstOrDefault(d => d.Id == request.DeliveryOptionId && d.IsActive);

        if (paymentOption is null || deliveryOption is null)
        {
            return new CheckoutResultViewModel
            {
                IsSuccess = false,
                Message = "Selected payment or delivery option is no longer available."
            };
        }

        if (paymentOption.PaymentType == "manual" && paymentOption.RequiresReceipt && string.IsNullOrWhiteSpace(paymentReceiptUrl))
        {
            return new CheckoutResultViewModel
            {
                IsSuccess = false,
                Message = "A payment receipt is required for manual bank transfer orders."
            };
        }

        var cartLines = request.Items
            .Select(line => (line, product: products.FirstOrDefault(p => p.Id == line.ProductId)))
            .Where(tuple => tuple.product is not null)
            .Where(tuple => tuple.line.Quantity > 0)
            .Where(tuple => tuple.product!.StockQuantity >= tuple.line.Quantity)
            .Select(tuple =>
            {
                var price = tuple.product!.DiscountPrice ?? tuple.product.Price;
                return (tuple.product.Id, tuple.line.Quantity, price);
            }).ToList();

        if (!cartLines.Any())
        {
            return new CheckoutResultViewModel
            {
                IsSuccess = false,
                Message = "No valid products were found in the cart."
            };
        }

        var subtotal = cartLines.Sum(i => i.price * i.Quantity);
        var initialOrderStatus = paymentOption.PaymentType == "manual" || paymentOption.PaymentType == "partner_bank"
            ? "Payment Review"
            : "Processing";
        var initialPaymentStatus = paymentOption.PaymentType == "manual"
            ? "Receipt Submitted"
            : paymentOption.PaymentType == "partner_bank"
                ? "Pending Confirmation"
                : "Paid";

        var payload = new CheckoutPayload
        {
            CustomerName = request.CustomerName,
            CustomerEmail = request.CustomerEmail,
            ShippingAddress = request.ShippingAddress,
            DeliveryOptionId = request.DeliveryOptionId,
            PaymentMethodId = request.PaymentMethodId,
            Status = initialOrderStatus,
            PaymentStatus = initialPaymentStatus,
            PaymentReceiptUrl = paymentReceiptUrl,
            PaymentReference = request.PaymentReference,
            Subtotal = subtotal,
            DeliveryFee = deliveryOption.Fee,
            ProcessingFee = paymentOption.ProcessingFee,
            Total = subtotal + deliveryOption.Fee + paymentOption.ProcessingFee,
            Items = cartLines.Select(l => (l.Id, l.Quantity, l.price)).ToList()
        };

        var (success, orderNumber) = await _repository.CreateOrderAsync(payload, cancellationToken);
        return new CheckoutResultViewModel
        {
            IsSuccess = success,
            OrderNumber = orderNumber,
            Message = success
                ? paymentOption.PaymentType == "manual"
                    ? "Order placed. Your receipt was submitted for payment review."
                    : "Thank you! Your order is confirmed."
                : "Something went wrong while creating the order."
        };
    }
}
