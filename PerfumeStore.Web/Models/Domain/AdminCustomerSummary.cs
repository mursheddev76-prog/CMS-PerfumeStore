namespace PerfumeStore.Web.Models.Domain;

public record AdminCustomerSummary(
    string FullName,
    string Email,
    int OrderCount,
    decimal LifetimeValue,
    int WishlistCount,
    DateTime? LastOrderAt);
