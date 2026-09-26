using System.ComponentModel.DataAnnotations;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Assets;
using Codaxy.Inventory.App.Shared.Validation;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Informations.Tags;

/// <summary>What creating and editing an information tag take.</summary>
public sealed record InformationTagForm(
    [property:
        Required(ErrorMessage = "Give the tag a name."),
        StringLength(200, ErrorMessage = "A name is at most 200 characters.")
    ]
        string? Name,
    [property: StringLength(1000, ErrorMessage = "A description is at most 1000 characters.")]
        string? Description
);

public sealed record InformationRow(Guid Id, string Name, string? Type);

/// <summary>An information tag as its page shows it, with the information of it.</summary>
public sealed record InformationTagDetail(
    Guid Id,
    string Name,
    string? Description,
    Section<InformationRow> Information
);

internal static class InformationTagWrites
{
    public static Task<IResult?> CheckAsync(
        InventoryContext context,
        InformationTagForm form,
        Guid? except,
        CancellationToken cancellationToken
    ) =>
        Unique.CheckAsync(
            context.InformationTags.Where(t => t.Id != except),
            t => t.Name,
            form.Name!,
            "name",
            "A tag with this name already exists.",
            cancellationToken
        );

    public static void Apply(InformationTag entity, InformationTagForm form)
    {
        entity.Name = form.Name!.Trim();
        entity.Description = string.IsNullOrWhiteSpace(form.Description)
            ? null
            : form.Description.Trim();
    }

    public static async Task<InformationTagDetail?> DetailAsync(
        InventoryContext context,
        Guid id,
        CancellationToken cancellationToken
    )
    {
        var entity = await context
            .InformationTags.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (entity is null)
            return null;

        return new InformationTagDetail(
            id,
            entity.Name,
            entity.Description,
            await AssetHoldings.SectionAsync(
                context
                    .Informations.AsNoTracking()
                    .Where(i => i.Tags.Any(t => t.InformationTagId == id)),
                q =>
                    q.OrderBy(i => i.Name)
                        .ThenBy(i => i.Id)
                        .Select(i => new InformationRow(i.Id, i.Name, i.InformationType.Name)),
                cancellationToken
            )
        );
    }
}
