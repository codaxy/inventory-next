using Codaxy.Inventory.App.Shared.Access;
using Codaxy.Inventory.App.Shared.Settings;

namespace Codaxy.Inventory.App.Company.People.SignIn;

/// <summary>
/// Whether employees may sign in at all, for a person's form before there is a person to read it
/// from: a new one's "Can sign in" is shown off, and why, while they may not.
/// </summary>
public static class Endpoint
{
    public static void Map(RouteGroupBuilder people) => people.MapGet("/sign-in", Handle);

    public sealed record Response(bool Open);

    private static async Task<Response> Handle(
        SettingStore settings,
        CancellationToken cancellationToken
    ) => new((await settings.LoadAsync(cancellationToken)).Get(SignInSettings.People));
}
