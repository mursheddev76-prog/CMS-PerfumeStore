using System.Data;
using Dapper;
using Microsoft.Extensions.Options;
using PerfumeStore.Web.Models.Domain;

namespace PerfumeStore.Web.Data.Repositories;

public interface ICommerceRepository
{
    Task<IReadOnlyList<Product>> GetFeaturedProductsAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<Product>> GetTrendingProductsAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<Product>> GetAllProductsAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<Category>> GetCategoriesAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<PaymentMethod>> GetPaymentMethodsAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<DeliveryOption>> GetDeliveryOptionsAsync(CancellationToken cancellationToken);
    Task<HeroContent> GetHeroContentAsync(CancellationToken cancellationToken);
    Task<AdminDashboardStats> GetDashboardStatsAsync(CancellationToken cancellationToken);
    Task UpsertProductAsync(Product product, CancellationToken cancellationToken);
    Task UpsertPaymentMethodAsync(PaymentMethod method, CancellationToken cancellationToken);
    Task UpsertDeliveryOptionAsync(DeliveryOption option, CancellationToken cancellationToken);
    Task UpsertHeroContentAsync(HeroContent hero, CancellationToken cancellationToken);
    Task<(bool Success, string? OrderNumber)> CreateOrderAsync(
        CheckoutPayload payload,
        CancellationToken cancellationToken);
}

public sealed class CommerceRepository : ICommerceRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly int _commandTimeout;

    public CommerceRepository(IDbConnectionFactory connectionFactory, IOptions<DatabaseOptions> options)
    {
        _connectionFactory = connectionFactory;
        _commandTimeout = options.Value.CommandTimeoutSeconds;
    }

    public async Task<IReadOnlyList<Product>> GetFeaturedProductsAsync(CancellationToken cancellationToken) =>
        (await QueryProductsAsync(StoredProcedures.ProductsGetFeatured, cancellationToken)).ToList();

    public async Task<IReadOnlyList<Product>> GetTrendingProductsAsync(CancellationToken cancellationToken) =>
        (await QueryProductsAsync(StoredProcedures.ProductsGetTrending, cancellationToken)).ToList();

    public async Task<IReadOnlyList<Product>> GetAllProductsAsync(CancellationToken cancellationToken) =>
        (await QueryProductsAsync(StoredProcedures.ProductsGetAll, cancellationToken)).ToList();

    public async Task<IReadOnlyList<Category>> GetCategoriesAsync(CancellationToken cancellationToken)
        => await QueryFunctionAsync<Category>(StoredProcedures.CategoriesGetAll, cancellationToken);

    public async Task<IReadOnlyList<PaymentMethod>> GetPaymentMethodsAsync(CancellationToken cancellationToken)
        => await QueryFunctionAsync<PaymentMethod>(StoredProcedures.PaymentMethodsGetAll, cancellationToken);

    public async Task<IReadOnlyList<DeliveryOption>> GetDeliveryOptionsAsync(CancellationToken cancellationToken)
        => await QueryFunctionAsync<DeliveryOption>(StoredProcedures.DeliveryOptionsGetAll, cancellationToken);

    public async Task<HeroContent> GetHeroContentAsync(CancellationToken cancellationToken)
    {
        var hero = await QuerySingleOrDefaultAsync<HeroContent>(StoredProcedures.HeroContentGet, cancellationToken);
        return hero ?? new HeroContent(
            "Signature scents, delivered fast.",
            "Craft bespoke fragrance rituals with curated capsule collections.",
            "/images/hero-default.jpg",
            "Explore Catalog",
            "/catalog",
            "Book Consultation",
            "/admin");
    }

    public async Task<AdminDashboardStats> GetDashboardStatsAsync(CancellationToken cancellationToken)
    {
        var stats = await QuerySingleOrDefaultAsync<AdminDashboardStats>(StoredProcedures.AdminDashboardStats, cancellationToken);
        return stats ?? new AdminDashboardStats(0, 0, 0, 0, 0);
    }

    public async Task UpsertProductAsync(Product product, CancellationToken cancellationToken)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_id", product.Id);
        parameters.Add("p_name", product.Name);
        parameters.Add("p_description", product.Description);
        parameters.Add("p_price", product.Price);
        parameters.Add("p_discount_price", product.DiscountPrice);
        parameters.Add("p_image_url", product.ImageUrl);
        parameters.Add("p_is_featured", product.IsFeatured);
        parameters.Add("p_is_trending", product.IsTrending);
        parameters.Add("p_category_id", product.CategoryId);
        parameters.Add("p_stock_quantity", product.StockQuantity);

        await ExecuteAsync(StoredProcedures.ProductUpsert, parameters, cancellationToken);
    }

    public async Task UpsertPaymentMethodAsync(PaymentMethod method, CancellationToken cancellationToken)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_id", method.Id);
        parameters.Add("p_name", method.Name);
        parameters.Add("p_provider", method.Provider);
        parameters.Add("p_processing_fee", method.ProcessingFee);
        parameters.Add("p_supports_installments", method.SupportsInstallments);
        parameters.Add("p_is_active", method.IsActive);

        await ExecuteAsync(StoredProcedures.PaymentMethodUpsert, parameters, cancellationToken);
    }

    public async Task UpsertDeliveryOptionAsync(DeliveryOption option, CancellationToken cancellationToken)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_id", option.Id);
        parameters.Add("p_name", option.Name);
        parameters.Add("p_description", option.Description);
        parameters.Add("p_fee", option.Fee);
        parameters.Add("p_estimated_days", option.EstimatedDays);
        parameters.Add("p_is_active", option.IsActive);

        await ExecuteAsync(StoredProcedures.DeliveryOptionUpsert, parameters, cancellationToken);
    }

    public async Task UpsertHeroContentAsync(HeroContent hero, CancellationToken cancellationToken)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_title", hero.Title);
        parameters.Add("p_subtitle", hero.Subtitle);
        parameters.Add("p_background_image_url", hero.BackgroundImageUrl);
        parameters.Add("p_primary_cta_text", hero.PrimaryCtaText);
        parameters.Add("p_primary_cta_link", hero.PrimaryCtaLink);
        parameters.Add("p_secondary_cta_text", hero.SecondaryCtaText);
        parameters.Add("p_secondary_cta_link", hero.SecondaryCtaLink);

        await ExecuteAsync(StoredProcedures.HeroContentUpsert, parameters, cancellationToken);
    }

    public async Task<(bool Success, string? OrderNumber)> CreateOrderAsync(
        CheckoutPayload payload,
        CancellationToken cancellationToken)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_customer_name", payload.CustomerName);
        parameters.Add("p_customer_email", payload.CustomerEmail);
        parameters.Add("p_shipping_address", payload.ShippingAddress);
        parameters.Add("p_payment_method_id", payload.PaymentMethodId);
        parameters.Add("p_delivery_option_id", payload.DeliveryOptionId);
        parameters.Add("p_subtotal", payload.Subtotal);
        parameters.Add("p_delivery_fee", payload.DeliveryFee);
        parameters.Add("p_processing_fee", payload.ProcessingFee);
        parameters.Add("p_total", payload.Total);
        parameters.Add("p_items", payload.BuildItemsTableValuedParameter());
        parameters.Add("p_order_number", dbType: DbType.String, size: 32, direction: ParameterDirection.Output);

        await ExecuteAsync(StoredProcedures.OrderCreate, parameters, cancellationToken);
        var orderNumber = parameters.Get<string>("p_order_number");
        return (string.IsNullOrWhiteSpace(orderNumber) is false, orderNumber);
    }

    private async Task<IEnumerable<Product>> QueryProductsAsync(string functionName, CancellationToken cancellationToken)
        => await QueryFunctionAsync<Product>(functionName, cancellationToken);

    private async Task<IReadOnlyList<T>> QueryFunctionAsync<T>(string functionName, CancellationToken cancellationToken)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var command = BuildFunctionCommand(functionName, cancellationToken);
        var results = await connection.QueryAsync<T>(command);
        return results.ToList();
    }

    private async Task<T?> QuerySingleOrDefaultAsync<T>(string functionName, CancellationToken cancellationToken)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var command = BuildFunctionCommand(functionName, cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<T>(command);
    }

    private CommandDefinition BuildFunctionCommand(string functionName, CancellationToken cancellationToken) =>
        new(
            $"select * from {functionName}()",
            commandType: CommandType.Text,
            cancellationToken: cancellationToken,
            commandTimeout: _commandTimeout);

    private async Task ExecuteAsync(string storedProcedure, DynamicParameters parameters, CancellationToken cancellationToken)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(
            new CommandDefinition(
                storedProcedure,
                parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken,
                commandTimeout: _commandTimeout));
    }
}

public sealed class CheckoutPayload
{
    public required string CustomerName { get; init; }
    public required string CustomerEmail { get; init; }
    public required string ShippingAddress { get; init; }
    public required int PaymentMethodId { get; init; }
    public required int DeliveryOptionId { get; init; }
    public required decimal Subtotal { get; init; }
    public required decimal DeliveryFee { get; init; }
    public required decimal ProcessingFee { get; init; }
    public required decimal Total { get; init; }
    public required IReadOnlyCollection<(int ProductId, int Quantity, decimal UnitPrice)> Items { get; init; }

    public DataTable BuildItemsTableValuedParameter()
    {
        var table = new DataTable();
        table.Columns.Add("product_id", typeof(int));
        table.Columns.Add("quantity", typeof(int));
        table.Columns.Add("unit_price", typeof(decimal));

        foreach (var item in Items)
        {
            table.Rows.Add(item.ProductId, item.Quantity, item.UnitPrice);
        }

        return table;
    }
}

