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
    public const string AdminOrdersGetRecent = "sp_admin_orders_get_recent";
    public const string AdminCustomersGetTop = "sp_admin_customers_get_top";
    public const string OrderReviewUpdate = "sp_order_review_update";

    public const string OrderCreate = "sp_order_create";
    public const string UserGetByUsername = "sp_user_get_by_username";
    public const string CustomerOrdersGetByEmail = "sp_customer_orders_get_by_email";
    public const string WishlistGetByUsername = "sp_customer_wishlist_get_by_username";
}
