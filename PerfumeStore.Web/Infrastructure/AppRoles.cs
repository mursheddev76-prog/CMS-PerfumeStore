namespace PerfumeStore.Web.Infrastructure;

public static class AppRoles
{
    public const string Admin = "admin";
    public const string OperationsManager = "operations_manager";
    public const string CatalogManager = "catalog_manager";
    public const string FinanceManager = "finance_manager";
    public const string Customer = "customer";

    public static readonly string[] StaffRoles =
    {
        Admin,
        OperationsManager,
        CatalogManager,
        FinanceManager
    };

    public static readonly string[] AllRoles =
    {
        Admin,
        OperationsManager,
        CatalogManager,
        FinanceManager,
        Customer
    };
}
