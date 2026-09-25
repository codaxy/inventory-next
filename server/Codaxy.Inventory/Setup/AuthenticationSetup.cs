using Codaxy.Inventory.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Codaxy.Inventory.Setup;

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
        services.AddScoped<IEmailSender, SmtpEmailSender>();

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
