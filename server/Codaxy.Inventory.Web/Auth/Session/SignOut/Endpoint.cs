using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Codaxy.Inventory.Web.Auth.Session.SignOut;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder auth) =>
        auth.MapPost(
            "/sign-out",
            async (HttpContext context) =>
            {
                await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return Results.NoContent();
            }
        );
}
