namespace PerfumeStore.Web.Data;

public static class StoredProcedures
{
    public const string ProductsGetFeatured = "sp_products_get_featured";
    public const string ProductsGetTrending = "sp_products_get_trending";
    public const string ProductsGetAll = "sp_products_get_all";
    public const string ProductUpsert = "sp_product_upsert";

    public const string CategoriesGetAll = "sp_categories_get_all";

    public const string HeroContentGet = "sp_hero_content_get";
    public const string HeroContentUpsert = "sp_hero_content_upsert";

    public const string PaymentMethodsGetAll = "sp_payment_methods_get_all";
    public const string PaymentMethodUpsert = "sp_payment_method_upsert";

    public const string DeliveryOptionsGetAll = "sp_delivery_options_get_all";
    public const string DeliveryOptionUpsert = "sp_delivery_option_upsert";

    public const string AdminDashboardStats = "sp_admin_dashboard_get_stats";

    public const string OrderCreate = "sp_order_create";
}

