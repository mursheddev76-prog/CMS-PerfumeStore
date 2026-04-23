using System.Data;
using Dapper;
using Microsoft.Extensions.Options;
using Npgsql;
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
    Task UpsertCategoryAsync(Category category, CancellationToken cancellationToken);
    Task UpsertPaymentMethodAsync(PaymentMethod method, CancellationToken cancellationToken);
    Task UpsertDeliveryOptionAsync(DeliveryOption option, CancellationToken cancellationToken);
    Task UpsertHeroContentAsync(HeroContent hero, CancellationToken cancellationToken);
    Task<(bool Success, string? OrderNumber)> CreateOrderAsync(
        CheckoutPayload payload,
        CancellationToken cancellationToken);
    Task<User?> GetUserByUsernameAsync(string username, CancellationToken cancellationToken);
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

    public async Task UpsertCategoryAsync(Category category, CancellationToken cancellationToken)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        
        string sql;
        var parameters = new DynamicParameters();

        if (category.Id == 0)
        {
            // Insert new category (let database auto-generate id)
            sql = @"insert into categories (name, description, is_active)
                    values (@Name, @Description, @IsActive);";
        }
        else
        {
            // Update existing category
            sql = @"insert into categories (id, name, description, is_active)
                    values (@Id, @Name, @Description, @IsActive)
                    on conflict (id) do update set
                        name = excluded.name,
                        description = excluded.description,
                        is_active = excluded.is_active;";
            parameters.Add("Id", category.Id);
        }

        parameters.Add("Name", category.Name);
        parameters.Add("Description", category.Description);
        parameters.Add("IsActive", category.IsActive);

        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            parameters,
            commandType: CommandType.Text,
            cancellationToken: cancellationToken,
            commandTimeout: _commandTimeout));
    }

    public async Task<(bool Success, string? OrderNumber)> CreateOrderAsync(
        CheckoutPayload payload,
        CancellationToken cancellationToken)
    {
        await using var connection = (NpgsqlConnection)await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(
            $"""
            call {StoredProcedures.OrderCreate}(
                @p_customer_name,
                @p_customer_email,
                @p_shipping_address,
                @p_payment_method_id,
                @p_delivery_option_id,
                @p_subtotal,
                @p_delivery_fee,
                @p_processing_fee,
                @p_total,
                cast(@p_items as order_item_type[]),
                null
            )
            """,
            connection)
        {
            CommandType = CommandType.Text,
            CommandTimeout = _commandTimeout
        };

        command.Parameters.AddWithValue("p_customer_name", payload.CustomerName);
        command.Parameters.AddWithValue("p_customer_email", payload.CustomerEmail);
        command.Parameters.AddWithValue("p_shipping_address", payload.ShippingAddress);
        command.Parameters.AddWithValue("p_payment_method_id", payload.PaymentMethodId);
        command.Parameters.AddWithValue("p_delivery_option_id", payload.DeliveryOptionId);
        command.Parameters.AddWithValue("p_subtotal", payload.Subtotal);
        command.Parameters.AddWithValue("p_delivery_fee", payload.DeliveryFee);
        command.Parameters.AddWithValue("p_processing_fee", payload.ProcessingFee);
        command.Parameters.AddWithValue("p_total", payload.Total);

        command.Parameters.AddWithValue("p_items", payload.BuildItemsArrayLiteral());

        string? orderNumber = null;
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
        {
            orderNumber = reader.IsDBNull(0) ? null : reader.GetString(0);
        }

        return (string.IsNullOrWhiteSpace(orderNumber) is false, orderNumber);
    }

    public async Task<User?> GetUserByUsernameAsync(string username, CancellationToken cancellationToken)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_username", username);

        using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var command = new CommandDefinition(
            $"select * from {StoredProcedures.UserGetByUsername}(@p_username)",
            parameters,
            commandType: CommandType.Text,
            cancellationToken: cancellationToken,
            commandTimeout: _commandTimeout);
        
        return await connection.QuerySingleOrDefaultAsync<User>(command);
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

    public string BuildItemsArrayLiteral()
    {
        if (Items.Count == 0)
        {
            return "{}";
        }

        var values = Items.Select(item =>
            $"\"({item.ProductId},{item.Quantity},{item.UnitPrice.ToString(System.Globalization.CultureInfo.InvariantCulture)})\"");

        return $"{{{string.Join(",", values)}}}";
    }
}
