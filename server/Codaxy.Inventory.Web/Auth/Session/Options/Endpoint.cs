using Microsoft.Extensions.Options;

namespace Codaxy.Inventory.Web.Auth.Session.Options;

/// <summary>Which sign-in methods this server offers, so the screen shows only those.</summary>
public static class Endpoint
{
    public static void Map(RouteGroupBuilder auth) =>
        auth.MapGet(
            "/options",
            (IOptions<AuthOptions> options) =>
                new Response(options.Value.Google.Enabled, options.Value.OneTimeCode.Enabled)
        );

    public sealed record Response(bool Google, bool OneTimeCode);
}
