namespace PerfumeStore.Web.Models.Domain;

public record DeliveryOption(
    int Id,
    string Name,
    string Description,
    decimal Fee,
    int EstimatedDays,
    bool IsActive);

