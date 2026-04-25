namespace PerfumeStore.Web.Models.Domain;

public record PaymentMethod(
    int Id,
    string Name,
    string Provider,
    string PaymentType,
    string? PartnerName,
    decimal ProcessingFee,
    bool SupportsInstallments,
    bool IsActive,
    string? AccountTitle,
    string? AccountNumber,
    string? BankName,
    string? Iban,
    string? Instructions,
    bool RequiresReceipt);
