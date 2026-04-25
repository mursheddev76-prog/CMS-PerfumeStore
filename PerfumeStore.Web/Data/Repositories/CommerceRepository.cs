using System.Data;
using Dapper;
using Microsoft.Extensions.Options;
using Npgsql;
using PerfumeStore.Web.Models.Domain;
using PerfumeStore.Web.Models.ViewModels;

namespace PerfumeStore.Web.Data.Repositories;

public interface ICommerceRepository
{
    Task<IReadOnlyList<Product>> GetFeaturedProductsAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<Product>> GetTrendingProductsAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<Product>> GetAllProductsAsync(CancellationToken cancellationToken);
    Task<Product?> GetProductByIdAsync(int id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Category>> GetCategoriesAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<PaymentMethod>> GetPaymentMethodsAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<DeliveryOption>> GetDeliveryOptionsAsync(CancellationToken cancellationToken);
    Task<HeroContent> GetHeroContentAsync(CancellationToken cancellationToken);
    Task<AdminDashboardStats> GetDashboardStatsAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<AdminOrderSummary>> GetRecentOrdersAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<AdminCustomerSummary>> GetTopCustomersAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<AdminOrderSummary>> SearchOrdersAsync(AdminOrderFiltersInput filters, CancellationToken cancellationToken);
    Task<IReadOnlyList<AppUserSummary>> GetUsersAsync(CancellationToken cancellationToken);
    Task UpsertProductAsync(Product product, CancellationToken cancellationToken);
    Task UpsertCategoryAsync(Category category, CancellationToken cancellationToken);
    Task UpsertPaymentMethodAsync(PaymentMethod method, CancellationToken cancellationToken);
    Task UpsertDeliveryOptionAsync(DeliveryOption option, CancellationToken cancellationToken);
    Task UpsertHeroContentAsync(HeroContent hero, CancellationToken cancellationToken);
    Task UpdateOrderReviewAsync(string orderNumber, string status, string paymentStatus, string? notes, CancellationToken cancellationToken);
    Task SetCategoryActiveAsync(int id, bool isActive, CancellationToken cancellationToken);
    Task DeleteCategoryAsync(int id, CancellationToken cancellationToken);
    Task DeleteProductAsync(int id, CancellationToken cancellationToken);
    Task SetPaymentMethodActiveAsync(int id, bool isActive, CancellationToken cancellationToken);
    Task DeletePaymentMethodAsync(int id, CancellationToken cancellationToken);
    Task SetDeliveryOptionActiveAsync(int id, bool isActive, CancellationToken cancellationToken);
    Task DeleteDeliveryOptionAsync(int id, CancellationToken cancellationToken);
    Task<(bool Success, string? OrderNumber)> CreateOrderAsync(
        CheckoutPayload payload,
        CancellationToken cancellationToken);
    Task<User?> GetUserByUsernameAsync(string username, CancellationToken cancellationToken);
    Task<int> CreateUserAsync(string fullName, string username, string passwordHash, CancellationToken cancellationToken);
    Task UpsertUserAsync(UserManagementInput input, CancellationToken cancellationToken);
    Task SetUserActiveAsync(int id, bool isActive, CancellationToken cancellationToken);
    Task DeleteUserAsync(int id, CancellationToken cancellationToken);
    Task<IReadOnlyList<CustomerOrderSummary>> GetCustomerOrdersByEmailAsync(string email, CancellationToken cancellationToken);
    Task<IReadOnlyList<int>> GetWishlistProductIdsAsync(string username, CancellationToken cancellationToken);
    Task<IReadOnlyList<Product>> GetWishlistProductsAsync(string username, CancellationToken cancellationToken);
    Task AddWishlistItemAsync(string username, int productId, CancellationToken cancellationToken);
    Task RemoveWishlistItemAsync(string username, int productId, CancellationToken cancellationToken);
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

    public async Task<Product?> GetProductByIdAsync(int id, CancellationToken cancellationToken)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var command = new CommandDefinition(
            """
            select
                p.id as Id,
                p.name as Name,
                p.description as Description,
                p.price as Price,
                p.discount_price as DiscountPrice,
                p.image_url as ImageUrl,
                p.is_featured as IsFeatured,
                p.is_trending as IsTrending,
                p.category_id as CategoryId,
                c.name as CategoryName,
                p.stock_quantity as StockQuantity
            from products p
            join categories c on c.id = p.category_id
            where p.id = @Id
            """,
            new { Id = id },
            commandType: CommandType.Text,
            cancellationToken: cancellationToken,
            commandTimeout: _commandTimeout);

        return await connection.QuerySingleOrDefaultAsync<Product>(command);
    }

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

    public async Task<IReadOnlyList<AdminOrderSummary>> GetRecentOrdersAsync(CancellationToken cancellationToken)
        => await QueryFunctionAsync<AdminOrderSummary>(StoredProcedures.AdminOrdersGetRecent, cancellationToken);

    public async Task<IReadOnlyList<AdminCustomerSummary>> GetTopCustomersAsync(CancellationToken cancellationToken)
        => await QueryFunctionAsync<AdminCustomerSummary>(StoredProcedures.AdminCustomersGetTop, cancellationToken);

    public async Task<IReadOnlyList<AdminOrderSummary>> SearchOrdersAsync(AdminOrderFiltersInput filters, CancellationToken cancellationToken)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var command = new CommandDefinition(
            """
            select
                o.order_number as OrderNumber,
                o.customer_name as CustomerName,
                o.customer_email as CustomerEmail,
                o.shipping_address as ShippingAddress,
                o.status as Status,
                o.payment_status as PaymentStatus,
                pm.name as PaymentMethod,
                pm.id as PaymentMethodId,
                pm.payment_type as PaymentType,
                d.name as DeliveryOption,
                coalesce(sum(oi.quantity), 0)::int as ItemCount,
                o.total as Total,
                o.created_at as CreatedAt,
                o.payment_receipt_url as PaymentReceiptUrl,
                o.payment_reference as PaymentReference,
                o.payment_review_notes as PaymentReviewNotes
            from orders o
            join payment_methods pm on pm.id = o.payment_method_id
            join delivery_options d on d.id = o.delivery_option_id
            left join order_items oi on oi.order_id = o.id
            where (
                @Query is null
                or o.order_number ilike @Pattern
                or o.customer_name ilike @Pattern
                or o.customer_email ilike @Pattern
            )
            and (@Status is null or o.status = @Status)
            and (@PaymentStatus is null or o.payment_status = @PaymentStatus)
            and (@PaymentMethodId is null or o.payment_method_id = @PaymentMethodId)
            and (@DateFrom is null or o.created_at::date >= @DateFrom)
            and (@DateTo is null or o.created_at::date <= @DateTo)
            group by o.order_number, o.customer_name, o.customer_email, o.shipping_address, o.status, o.payment_status, pm.name, pm.id, pm.payment_type, d.name, o.total, o.created_at, o.payment_receipt_url, o.payment_reference, o.payment_review_notes
            order by o.created_at desc
            limit 30;
            """,
            new
            {
                Query = string.IsNullOrWhiteSpace(filters.Query) ? null : filters.Query.Trim(),
                Pattern = $"%{filters.Query?.Trim()}%",
                filters.Status,
                filters.PaymentStatus,
                filters.PaymentMethodId,
                DateFrom = filters.DateFrom?.Date,
                DateTo = filters.DateTo?.Date
            },
            commandType: CommandType.Text,
            cancellationToken: cancellationToken,
            commandTimeout: _commandTimeout);

        var results = await connection.QueryAsync<AdminOrderSummary>(command);
        return results.ToList();
    }

    public async Task<IReadOnlyList<AppUserSummary>> GetUsersAsync(CancellationToken cancellationToken)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var command = new CommandDefinition(
            """
            select
                id as Id,
                coalesce(nullif(full_name, ''), username) as FullName,
                username as Username,
                role as Role,
                is_active as IsActive,
                created_at as CreatedAt
            from app_users
            order by
                case when role = 'customer' then 1 else 0 end,
                created_at desc;
            """,
            commandType: CommandType.Text,
            cancellationToken: cancellationToken,
            commandTimeout: _commandTimeout);

        var users = await connection.QueryAsync<AppUserSummary>(command);
        return users.ToList();
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
        parameters.Add("p_payment_type", method.PaymentType);
        parameters.Add("p_partner_name", method.PartnerName);
        parameters.Add("p_processing_fee", method.ProcessingFee);
        parameters.Add("p_supports_installments", method.SupportsInstallments);
        parameters.Add("p_is_active", method.IsActive);
        parameters.Add("p_account_title", method.AccountTitle);
        parameters.Add("p_account_number", method.AccountNumber);
        parameters.Add("p_bank_name", method.BankName);
        parameters.Add("p_iban", method.Iban);
        parameters.Add("p_instructions", method.Instructions);
        parameters.Add("p_requires_receipt", method.RequiresReceipt);

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

    public async Task UpdateOrderReviewAsync(string orderNumber, string status, string paymentStatus, string? notes, CancellationToken cancellationToken)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_order_number", orderNumber);
        parameters.Add("p_status", status);
        parameters.Add("p_payment_status", paymentStatus);
        parameters.Add("p_payment_review_notes", notes);

        await ExecuteAsync(StoredProcedures.OrderReviewUpdate, parameters, cancellationToken);
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

    public Task SetCategoryActiveAsync(int id, bool isActive, CancellationToken cancellationToken) =>
        ExecuteSqlAsync(
            """
            update categories
            set is_active = @IsActive
            where id = @Id;
            """,
            new { Id = id, IsActive = isActive },
            cancellationToken);

    public Task DeleteCategoryAsync(int id, CancellationToken cancellationToken) =>
        ExecuteSqlAsync(
            """
            delete from categories
            where id = @Id;
            """,
            new { Id = id },
            cancellationToken);

    public Task DeleteProductAsync(int id, CancellationToken cancellationToken) =>
        ExecuteSqlAsync(
            """
            delete from products
            where id = @Id;
            """,
            new { Id = id },
            cancellationToken);

    public Task SetPaymentMethodActiveAsync(int id, bool isActive, CancellationToken cancellationToken) =>
        ExecuteSqlAsync(
            """
            update payment_methods
            set is_active = @IsActive
            where id = @Id;
            """,
            new { Id = id, IsActive = isActive },
            cancellationToken);

    public Task DeletePaymentMethodAsync(int id, CancellationToken cancellationToken) =>
        ExecuteSqlAsync(
            """
            delete from payment_methods
            where id = @Id;
            """,
            new { Id = id },
            cancellationToken);

    public Task SetDeliveryOptionActiveAsync(int id, bool isActive, CancellationToken cancellationToken) =>
        ExecuteSqlAsync(
            """
            update delivery_options
            set is_active = @IsActive
            where id = @Id;
            """,
            new { Id = id, IsActive = isActive },
            cancellationToken);

    public Task DeleteDeliveryOptionAsync(int id, CancellationToken cancellationToken) =>
        ExecuteSqlAsync(
            """
            delete from delivery_options
            where id = @Id;
            """,
            new { Id = id },
            cancellationToken);

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
                @p_status,
                @p_payment_status,
                @p_payment_receipt_url,
                @p_payment_reference,
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
        command.Parameters.AddWithValue("p_status", payload.Status);
        command.Parameters.AddWithValue("p_payment_status", payload.PaymentStatus);
        command.Parameters.AddWithValue("p_payment_receipt_url", (object?)payload.PaymentReceiptUrl ?? DBNull.Value);
        command.Parameters.AddWithValue("p_payment_reference", (object?)payload.PaymentReference ?? DBNull.Value);
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
            new { p_username = NormalizeUsername(username) },
            commandType: CommandType.Text,
            cancellationToken: cancellationToken,
            commandTimeout: _commandTimeout);
        
        return await connection.QuerySingleOrDefaultAsync<User>(command);
    }

    public async Task<int> CreateUserAsync(string fullName, string username, string passwordHash, CancellationToken cancellationToken)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var command = new CommandDefinition(
            """
            insert into app_users(full_name, username, password_hash, role)
            values (@FullName, @Username, @PasswordHash, 'customer')
            returning id;
            """,
            new
            {
                FullName = fullName,
                Username = NormalizeUsername(username),
                PasswordHash = passwordHash
            },
            commandType: CommandType.Text,
            cancellationToken: cancellationToken,
            commandTimeout: _commandTimeout);

        return await connection.ExecuteScalarAsync<int>(command);
    }

    public async Task UpsertUserAsync(UserManagementInput input, CancellationToken cancellationToken)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        if (input.Id == 0)
        {
            var command = new CommandDefinition(
                """
                insert into app_users(full_name, username, password_hash, role, is_active)
                values (@FullName, @Username, @PasswordHash, @Role, @IsActive);
                """,
                new
                {
                    FullName = input.FullName.Trim(),
                    Username = NormalizeUsername(input.Username),
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(input.Password ?? "password123"),
                    input.Role,
                    input.IsActive
                },
                commandType: CommandType.Text,
                cancellationToken: cancellationToken,
                commandTimeout: _commandTimeout);

            await connection.ExecuteAsync(command);
            return;
        }

        var sql = string.IsNullOrWhiteSpace(input.Password)
            ? """
              update app_users
              set full_name = @FullName,
                  username = @Username,
                  role = @Role,
                  is_active = @IsActive
              where id = @Id;
              """
            : """
              update app_users
              set full_name = @FullName,
                  username = @Username,
                  password_hash = @PasswordHash,
                  role = @Role,
                  is_active = @IsActive
              where id = @Id;
              """;

        var commandUpdate = new CommandDefinition(
            sql,
            new
            {
                input.Id,
                FullName = input.FullName.Trim(),
                Username = NormalizeUsername(input.Username),
                PasswordHash = string.IsNullOrWhiteSpace(input.Password) ? null : BCrypt.Net.BCrypt.HashPassword(input.Password),
                input.Role,
                input.IsActive
            },
            commandType: CommandType.Text,
            cancellationToken: cancellationToken,
            commandTimeout: _commandTimeout);

        await connection.ExecuteAsync(commandUpdate);
    }

    public Task SetUserActiveAsync(int id, bool isActive, CancellationToken cancellationToken) =>
        ExecuteSqlAsync(
            """
            update app_users
            set is_active = @IsActive
            where id = @Id;
            """,
            new { Id = id, IsActive = isActive },
            cancellationToken);

    public Task DeleteUserAsync(int id, CancellationToken cancellationToken) =>
        ExecuteSqlAsync(
            """
            delete from app_users
            where id = @Id;
            """,
            new { Id = id },
            cancellationToken);

    public async Task<IReadOnlyList<CustomerOrderSummary>> GetCustomerOrdersByEmailAsync(string email, CancellationToken cancellationToken)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var command = new CommandDefinition(
            $"select * from {StoredProcedures.CustomerOrdersGetByEmail}(@p_customer_email)",
            new { p_customer_email = email },
            commandType: CommandType.Text,
            cancellationToken: cancellationToken,
            commandTimeout: _commandTimeout);

        var results = await connection.QueryAsync<CustomerOrderSummary>(command);
        return results.ToList();
    }

    public async Task<IReadOnlyList<int>> GetWishlistProductIdsAsync(string username, CancellationToken cancellationToken)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var command = new CommandDefinition(
            """
            select cw.product_id
            from customer_wishlist cw
            join app_users u on u.id = cw.user_id
            where lower(u.username) = lower(@Username)
            """,
            new { Username = NormalizeUsername(username) },
            commandType: CommandType.Text,
            cancellationToken: cancellationToken,
            commandTimeout: _commandTimeout);

        var ids = await connection.QueryAsync<int>(command);
        return ids.ToList();
    }

    public async Task<IReadOnlyList<Product>> GetWishlistProductsAsync(string username, CancellationToken cancellationToken)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var command = new CommandDefinition(
            $"select * from {StoredProcedures.WishlistGetByUsername}(@p_username)",
            new { p_username = NormalizeUsername(username) },
            commandType: CommandType.Text,
            cancellationToken: cancellationToken,
            commandTimeout: _commandTimeout);

        var products = await connection.QueryAsync<Product>(command);
        return products.ToList();
    }

    public async Task AddWishlistItemAsync(string username, int productId, CancellationToken cancellationToken)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var command = new CommandDefinition(
            """
            insert into customer_wishlist(user_id, product_id)
            select id, @ProductId
            from app_users
            where lower(username) = lower(@Username)
            on conflict do nothing;
            """,
            new
            {
                Username = NormalizeUsername(username),
                ProductId = productId
            },
            commandType: CommandType.Text,
            cancellationToken: cancellationToken,
            commandTimeout: _commandTimeout);

        await connection.ExecuteAsync(command);
    }

    public async Task RemoveWishlistItemAsync(string username, int productId, CancellationToken cancellationToken)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var command = new CommandDefinition(
            """
            delete from customer_wishlist
            where product_id = @ProductId
              and user_id = (
                  select id
                  from app_users
                  where lower(username) = lower(@Username)
              );
            """,
            new
            {
                Username = NormalizeUsername(username),
                ProductId = productId
            },
            commandType: CommandType.Text,
            cancellationToken: cancellationToken,
            commandTimeout: _commandTimeout);

        await connection.ExecuteAsync(command);
    }

    private static string NormalizeUsername(string username) =>
        username.Trim().ToLowerInvariant();

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

    private async Task ExecuteSqlAsync(string sql, object? parameters, CancellationToken cancellationToken)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                parameters,
                commandType: CommandType.Text,
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
    public required string Status { get; init; }
    public required string PaymentStatus { get; init; }
    public string? PaymentReceiptUrl { get; init; }
    public string? PaymentReference { get; init; }
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
