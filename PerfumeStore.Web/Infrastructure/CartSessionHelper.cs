using System.Security.Claims;

namespace PerfumeStore.Web.Infrastructure;

public static class CartSessionHelper
{
    private const string CartSessionKeyPrefix = "checkout-cart";
    private const string AnonymousCartOwnerKey = "checkout-cart-owner";

    public static string GetCartKey(ClaimsPrincipal? user, ISession session)
    {
        var username = user?.Identity?.IsAuthenticated == true
            ? user.FindFirstValue(ClaimTypes.Name)
            : null;

        if (!string.IsNullOrWhiteSpace(username))
        {
            return $"{CartSessionKeyPrefix}:{username.Trim().ToLowerInvariant()}";
        }

        var anonymousOwner = session.GetString(AnonymousCartOwnerKey);
        if (string.IsNullOrWhiteSpace(anonymousOwner))
        {
            anonymousOwner = Guid.NewGuid().ToString("N");
            session.SetString(AnonymousCartOwnerKey, anonymousOwner);
        }

        return $"{CartSessionKeyPrefix}:anon:{anonymousOwner}";
    }
}
