namespace PerfumeStore.Web.Data;

public class DatabaseOptions
{
    public const string SectionName = "Database";

    public int CommandTimeoutSeconds { get; set; } = 30;
}

