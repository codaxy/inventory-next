using System.ComponentModel.DataAnnotations;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Assets;
using Codaxy.Inventory.App.Shared.Validation;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Company.Manufacturers;

/// <summary>What creating and editing a manufacturer take.</summary>
public sealed record ManufacturerForm(
    [property:
        Required(ErrorMessage = "Give the manufacturer a name."),
        StringLength(200, ErrorMessage = "A name is at most 200 characters.")
    ]
        string? Name,
    [property: StringLength(500, ErrorMessage = "A URL is at most 500 characters.")] string? Url
);

public sealed record SoftwareRow(Guid Id, string Name, string? Category);

/// <summary>A manufacturer as its page shows it: its devices and its software and services.</summary>
public sealed record ManufacturerDetail(
    Guid Id,
    string Name,
    string? Url,
    Section<AssetRow> Devices,
    Section<SoftwareRow> Software
);

internal static class ManufacturerWrites
{
    public static Task<IResult?> CheckAsync(
        InventoryContext context,
        ManufacturerForm form,
        Guid? except,
        CancellationToken cancellationToken
    ) =>
        Unique.CheckAsync(
            context.Manufacturers.Where(m => m.Id != except),
            m => m.Name,
            form.Name!,
            "name",
            "A manufacturer with this name already exists.",
            cancellationToken
        );

    public static void Apply(Manufacturer manufacturer, ManufacturerForm form)
    {
        manufacturer.Name = form.Name!.Trim();
        manufacturer.URL = string.IsNullOrWhiteSpace(form.Url) ? null : form.Url.Trim();
    }

    public static async Task<ManufacturerDetail?> DetailAsync(
        InventoryContext context,
        Guid id,
        CancellationToken cancellationToken
    )
    {
        var manufacturer = await context
            .Manufacturers.AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
        if (manufacturer is null)
            return null;

        return new ManufacturerDetail(
            id,
            manufacturer.Name,
            manufacturer.URL,
            await AssetHoldings.SectionAsync(
                context.ElectronicDevices.AsNoTracking().Where(d => d.ManufacturerId == id),
                q =>
                    q.OrderBy(d => d.Asset.Name)
                        .ThenBy(d => d.AssetId)
                        .Select(d => new AssetRow(
                            d.AssetId,
                            d.Asset.InventoryNumber,
                            d.Asset.Name,
                            d.ElectronicDeviceType.Name,
                            d.ModelName
                        )),
                cancellationToken
            ),
            await AssetHoldings.SectionAsync(
                context.SoftwareOrServices.AsNoTracking().Where(s => s.ManufacturerId == id),
                q =>
                    q.OrderBy(s => s.Name)
                        .ThenBy(s => s.Id)
                        .Select(s => new SoftwareRow(
                            s.Id,
                            s.Name,
                            s.SoftwareOrServiceCategory.Name
                        )),
                cancellationToken
            )
        );
    }
}
