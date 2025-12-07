namespace PerfumeStore.Web.Models.Domain;

public record Category(
    int Id,
    string Name,
    string Description,
    bool IsActive);

