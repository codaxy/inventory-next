using System.Security.Claims;

namespace Codaxy.Inventory.Web.Auth.Session.Me;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder auth) =>
        auth.MapGet(
                "/me",
                (ClaimsPrincipal user) =>
                    new Response(
                        user.FindFirstValue(SignIn.EmailClaim) ?? "",
                        user.Identity?.Name ?? ""
                    )
            )
            .RequireAuthorization();

    public sealed record Response(string Email, string Name);
}
