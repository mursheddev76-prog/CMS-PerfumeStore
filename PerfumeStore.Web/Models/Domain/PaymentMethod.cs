namespace PerfumeStore.Web.Models.Domain;

public record PaymentMethod(
    int Id,
    string Name,
    string Provider,
    decimal ProcessingFee,
    bool SupportsInstallments,
    bool IsActive);

