using Codaxy.Inventory.App.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Licenses.Activations.Options;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder activations) => activations.MapGet("/options", Handle);

    public sealed record Option(Guid Id, string Text);

    /// <param name="Number">The device's inventory number, which a picker searches as well as the name.</param>
    public sealed record DeviceOption(Guid Id, string Text, int? Number, string? Holder);

    /// <param name="Text">What tells one volume from another of the same licence and software: its description, else its type.</param>
    public sealed record VolumeOption(
        Guid Id,
        string Text,
        Guid SoftwareId,
        string Software,
        Guid LicenseId
    );

    /// <summary>
    /// What the form and the list's filters pick from: software and services that have a volume,
    /// licences, people, and the electronic devices whose type holds licences — the only ones a
    /// per-device volume is activated for.
    /// </summary>
    public sealed record Response(
        IReadOnlyList<Option> Software,
        IReadOnlyList<Option> Licenses,
        IReadOnlyList<Option> People,
        IReadOnlyList<DeviceOption> Devices,
        IReadOnlyList<VolumeOption> Volumes
    );

    private static async Task<Response> Handle(
        InventoryContext context,
        CancellationToken cancellationToken
    ) =>
        new(
            await context
                .SoftwareOrServices.AsNoTracking()
                .Where(s => s.Volumes.Any())
                .OrderBy(s => s.Name)
                .Select(s => new Option(s.Id, s.Name))
                .ToListAsync(cancellationToken),
            await context
                .Licenses.AsNoTracking()
                .OrderBy(l => l.Asset.Name)
                .Select(l => new Option(l.AssetId, l.Asset.Name))
                .ToListAsync(cancellationToken),
            await context
                .Persons.AsNoTracking()
                .OrderBy(p => p.Name)
                .Select(p => new Option(p.Id, p.Name))
                .ToListAsync(cancellationToken),
            await context
                .ElectronicDevices.AsNoTracking()
                .Where(d => d.ElectronicDeviceType != null && d.ElectronicDeviceType.HoldLicences)
                .OrderBy(d => d.Asset.Name)
                .Select(d => new DeviceOption(
                    d.AssetId,
                    d.Asset.Name,
                    d.Asset.InventoryNumber,
                    d.Asset.Person.Name
                ))
                .ToListAsync(cancellationToken),
            await context
                .Volumes.AsNoTracking()
                .OrderBy(v => v.SoftwareOrService.Name)
                .ThenBy(v => v.License.Asset.Name)
                .Select(v => new VolumeOption(
                    v.Id,
                    v.SoftwareOrService.Name
                        + " · "
                        + v.License.Asset.Name
                        + " · "
                        + (
                            v.Description != null && v.Description != ""
                                ? v.Description
                                : v.VolumeType.Text
                        ),
                    v.SoftwareOrServiceId,
                    v.SoftwareOrService.Name,
                    v.LicenseId
                ))
                .ToListAsync(cancellationToken)
        );
}
