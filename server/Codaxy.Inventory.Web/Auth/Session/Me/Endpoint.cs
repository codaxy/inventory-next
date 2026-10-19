using Codaxy.Inventory.App.Administration.Users;

namespace Codaxy.Inventory.Web.Auth.Session.Me;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder auth) =>
        auth.MapGet("/me", Handle).RequireAuthorization();

    public const string UserKind = "user";
    public const string PersonKind = "person";

    /// <param name="Kind">
    /// <see cref="UserKind"/>, someone running the application with a role, or <see cref="PersonKind"/>,
    /// an employee signed in to see their own equipment, with no role and no permission.
    /// </param>
    /// <param name="Name">The name the application keeps, not the provider's.</param>
    /// <param name="Permissions">What the client shows; the server checks every request anyway.</param>
    /// <param name="MyEquipment">
    /// Whether "My equipment" has anything to show: always for a person; for a user, whether a person
    /// is kept under the same email.
    /// </param>
    public sealed record Response(
        string Kind,
        string Email,
        string Name,
        string? Role,
        IReadOnlyList<string> Permissions,
        bool MyEquipment
    );

    private static async Task<IResult> Handle(UserAccess access) =>
        await access.CurrentAsync() is { } user
            ? Results.Ok(
                new Response(
                    UserKind,
                    user.Email,
                    user.Name,
                    user.RoleName,
                    user.Permissions.Order().ToList(),
                    await access.EquipmentHolderAsync() is not null
                )
            )
        : await access.CurrentPersonAsync() is { } person
            ? Results.Ok(new Response(PersonKind, person.Email, person.Name, null, [], true))
        : Results.Unauthorized();
}
