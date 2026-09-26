using Codaxy.Inventory.App.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.ElectronicDevices.Devices.Delete;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder devices) => devices.MapDelete("/{id:guid}", Handle);

    /// <summary>
    /// The device, its asset and its maintenance contracts, in one save — the contracts restrict the
    /// asset, so they go first in that save, not in a request of their own as the original's client
    /// did. Seats activated on it or information kept on it refuse the delete: both foreign keys
    /// would, as a 500, and neither is the device's to take.
    /// </summary>
    private static async Task<IResult> Handle(
        Guid id,
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        var device = await context
            .ElectronicDevices.Include(d => d.Asset)
            .FirstOrDefaultAsync(d => d.AssetId == id, cancellationToken);
        if (device is null)
            return Results.NotFound();

        var seats = await context.Activations.CountAsync(a => a.AssetId == id, cancellationToken);
        var information = await context.InformationLocations.CountAsync(
            l => l.ElectronicDeviceId == id,
            cancellationToken
        );
        var held = new[]
        {
            seats switch
            {
                0 => null,
                1 => "a seat is activated on it",
                _ => $"{seats} seats are activated on it",
            },
            information switch
            {
                0 => null,
                1 => "a piece of information is kept on it",
                _ => $"{information} pieces of information are kept on it",
            },
        }.OfType<string>().ToList();
        if (held.Count > 0)
        {
            var reason = string.Join(" and ", held);
            return Results.Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: $"{char.ToUpperInvariant(reason[0])}{reason[1..]}, so the device cannot be deleted."
            );
        }

        context.MaintenanceContracts.RemoveRange(
            await context
                .MaintenanceContracts.Where(c => c.AssetId == id)
                .ToListAsync(cancellationToken)
        );
        context.ElectronicDevices.Remove(device);
        context.Assets.Remove(device.Asset);
        await context.SaveChangesAsync(cancellationToken);
        return Results.NoContent();
    }
}
