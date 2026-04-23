using System.Text.Json;

namespace PerfumeStore.Web.Infrastructure;

public static class SessionExtensions
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public static void SetJson<T>(this ISession session, string key, T value)
    {
        session.SetString(key, JsonSerializer.Serialize(value, SerializerOptions));
    }

    public static T? GetJson<T>(this ISession session, string key)
    {
        var raw = session.GetString(key);
        return string.IsNullOrWhiteSpace(raw)
            ? default
            : JsonSerializer.Deserialize<T>(raw, SerializerOptions);
    }
}
