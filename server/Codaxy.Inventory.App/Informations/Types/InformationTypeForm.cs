using System.ComponentModel.DataAnnotations;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Assets;
using Codaxy.Inventory.App.Shared.Validation;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Informations.Types;

/// <summary>What creating and editing an information type take.</summary>
public sealed record InformationTypeForm(
    [property:
        Required(ErrorMessage = "Give the type a name."),
        StringLength(200, ErrorMessage = "A name is at most 200 characters.")
    ]
        string? Name,
    [property: StringLength(1000, ErrorMessage = "A description is at most 1000 characters.")]
        string? Description
);

public sealed record InformationRow(Guid Id, string Name, string? Type);

/// <summary>An information type as its page shows it, with the information of it.</summary>
public sealed record InformationTypeDetail(
    Guid Id,
    string Name,
    string? Description,
    Section<InformationRow> Information
);

internal static class InformationTypeWrites
{
    public static Task<IResult?> CheckAsync(
        InventoryContext context,
        InformationTypeForm form,
        Guid? except,
        CancellationToken cancellationToken
    ) =>
        Unique.CheckAsync(
            context.InformationTypes.Where(t => t.Id != except),
            t => t.Name,
            form.Name!,
            "name",
            "A type with this name already exists.",
            cancellationToken
        );

    public static void Apply(InformationType entity, InformationTypeForm form)
    {
        entity.Name = form.Name!.Trim();
        entity.Description = string.IsNullOrWhiteSpace(form.Description)
            ? null
            : form.Description.Trim();
    }

    public static async Task<InformationTypeDetail?> DetailAsync(
        InventoryContext context,
        Guid id,
        CancellationToken cancellationToken
    )
    {
        var entity = await context
            .InformationTypes.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (entity is null)
            return null;

        return new InformationTypeDetail(
            id,
            entity.Name,
            entity.Description,
            await AssetHoldings.SectionAsync(
                context.Informations.AsNoTracking().Where(i => i.InformationTypeId == id),
                q =>
                    q.OrderBy(i => i.Name)
                        .ThenBy(i => i.Id)
                        .Select(i => new InformationRow(i.Id, i.Name, i.InformationType.Name)),
                cancellationToken
            )
        );
    }
}
