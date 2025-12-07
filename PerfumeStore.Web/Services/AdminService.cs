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

    public async Task<AdminDashboardViewModel> BuildDashboardAsync(CancellationToken cancellationToken)
    {
        var statsTask = _repository.GetDashboardStatsAsync(cancellationToken);
        var productsTask = _repository.GetAllProductsAsync(cancellationToken);
        var categoriesTask = _repository.GetCategoriesAsync(cancellationToken);
        var paymentMethodsTask = _repository.GetPaymentMethodsAsync(cancellationToken);
        var deliveryOptionsTask = _repository.GetDeliveryOptionsAsync(cancellationToken);
        var heroTask = _repository.GetHeroContentAsync(cancellationToken);

        await Task.WhenAll(statsTask, productsTask, categoriesTask, paymentMethodsTask, deliveryOptionsTask, heroTask);

        var hero = heroTask.Result;

        return new AdminDashboardViewModel
        {
            Stats = statsTask.Result,
            Products = productsTask.Result,
            Categories = categoriesTask.Result,
            PaymentMethods = paymentMethodsTask.Result,
            DeliveryOptions = deliveryOptionsTask.Result,
            Hero = new HeroSectionInput
            {
                Title = hero.Title,
                Subtitle = hero.Subtitle,
                BackgroundImageUrl = hero.BackgroundImageUrl,
                PrimaryCtaText = hero.PrimaryCtaText,
                PrimaryCtaLink = hero.PrimaryCtaLink,
                SecondaryCtaText = hero.SecondaryCtaText,
                SecondaryCtaLink = hero.SecondaryCtaLink
            }
        };
    }

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
}

