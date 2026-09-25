using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Codaxy.Inventory.Setup;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Codaxy.Inventory.Auth;

public static class AuthEndpoints
{
    /// <summary>The cookie the OAuth hop lands in, before the sign-in rules have been applied.</summary>
    public const string ExternalScheme = "External";

    public static void MapAuth(this IEndpointRouteBuilder app)
    {
        // Anonymous by default: there is no fallback authorization policy, and a group-level
        // AllowAnonymous would win over RequireAuthorization on the one endpoint that needs it.
        var auth = app.MapGroup("/api/auth");

        auth.MapGet(
            "/options",
            (IOptions<AuthOptions> options) =>
                new AuthOptionsResponse(
                    options.Value.Google.Enabled,
                    options.Value.OneTimeCode.Enabled
                )
        );

        auth.MapGet(
                "/me",
                (ClaimsPrincipal user) =>
                    new MeResponse(
                        user.FindFirstValue(SignIn.EmailClaim) ?? "",
                        user.Identity?.Name ?? ""
                    )
            )
            .RequireAuthorization();

        auth.MapPost(
            "/sign-out",
            async (HttpContext context) =>
            {
                await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return Results.NoContent();
            }
        );

        auth.MapPost("/one-time-code/request", RequestOneTimeCode)
            .RequireRateLimiting(RateLimitSetup.SignInPolicy);
        auth.MapPost("/one-time-code/verify", VerifyOneTimeCode)
            .RequireRateLimiting(RateLimitSetup.SignInPolicy);

        var google = app.MapGroup("/auth/google");

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

        google.MapGet("/callback", GoogleCallback);
    }

    private static async Task<IResult> RequestOneTimeCode(
        [FromBody] OneTimeCodeRequest request,
        IOptions<AuthOptions> options,
        SignInPolicy policy,
        IOneTimeCodeStore codes,
        IEmailSender email,
        ILogger<OneTimeCodeRequest> log,
        CancellationToken cancellation
    )
    {
        if (!options.Value.OneTimeCode.Enabled)
            return Results.NotFound();

        if (!MiniValidator.IsValid(request, out var problem))
            return problem;

        var decision = policy.Evaluate(request.Email);

        if (decision is { IsAllowed: false, IsSayable: true })
            return Results.Problem(
                title: decision.Reason,
                statusCode: StatusCodes.Status403Forbidden
            );

        // Otherwise the answer is the same whether or not the address may sign in: anything else
        // turns this endpoint into a way of listing who works here.
        if (decision.IsAllowed)
        {
            // Null when one was sent moments ago: a caller changing address cannot make someone
            // else's mailbox the target.
            var code = codes.Issue(request.Email);

            if (code is not null)
                await email.SendAsync(
                    request.Email,
                    "Your Inventory sign-in code",
                    $"Your sign-in code is {code}. It is valid for "
                        + $"{options.Value.OneTimeCode.Validity.TotalMinutes:0} minutes.",
                    cancellation
                );
        }
        else
        {
            log.LogInformation(
                "One-time code refused for {Email}: {Reason}",
                request.Email,
                decision.Reason
            );
        }

        return Results.NoContent();
    }

    private static async Task<IResult> VerifyOneTimeCode(
        [FromBody] OneTimeCodeVerification request,
        HttpContext context,
        IOptions<AuthOptions> options,
        SignInPolicy policy,
        IOneTimeCodeStore codes
    )
    {
        if (!options.Value.OneTimeCode.Enabled)
            return Results.NotFound();

        if (!MiniValidator.IsValid(request, out var problem))
            return problem;

        if (
            !policy.Evaluate(request.Email).IsAllowed || !codes.Consume(request.Email, request.Code)
        )
            return Results.Problem(
                title: "That code is not valid.",
                statusCode: StatusCodes.Status401Unauthorized
            );

        await context.SignInAsync(request.Email, displayName: null);
        return Results.NoContent();
    }

    private static async Task<IResult> GoogleCallback(HttpContext context, SignInPolicy policy)
    {
        var result = await context.AuthenticateAsync(ExternalScheme);

        if (!result.Succeeded)
            return Results.Redirect("/sign-in?error=google");

        var email = result.Principal.FindFirstValue(ClaimTypes.Email);
        var decision = policy.Evaluate(email);

        await context.SignOutAsync(ExternalScheme);

        if (!decision.IsAllowed)
            return Results.Redirect("/sign-in?error=refused");

        await context.SignInAsync(email!, result.Principal.FindFirstValue(ClaimTypes.Name));
        return Results.Redirect("/");
    }

    public sealed record AuthOptionsResponse(bool Google, bool OneTimeCode);

    public sealed record MeResponse(string Email, string Name);

    // The messages reach the sign-in screen verbatim.
    public sealed record OneTimeCodeRequest(
        [property: Required(ErrorMessage = EmailMessage), EmailAddress(ErrorMessage = EmailMessage)]
            string Email
    );

    public sealed record OneTimeCodeVerification(
        [property: Required(ErrorMessage = EmailMessage), EmailAddress(ErrorMessage = EmailMessage)]
            string Email,
        [property:
            Required(ErrorMessage = CodeMessage),
            RegularExpression("^[0-9]{6}$", ErrorMessage = CodeMessage)
        ]
            string Code
    );

    private const string EmailMessage = "Enter a valid email address.";
    private const string CodeMessage = "Enter the six-digit code from the email.";
}
