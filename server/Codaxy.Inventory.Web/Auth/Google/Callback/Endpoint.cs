using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace Codaxy.Inventory.Web.Auth.Google.Callback;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder google) => google.MapGet("/callback", Handle);

    private static async Task<IResult> Handle(HttpContext context, SignInPolicy policy)
    {
        var result = await context.AuthenticateAsync(AuthEndpoints.ExternalScheme);

        if (!result.Succeeded)
            return Results.Redirect("/sign-in?error=google");

        var email = result.Principal.FindFirstValue(ClaimTypes.Email);
        var decision = policy.Evaluate(email);

        await context.SignOutAsync(AuthEndpoints.ExternalScheme);

        if (!decision.IsAllowed)
            return Results.Redirect("/sign-in?error=refused");

        await context.SignInAsync(email!, result.Principal.FindFirstValue(ClaimTypes.Name));
        return Results.Redirect("/");
    }
}
