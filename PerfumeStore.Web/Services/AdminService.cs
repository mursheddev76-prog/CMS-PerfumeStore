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
        CancellationToken cancellationToken)
    {
        var statsTask = _repository.GetDashboardStatsAsync(cancellationToken);
        var productsTask = _repository.GetAllProductsAsync(cancellationToken);
        var categoriesTask = _repository.GetCategoriesAsync(cancellationToken);
        var paymentMethodsTask = _repository.GetPaymentMethodsAsync(cancellationToken);
        var deliveryOptionsTask = _repository.GetDeliveryOptionsAsync(cancellationToken);
        var heroTask = _repository.GetHeroContentAsync(cancellationToken);

        await Task.WhenAll(statsTask, productsTask, categoriesTask, paymentMethodsTask, deliveryOptionsTask, heroTask);

        var hero = heroTask.Result;
        var products = productsTask.Result;
        var categories = categoriesTask.Result;
        var paymentMethods = paymentMethodsTask.Result;
        var deliveryOptions = deliveryOptionsTask.Result;
        var selectedCategory = categories.FirstOrDefault(c => c.Id == categoryId);
        var selectedProduct = products.FirstOrDefault(p => p.Id == productId);
        var selectedPaymentMethod = paymentMethods.FirstOrDefault(p => p.Id == paymentMethodId);
        var selectedDeliveryOption = deliveryOptions.FirstOrDefault(d => d.Id == deliveryOptionId);

        return new AdminDashboardViewModel
        {
            Stats = statsTask.Result,
            Products = products,
            Categories = categories,
            PaymentMethods = paymentMethods,
            DeliveryOptions = deliveryOptions,
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
                    ProcessingFee = selectedPaymentMethod.ProcessingFee,
                    SupportsInstallments = selectedPaymentMethod.SupportsInstallments,
                    IsActive = selectedPaymentMethod.IsActive
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
                }
        };
    }

    public Task<AdminDashboardViewModel> BuildDashboardAsync(CancellationToken cancellationToken) =>
        BuildDashboardAsync(null, null, null, null, cancellationToken);

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
            input.ProcessingFee,
            input.SupportsInstallments,
            input.IsActive), cancellationToken);

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
}
