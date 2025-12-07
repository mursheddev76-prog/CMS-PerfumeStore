namespace PerfumeStore.Web.Models.Domain;

public record AdminDashboardStats(
    int ActiveProducts,
    int ActivePaymentMethods,
    int ActiveDeliveryOptions,
    decimal TodayRevenue,
    int PendingOrders);

