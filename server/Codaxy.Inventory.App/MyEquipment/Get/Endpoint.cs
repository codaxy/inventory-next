using Codaxy.Inventory.App.Administration.Users;
using Codaxy.Inventory.App.Licenses.Licenses;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Settings;

namespace Codaxy.Inventory.App.MyEquipment.Get;

/// <summary>
/// The signed-in employee's page, whole: their name, what needs attention, and everything of theirs
/// by section. Answers a person's session alone, about that person: no permission stands for it,
/// the identity does; a user, who holds no person session, is refused.
/// </summary>
public static class Endpoint
{
    public static void Map(RouteGroupBuilder mine) => mine.MapGet("", Handle);

    public sealed record Response(
        string Name,
        IReadOnlyList<Warning> Warnings,
        IReadOnlyList<Thing> Devices,
        IReadOnlyList<Thing> Furniture,
        IReadOnlyList<Thing> Software,
        IReadOnlyList<Thing> Licenses
    );

    private static async Task<IResult> Handle(
        UserAccess access,
        InventoryContext context,
        SettingStore settings,
        TimeProvider clock,
        CancellationToken cancellationToken
    )
    {
        if (await access.CurrentPersonAsync(cancellationToken) is not { } person)
            return Results.Forbid();

        var today = Expiry.Today(clock);
        var warnings = await Mine.WarningsAsync(
            context,
            settings,
            person.PersonId,
            today,
            cancellationToken
        );
        var things = await Mine.ThingsAsync(
            context,
            person.PersonId,
            warnings,
            today,
            cancellationToken
        );

        return Results.Ok(
            new Response(
                person.Name,
                warnings,
                things.Devices,
                things.Furniture,
                things.Software,
                things.Licenses
            )
        );
    }
}
