using System.ComponentModel.DataAnnotations;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Assets;
using Codaxy.Inventory.App.Shared.Validation;
using Codaxy.Inventory.App.Shared.Volumes;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Infrastructure.CloudSubscriptions;

/// <summary>What creating and editing a cloud subscription take.</summary>
public sealed record CloudSubscriptionForm(
    [property:
        Required(ErrorMessage = "Give the cloud subscription a name."),
        StringLength(200, ErrorMessage = "A name is at most 200 characters.")
    ]
        string? Name,
    [property: Required(ErrorMessage = "Choose the volume it is bought under.")] Guid? VolumeId,
    [property: StringLength(500, ErrorMessage = "A URL is at most 500 characters.")]
        string? ManagementUrl
);

public sealed record InformationRow(Guid Id, string Name, string? Type);

/// <summary>A cloud subscription as its page shows it, with the information kept on it.</summary>
public sealed record CloudSubscriptionDetail(
    Guid Id,
    string Name,
    string? ManagementUrl,
    VolumeRef Volume,
    Section<InformationRow> Information
);

internal static class CloudSubscriptionWrites
{
    public static async Task<IResult?> CheckAsync(
        InventoryContext context,
        CloudSubscriptionForm form,
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
            context.Clouds.Where(t => t.Id != except),
            t => t.Name,
            form.Name!,
            "name",
            "A cloud subscription with this name already exists.",
            cancellationToken
        );
    }

    private static string? Text(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    public static void Apply(Cloud entity, CloudSubscriptionForm form)
    {
        entity.Name = form.Name!.Trim();
        entity.VolumeId = form.VolumeId!.Value;
        entity.ManagementURL = Text(form.ManagementUrl);
    }

    public static async Task<CloudSubscriptionDetail?> DetailAsync(
        InventoryContext context,
        Guid id,
        CancellationToken cancellationToken
    )
    {
        var entity = await context
            .Clouds.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (entity is null)
            return null;

        return new CloudSubscriptionDetail(
            id,
            entity.Name,
            entity.ManagementURL,
            (await VolumeOptions.RefAsync(context, entity.VolumeId, cancellationToken))!,
            await AssetHoldings.SectionAsync(
                context
                    .Informations.AsNoTracking()
                    .Where(i => i.InformationLocations.Any(l => l.CloudId == id)),
                q =>
                    q.OrderBy(i => i.Name)
                        .ThenBy(i => i.Id)
                        .Select(i => new InformationRow(i.Id, i.Name, i.InformationType.Name)),
                cancellationToken
            )
        );
    }
}
