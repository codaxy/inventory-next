using Codaxy.Inventory.App.Administration.Users;

namespace Codaxy.Inventory.Web.Auth.Session.Me;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder auth) =>
        auth.MapGet("/me", Handle).RequireAuthorization();

    /// <param name="Name">The user's name as an administrator gave it, not the provider's.</param>
    /// <param name="Permissions">What the client shows; the server checks every request anyway.</param>
    public sealed record Response(
        string Email,
        string Name,
        string Role,
        IReadOnlyList<string> Permissions
    );

    private static async Task<IResult> Handle(UserAccess access) =>
        await access.CurrentAsync() is { } user
            ? Results.Ok(
                new Response(
                    user.Email,
                    user.Name,
                    user.RoleName,
                    user.Permissions.Order().ToList()
                )
            )
            : Results.Unauthorized();
}
