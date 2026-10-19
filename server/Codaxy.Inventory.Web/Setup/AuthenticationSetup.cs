using System.Security.Claims;
using Codaxy.Inventory.App.Administration.Users;
using Codaxy.Inventory.App.Shared.Access;
using Codaxy.Inventory.Web.Auth;
using Codaxy.Inventory.Web.Auth.OneTimeCodes;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

namespace Codaxy.Inventory.Web.Setup;

public static class AuthenticationSetup
{
    public static IServiceCollection AddInventoryAuthentication(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IOneTimeCodeStore, InMemoryOneTimeCodeStore>();
        services.AddScoped<SignInPolicy>();
        services.AddScoped<UserAccess>();
        services.AddScoped<IAuthorizationHandler, PermissionHandler>();

        // A policy per permission in the catalogue, and none for anything else: an endpoint naming a
        // key outside it fails rather than letting every session through.
        var authorization = services.AddAuthorizationBuilder();
        foreach (var permission in Permissions.All)
            authorization.AddPolicy(
                PermissionPolicy.For(permission.Key),
                policy =>
                    policy
                        .RequireAuthenticatedUser()
                        .AddRequirements(new PermissionRequirement(permission.Key))
            );

        var authentication = services
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(ConfigureSessionCookie)
            .AddCookie(AuthEndpoints.ExternalScheme);

        var google = configuration.GetSection(AuthOptions.Section).GetSection("Google");

        // Registered only when it is configured: a handler with no credentials would advertise a
        // provider that fails at Google rather than one that is simply not offered.
        if (
            !string.IsNullOrWhiteSpace(google["ClientId"])
            && !string.IsNullOrWhiteSpace(google["ClientSecret"])
        )
            authentication.AddGoogle(options =>
            {
                options.ClientId = google["ClientId"]!;
                options.ClientSecret = google["ClientSecret"]!;
                options.SignInScheme = AuthEndpoints.ExternalScheme;
                options.CallbackPath = "/signin-google";

                // Lax is enough for the cookie that carries the flow across to Google, because the
                // redirect back is a top-level GET; None would let it travel with any cross-site
                // request for no gain.
                options.CorrelationCookie.SameSite = SameSiteMode.Lax;
            });

        return services.AddAuthorization();
    }

    private static void ConfigureSessionCookie(CookieAuthenticationOptions options)
    {
        options.Cookie.Name = "inventory.session";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;

        // Lax rather than Strict because the Google redirect returns as a top-level GET, which Strict
        // would strip the cookie from. It still refuses to travel with a cross-site POST, which is
        // what stands in for anti-forgery on the write endpoints.
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.ExpireTimeSpan = TimeSpan.FromDays(14);
        options.SlidingExpiration = true;

        // The API answers with status codes; only the client decides to show a sign-in screen.
        // A cookie outlives the access it was issued for, so the user is looked up on every request:
        // a deactivated or missing one is signed out there, not in fourteen days.
        options.Events.OnValidatePrincipal = async context =>
        {
            var email = context.Principal?.FindFirstValue(SignIn.EmailClaim);
            var access = context.HttpContext.RequestServices.GetRequiredService<UserAccess>();

            if (email is null || await access.ForAsync(email) is null)
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme
                );
            }
        };

        options.Events.OnRedirectToLogin = Status(StatusCodes.Status401Unauthorized);
        options.Events.OnRedirectToAccessDenied = Status(StatusCodes.Status403Forbidden);

        static Func<RedirectContext<CookieAuthenticationOptions>, Task> Status(int statusCode) =>
            context =>
            {
                context.Response.StatusCode = statusCode;
                return Task.CompletedTask;
            };
    }
}
