using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Codaxy.Inventory.Web.Auth.OneTimeCodes.Verify;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder auth) =>
        auth.MapPost("/one-time-code/verify", Handle)
            .RequireRateLimiting(AuthEndpoints.SignInRateLimit);

    public sealed record Body(
        [property:
            Required(ErrorMessage = Messages.Email),
            EmailAddress(ErrorMessage = Messages.Email)
        ]
            string Email,
        [property:
            Required(ErrorMessage = Messages.Code),
            RegularExpression("^[0-9]{6}$", ErrorMessage = Messages.Code)
        ]
            string Code
    );

    private static async Task<IResult> Handle(
        [FromBody] Body request,
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
}
