namespace Codaxy.Inventory.Web.Auth;

public static class AuthEndpoints
{
    /// <summary>The cookie the OAuth hop lands in, before the sign-in rules have been applied.</summary>
    public const string ExternalScheme = "External";

    /// <summary>The rate-limit policy the one-time-code endpoints carry; the host registers it.</summary>
    public const string SignInRateLimit = "sign-in";

    public static void MapAuth(this IEndpointRouteBuilder app)
    {
        // Anonymous by default: there is no fallback authorization policy, and a group-level
        // AllowAnonymous would win over RequireAuthorization on the one endpoint that needs it.
        var auth = app.MapGroup("/api/auth");

        Session.Options.Endpoint.Map(auth);
        Session.Me.Endpoint.Map(auth);
        Session.SignOut.Endpoint.Map(auth);
        OneTimeCodes.Request.Endpoint.Map(auth);
        OneTimeCodes.Verify.Endpoint.Map(auth);

        // Outside /api: the browser navigates here, it does not fetch.
        var google = app.MapGroup("/auth/google");

        Google.Start.Endpoint.Map(google);
        Google.Callback.Endpoint.Map(google);
    }
}
