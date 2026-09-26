using System.ComponentModel.DataAnnotations;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Assets;
using Codaxy.Inventory.App.Shared.Validation;
using Codaxy.Inventory.App.Shared.Volumes;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Infrastructure.Softwares;

/// <summary>What creating and editing a software take.</summary>
public sealed record SoftwareForm(
    [property:
        Required(ErrorMessage = "Give the software a name."),
        StringLength(200, ErrorMessage = "A name is at most 200 characters.")
    ]
        string? Name,
    [property: Required(ErrorMessage = "Choose the volume it is bought under.")] Guid? VolumeId
);

public sealed record InformationRow(Guid Id, string Name, string? Type);

/// <summary>A software as its page shows it, with the information kept on it.</summary>
public sealed record SoftwareDetail(
    Guid Id,
    string Name,
    VolumeRef Volume,
    Section<InformationRow> Information
);

internal static class SoftwareWrites
{
    public static async Task<IResult?> CheckAsync(
        InventoryContext context,
        SoftwareForm form,
        Guid? except,
        CancellationToken cancellationToken
    )
    {
        if (!await context.Volumes.AnyAsync(v => v.Id == form.VolumeId, cancellationToken))
            return Results.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    ["volumeId"] = ["That choice no longer exists."],
                }
            );
        return await Unique.CheckAsync(
            context.Softwares.Where(t => t.Id != except),
            t => t.Name,
            form.Name!,
            "name",
            "A software with this name already exists.",
            cancellationToken
        );
    }

    private static string? Text(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    public static void Apply(Software entity, SoftwareForm form)
    {
        entity.Name = form.Name!.Trim();
        entity.VolumeId = form.VolumeId!.Value;
    }

    public static async Task<SoftwareDetail?> DetailAsync(
        InventoryContext context,
        Guid id,
        CancellationToken cancellationToken
    )
    {
        var entity = await context
            .Softwares.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (entity is null)
            return null;

        return new SoftwareDetail(
            id,
            entity.Name,
            (await VolumeOptions.RefAsync(context, entity.VolumeId, cancellationToken))!,
            await AssetHoldings.SectionAsync(
                context
                    .Informations.AsNoTracking()
                    .Where(i => i.InformationLocations.Any(l => l.SoftwareId == id)),
                q =>
                    q.OrderBy(i => i.Name)
                        .ThenBy(i => i.Id)
                        .Select(i => new InformationRow(i.Id, i.Name, i.InformationType.Name)),
                cancellationToken
            )
        );
    }
}
