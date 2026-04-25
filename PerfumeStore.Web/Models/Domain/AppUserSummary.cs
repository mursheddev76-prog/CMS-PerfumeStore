namespace PerfumeStore.Web.Models.Domain;

public record AppUserSummary(
    int Id,
    string FullName,
    string Username,
    string Role,
    bool IsActive,
    DateTime CreatedAt);
