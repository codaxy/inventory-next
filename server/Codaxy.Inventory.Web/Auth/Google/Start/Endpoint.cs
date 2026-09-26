using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.Extensions.Options;

namespace Codaxy.Inventory.Web.Auth.Google.Start;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder google) =>
        google.MapGet(
            "/start",
            (IOptions<AuthOptions> options) =>
                options.Value.Google.Enabled
                    ? Results.Challenge(
                        new AuthenticationProperties { RedirectUri = "/auth/google/callback" },
                        [GoogleDefaults.AuthenticationScheme]
                    )
                    : Results.NotFound()
        );
}
