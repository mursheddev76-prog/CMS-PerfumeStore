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

    public async Task<CheckoutPageViewModel> BuildCheckoutAsync(CancellationToken cancellationToken)
    {
        var productsTask = _repository.GetFeaturedProductsAsync(cancellationToken);
        var paymentsTask = _repository.GetPaymentMethodsAsync(cancellationToken);
        var deliveryTask = _repository.GetDeliveryOptionsAsync(cancellationToken);

        await Task.WhenAll(productsTask, paymentsTask, deliveryTask);

        var randomCart = productsTask.Result.Take(3).Select(p => new ProductCardViewModel
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Category = p.CategoryName,
            ImageUrl = p.ImageUrl,
            Price = p.Price,
            DiscountPrice = p.DiscountPrice,
            IsNewArrival = p.IsFeatured
        }).ToList();

        var subtotal = randomCart.Sum(p => p.DiscountPrice ?? p.Price);
        return new CheckoutPageViewModel
        {
            CartProducts = randomCart,
            PaymentMethods = paymentsTask.Result.Where(p => p.IsActive).ToList(),
            DeliveryOptions = deliveryTask.Result.Where(d => d.IsActive).ToList(),
            Subtotal = subtotal,
            EstimatedTotal = subtotal + deliveryTask.Result.FirstOrDefault(d => d.IsActive)?.Fee ?? 0,
            Request = new CheckoutRequest
            {
                Items = randomCart.Select(p => new CheckoutLine
                {
                    ProductId = p.Id,
                    Quantity = 1
                }).ToList()
            }
        };
    }

    public async Task<CheckoutResultViewModel> PlaceOrderAsync(CheckoutRequest request, CancellationToken cancellationToken)
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

        var cartLines = request.Items
            .Select(line => (line, product: products.FirstOrDefault(p => p.Id == line.ProductId)))
            .Where(tuple => tuple.product is not null)
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
        var payload = new CheckoutPayload
        {
            CustomerName = request.CustomerName,
            CustomerEmail = request.CustomerEmail,
            ShippingAddress = request.ShippingAddress,
            DeliveryOptionId = request.DeliveryOptionId,
            PaymentMethodId = request.PaymentMethodId,
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
                ? "Thank you! Your order is confirmed."
                : "Something went wrong while creating the order."
        };
    }
}

