using PerfumeStore.Web.Data.Repositories;
using PerfumeStore.Web.Models.ViewModels;

namespace PerfumeStore.Web.Services;

public class CustomerService
{
    private readonly ICommerceRepository _repository;

    public CustomerService(ICommerceRepository repository)
    {
        _repository = repository;
    }

    public async Task<CustomerDashboardViewModel?> BuildDashboardAsync(string username, CancellationToken cancellationToken)
    {
        var user = await _repository.GetUserByUsernameAsync(username, cancellationToken);
        if (user is null)
        {
            return null;
        }

        var ordersTask = _repository.GetCustomerOrdersByEmailAsync(user.Username ?? username, cancellationToken);
        var wishlistTask = _repository.GetWishlistProductsAsync(username, cancellationToken);

        await Task.WhenAll(ordersTask, wishlistTask);

        return new CustomerDashboardViewModel
        {
            FullName = user.FullName ?? string.Empty,
            Email = user.Username ?? username,
            Orders = ordersTask.Result.Select(order => new CustomerOrderViewModel
            {
                OrderNumber = order.OrderNumber,
                Status = order.Status,
                PaymentStatus = order.PaymentStatus,
                PaymentMethod = order.PaymentMethod,
                DeliveryOption = order.DeliveryOption,
                EstimatedDays = order.EstimatedDays,
                ItemCount = order.ItemCount,
                Total = order.Total,
                CreatedAt = order.CreatedAt,
                PaymentReference = order.PaymentReference,
                PaymentReceiptUrl = order.PaymentReceiptUrl
            }).ToList(),
            Wishlist = wishlistTask.Result.Select(product => new ProductCardViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Category = product.CategoryName,
                ImageUrl = product.ImageUrl,
                Price = product.Price,
                DiscountPrice = product.DiscountPrice,
                IsNewArrival = product.IsFeatured,
                IsWishlisted = true,
                StockQuantity = product.StockQuantity
            }).ToList()
        };
    }
}
