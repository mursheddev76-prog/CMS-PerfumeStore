using PerfumeStore.Web.Data.Repositories;
using PerfumeStore.Web.Models.Domain;
using PerfumeStore.Web.Models.ViewModels;

namespace PerfumeStore.Web.Services;

public class AdminService
{
    private readonly ICommerceRepository _repository;

    public AdminService(ICommerceRepository repository)
    {
        _repository = repository;
    }

    public async Task<AdminDashboardViewModel> BuildDashboardAsync(
        int? categoryId,
        int? productId,
        int? paymentMethodId,
        int? deliveryOptionId,
        int? userId,
        AdminOrderFiltersInput? orderFilters,
        CancellationToken cancellationToken)
    {
        var statsTask = _repository.GetDashboardStatsAsync(cancellationToken);
        var productsTask = _repository.GetAllProductsAsync(cancellationToken);
        var categoriesTask = _repository.GetCategoriesAsync(cancellationToken);
        var paymentMethodsTask = _repository.GetPaymentMethodsAsync(cancellationToken);
        var deliveryOptionsTask = _repository.GetDeliveryOptionsAsync(cancellationToken);
        var heroTask = _repository.GetHeroContentAsync(cancellationToken);
        var effectiveFilters = orderFilters ?? new AdminOrderFiltersInput();
        var recentOrdersTask = HasActiveOrderFilters(effectiveFilters)
            ? _repository.SearchOrdersAsync(effectiveFilters, cancellationToken)
            : _repository.GetRecentOrdersAsync(cancellationToken);
        var topCustomersTask = _repository.GetTopCustomersAsync(cancellationToken);
        var usersTask = _repository.GetUsersAsync(cancellationToken);

        await Task.WhenAll(statsTask, productsTask, categoriesTask, paymentMethodsTask, deliveryOptionsTask, heroTask, recentOrdersTask, topCustomersTask, usersTask);

        var hero = heroTask.Result;
        var products = productsTask.Result;
        var categories = categoriesTask.Result;
        var paymentMethods = paymentMethodsTask.Result;
        var deliveryOptions = deliveryOptionsTask.Result;
        var selectedCategory = categories.FirstOrDefault(c => c.Id == categoryId);
        var selectedProduct = products.FirstOrDefault(p => p.Id == productId);
        var selectedPaymentMethod = paymentMethods.FirstOrDefault(p => p.Id == paymentMethodId);
        var selectedDeliveryOption = deliveryOptions.FirstOrDefault(d => d.Id == deliveryOptionId);
        var selectedUser = usersTask.Result.FirstOrDefault(u => u.Id == userId);

        return new AdminDashboardViewModel
        {
            Stats = statsTask.Result,
            Products = products,
            Categories = categories,
            PaymentMethods = paymentMethods,
            DeliveryOptions = deliveryOptions,
            RecentOrders = recentOrdersTask.Result.Select(order => new AdminOrderSummaryViewModel
            {
                OrderNumber = order.OrderNumber,
                CustomerName = order.CustomerName,
                CustomerEmail = order.CustomerEmail,
                ShippingAddress = order.ShippingAddress,
                Status = order.Status,
                PaymentStatus = order.PaymentStatus,
                PaymentMethod = order.PaymentMethod,
                PaymentMethodId = order.PaymentMethodId,
                PaymentType = order.PaymentType,
                DeliveryOption = order.DeliveryOption,
                ItemCount = order.ItemCount,
                Total = order.Total,
                CreatedAt = order.CreatedAt,
                PaymentReceiptUrl = order.PaymentReceiptUrl,
                PaymentReference = order.PaymentReference,
                PaymentReviewNotes = order.PaymentReviewNotes
            }).ToList(),
            TopCustomers = topCustomersTask.Result.Select(customer => new AdminCustomerSummaryViewModel
            {
                FullName = string.IsNullOrWhiteSpace(customer.FullName) ? "Customer" : customer.FullName,
                Email = customer.Email,
                OrderCount = customer.OrderCount,
                LifetimeValue = customer.LifetimeValue,
                WishlistCount = customer.WishlistCount,
                LastOrderAt = customer.LastOrderAt
            }).ToList(),
            Users = usersTask.Result,
            Hero = new HeroSectionInput
            {
                Title = hero.Title,
                Subtitle = hero.Subtitle,
                BackgroundImageUrl = hero.BackgroundImageUrl,
                PrimaryCtaText = hero.PrimaryCtaText,
                PrimaryCtaLink = hero.PrimaryCtaLink,
                SecondaryCtaText = hero.SecondaryCtaText,
                SecondaryCtaLink = hero.SecondaryCtaLink
            },
            CategoryForm = selectedCategory is null
                ? new CategoryInput()
                : new CategoryInput
                {
                    Id = selectedCategory.Id,
                    Name = selectedCategory.Name,
                    Description = selectedCategory.Description,
                    IsActive = selectedCategory.IsActive
                },
            ProductForm = selectedProduct is null
                ? new ProductInput()
                : new ProductInput
                {
                    Id = selectedProduct.Id,
                    Name = selectedProduct.Name,
                    Description = selectedProduct.Description,
                    Price = selectedProduct.Price,
                    DiscountPrice = selectedProduct.DiscountPrice,
                    CategoryId = selectedProduct.CategoryId,
                    StockQuantity = selectedProduct.StockQuantity,
                    ImageUrl = selectedProduct.ImageUrl,
                    IsFeatured = selectedProduct.IsFeatured,
                    IsTrending = selectedProduct.IsTrending
                },
            PaymentForm = selectedPaymentMethod is null
                ? new PaymentMethodInput()
                : new PaymentMethodInput
                {
                    Id = selectedPaymentMethod.Id,
                    Name = selectedPaymentMethod.Name,
                    Provider = selectedPaymentMethod.Provider,
                    PaymentType = selectedPaymentMethod.PaymentType,
                    PartnerName = selectedPaymentMethod.PartnerName,
                    ProcessingFee = selectedPaymentMethod.ProcessingFee,
                    SupportsInstallments = selectedPaymentMethod.SupportsInstallments,
                    IsActive = selectedPaymentMethod.IsActive,
                    AccountTitle = selectedPaymentMethod.AccountTitle,
                    AccountNumber = selectedPaymentMethod.AccountNumber,
                    BankName = selectedPaymentMethod.BankName,
                    Iban = selectedPaymentMethod.Iban,
                    Instructions = selectedPaymentMethod.Instructions,
                    RequiresReceipt = selectedPaymentMethod.RequiresReceipt
                },
            DeliveryForm = selectedDeliveryOption is null
                ? new DeliveryOptionInput()
                : new DeliveryOptionInput
                {
                    Id = selectedDeliveryOption.Id,
                    Name = selectedDeliveryOption.Name,
                    Description = selectedDeliveryOption.Description,
                    Fee = selectedDeliveryOption.Fee,
                    EstimatedDays = selectedDeliveryOption.EstimatedDays,
                    IsActive = selectedDeliveryOption.IsActive
                },
            OrderReviewForm = recentOrdersTask.Result.FirstOrDefault() is { } order
                ? new OrderReviewInput
                {
                    OrderNumber = order.OrderNumber,
                    Status = order.Status,
                    PaymentStatus = order.PaymentStatus,
                    PaymentReviewNotes = order.PaymentReviewNotes
                }
                : new OrderReviewInput(),
            UserForm = selectedUser is null
                ? new UserManagementInput()
                : new UserManagementInput
                {
                    Id = selectedUser.Id,
                    FullName = selectedUser.FullName,
                    Username = selectedUser.Username,
                    Role = selectedUser.Role,
                    IsActive = selectedUser.IsActive
                },
            OrderFilters = effectiveFilters
        };
    }

    public Task<AdminDashboardViewModel> BuildDashboardAsync(CancellationToken cancellationToken) =>
        BuildDashboardAsync(null, null, null, null, null, null, cancellationToken);

    public Task SaveHeroAsync(HeroSectionInput input, CancellationToken cancellationToken) =>
        _repository.UpsertHeroContentAsync(new HeroContent(
            input.Title,
            input.Subtitle,
            input.BackgroundImageUrl,
            input.PrimaryCtaText,
            input.PrimaryCtaLink,
            input.SecondaryCtaText,
            input.SecondaryCtaLink), cancellationToken);

    public Task SaveProductAsync(ProductInput input, string categoryName, CancellationToken cancellationToken) =>
        _repository.UpsertProductAsync(new Product(
            input.Id,
            input.Name,
            input.Description,
            input.Price,
            input.DiscountPrice,
            input.ImageUrl,
            input.IsFeatured,
            input.IsTrending,
            input.CategoryId,
            categoryName,
            input.StockQuantity), cancellationToken);

    public Task SavePaymentMethodAsync(PaymentMethodInput input, CancellationToken cancellationToken) =>
        _repository.UpsertPaymentMethodAsync(new PaymentMethod(
            input.Id,
            input.Name,
            input.Provider,
            input.PaymentType,
            input.PartnerName,
            input.ProcessingFee,
            input.SupportsInstallments,
            input.IsActive,
            input.AccountTitle,
            input.AccountNumber,
            input.BankName,
            input.Iban,
            input.Instructions,
            input.RequiresReceipt), cancellationToken);

    public Task SaveDeliveryOptionAsync(DeliveryOptionInput input, CancellationToken cancellationToken) =>
        _repository.UpsertDeliveryOptionAsync(new DeliveryOption(
            input.Id,
            input.Name,
            input.Description,
            input.Fee,
            input.EstimatedDays,
            input.IsActive), cancellationToken);

    public Task SaveCategoryAsync(CategoryInput input, CancellationToken cancellationToken)
        => _repository.UpsertCategoryAsync(new Models.Domain.Category(
            input.Id,
            input.Name,
            input.Description ?? string.Empty,
            input.IsActive), cancellationToken);

    public Task UpdateOrderReviewAsync(OrderReviewInput input, CancellationToken cancellationToken) =>
        _repository.UpdateOrderReviewAsync(input.OrderNumber, input.Status, input.PaymentStatus, input.PaymentReviewNotes, cancellationToken);

    public Task SaveUserAsync(UserManagementInput input, CancellationToken cancellationToken) =>
        _repository.UpsertUserAsync(input, cancellationToken);

    public Task SetCategoryActiveAsync(int id, bool isActive, CancellationToken cancellationToken) =>
        _repository.SetCategoryActiveAsync(id, isActive, cancellationToken);

    public Task DeleteCategoryAsync(int id, CancellationToken cancellationToken) =>
        _repository.DeleteCategoryAsync(id, cancellationToken);

    public Task DeleteProductAsync(int id, CancellationToken cancellationToken) =>
        _repository.DeleteProductAsync(id, cancellationToken);

    public Task SetPaymentMethodActiveAsync(int id, bool isActive, CancellationToken cancellationToken) =>
        _repository.SetPaymentMethodActiveAsync(id, isActive, cancellationToken);

    public Task DeletePaymentMethodAsync(int id, CancellationToken cancellationToken) =>
        _repository.DeletePaymentMethodAsync(id, cancellationToken);

    public Task SetDeliveryOptionActiveAsync(int id, bool isActive, CancellationToken cancellationToken) =>
        _repository.SetDeliveryOptionActiveAsync(id, isActive, cancellationToken);

    public Task DeleteDeliveryOptionAsync(int id, CancellationToken cancellationToken) =>
        _repository.DeleteDeliveryOptionAsync(id, cancellationToken);

    public Task SetUserActiveAsync(int id, bool isActive, CancellationToken cancellationToken) =>
        _repository.SetUserActiveAsync(id, isActive, cancellationToken);

    public Task DeleteUserAsync(int id, CancellationToken cancellationToken) =>
        _repository.DeleteUserAsync(id, cancellationToken);

    public AdminDashboardViewModel WithCategoryForm(AdminDashboardViewModel model, CategoryInput input)
    {
        model.CategoryForm = input;
        return model;
    }

    public AdminDashboardViewModel WithProductForm(AdminDashboardViewModel model, ProductInput input)
    {
        model.ProductForm = input;
        return model;
    }

    public AdminDashboardViewModel WithPaymentForm(AdminDashboardViewModel model, PaymentMethodInput input)
    {
        model.PaymentForm = input;
        return model;
    }

    public AdminDashboardViewModel WithDeliveryForm(AdminDashboardViewModel model, DeliveryOptionInput input)
    {
        model.DeliveryForm = input;
        return model;
    }

    public AdminDashboardViewModel WithOrderStatusForm(AdminDashboardViewModel model, OrderReviewInput input)
    {
        model.OrderReviewForm = input;
        return model;
    }

    public AdminDashboardViewModel WithUserForm(AdminDashboardViewModel model, UserManagementInput input)
    {
        model.UserForm = input;
        return model;
    }

    private static bool HasActiveOrderFilters(AdminOrderFiltersInput filters) =>
        !string.IsNullOrWhiteSpace(filters.Query)
        || !string.IsNullOrWhiteSpace(filters.Status)
        || !string.IsNullOrWhiteSpace(filters.PaymentStatus)
        || filters.PaymentMethodId.HasValue
        || filters.DateFrom.HasValue
        || filters.DateTo.HasValue;
}
