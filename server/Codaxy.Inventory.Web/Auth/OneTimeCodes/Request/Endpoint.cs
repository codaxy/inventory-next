using System.ComponentModel.DataAnnotations;
using Codaxy.Inventory.App.Shared.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Codaxy.Inventory.Web.Auth.OneTimeCodes.Request;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder auth) =>
        auth.MapPost("/one-time-code/request", Handle)
            .RequireRateLimiting(AuthEndpoints.SignInRateLimit);

    public sealed record Body(
        [property:
            Required(ErrorMessage = Messages.Email),
            EmailAddress(ErrorMessage = Messages.Email)
        ]
            string Email
    );

    private static async Task<IResult> Handle(
        [FromBody] Body request,
        IOptions<AuthOptions> options,
        SignInPolicy policy,
        IOneTimeCodeStore codes,
        IEmailSender email,
        ILogger<Body> log,
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
}
