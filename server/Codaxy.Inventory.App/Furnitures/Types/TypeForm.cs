using System.ComponentModel.DataAnnotations;
using Codaxy.Inventory.App.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Furnitures.Types;

/// <summary>What creating and editing a furniture type take.</summary>
public sealed record TypeForm(
    [property:
        Required(ErrorMessage = "Give the type a name."),
        StringLength(100, ErrorMessage = "A name is at most 100 characters.")
    ]
        string? Name,
    [property: StringLength(1000, ErrorMessage = "A description is at most 1000 characters.")]
        string? Description
);

/// <summary>A furniture type as its page shows it.</summary>
public sealed record TypeDetail(Guid Id, string Name, string? Description, int FurnitureCount);

internal static class FurnitureTypes
{
    /// <summary>
    /// No other type holds the name, whatever its case — the original's grid refused duplicates.
    /// Checked here, not by an index: the schema is frozen, so two saves at once can still both pass.
    /// </summary>
    public static async Task<IResult?> CheckNameAsync(
        InventoryContext context,
        string name,
        Guid? except,
        CancellationToken cancellationToken
    )
    {
        var lower = name.Trim().ToLowerInvariant();
        var taken = await context.FurnitureTypes.AnyAsync(
            t => t.Id != except && t.Name.ToLower() == lower,
            cancellationToken
        );

        return taken
            ? Results.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    ["name"] = ["A type with this name already exists."],
                }
            )
            : null;
    }

    public static void Apply(FurnitureType type, TypeForm form)
    {
        type.Name = form.Name!.Trim();
        type.Description = string.IsNullOrWhiteSpace(form.Description)
            ? null
            : form.Description.Trim();
    }

    public static Task<TypeDetail?> DetailAsync(
        InventoryContext context,
        Guid id,
        CancellationToken cancellationToken
    ) =>
        context
            .FurnitureTypes.AsNoTracking()
            .Where(t => t.Id == id)
            .Select(t => new TypeDetail(
                t.Id,
                t.Name,
                t.Description,
                context.Furnitures.Count(f => f.FurnitureTypeId == t.Id)
            ))
            .FirstOrDefaultAsync(cancellationToken);
}
