using System.ComponentModel.DataAnnotations;
using Codaxy.Inventory.App.ElectronicDevices.Types;
using Codaxy.Inventory.App.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.ElectronicDevices.Tags;

/// <summary>What creating and editing a tag take: its words, and the types that carry it.</summary>
public sealed record TagForm(
    [property:
        Required(ErrorMessage = "Give the tag a name."),
        StringLength(100, ErrorMessage = "A name is at most 100 characters.")
    ]
        string? Name,
    [property: StringLength(1000, ErrorMessage = "A description is at most 1000 characters.")]
        string? Description,
    IReadOnlyList<Guid>? TypeIds
);

/// <summary>A tag as its editor shows it.</summary>
public sealed record TagDetail(
    Guid Id,
    string Name,
    string? Description,
    IReadOnlyList<TypeRef> Types
);

public sealed record TypeRef(Guid Id, string Name);

internal static class Tags
{
    /// <summary>Every id names a type; the problem to answer with when one does not.</summary>
    public static async Task<IResult?> CheckTypesAsync(
        InventoryContext context,
        IReadOnlyList<Guid> ids,
        CancellationToken cancellationToken
    )
    {
        var known = await context
            .ElectronicDeviceTypes.Where(t => ids.Contains(t.Id))
            .CountAsync(cancellationToken);

        return known == ids.Count
            ? null
            : Results.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    ["typeIds"] = ["A type in the list no longer exists."],
                }
            );
    }

    /// <summary>
    /// No other tag holds the name, whatever its case — the original refused duplicates too. Checked
    /// here, not by an index: the schema is frozen, so two saves at once can still both pass.
    /// </summary>
    public static async Task<IResult?> CheckNameAsync(
        InventoryContext context,
        string name,
        Guid? except,
        CancellationToken cancellationToken
    )
    {
        var lower = name.Trim().ToLowerInvariant();
        var taken = await context.ElectronicDeviceTags.AnyAsync(
            t => t.Id != except && t.Name.ToLower() == lower,
            cancellationToken
        );

        return taken
            ? Results.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    ["name"] = ["A tag with this name already exists."],
                }
            )
            : null;
    }

    /// <summary>
    /// The tag's fields from the form, and its links made to match the form's types: missing ones
    /// added, extra ones removed, the rest untouched. The tag writes the link table as the type
    /// editor does — either side of a many-to-many owns the pairs.
    /// </summary>
    public static void Apply(
        InventoryContext context,
        ElectronicDeviceTag tag,
        TagForm form,
        IReadOnlyList<Guid> typeIds
    )
    {
        tag.Name = form.Name!.Trim();
        tag.Description = string.IsNullOrWhiteSpace(form.Description)
            ? null
            : form.Description.Trim();

        var links = tag.Types ??= [];

        foreach (var link in links.Where(l => !typeIds.Contains(l.ElectronicDeviceTypeId)).ToList())
        {
            links.Remove(link);
            context.Remove(link);
        }

        foreach (var id in typeIds.Where(id => links.All(l => l.ElectronicDeviceTypeId != id)))
            links.Add(
                new ElectronicDeviceTypeElectronicDeviceTag
                {
                    ElectronicDeviceTypeId = id,
                    ElectronicDeviceTag = tag,
                }
            );
    }

    public static Task<TagDetail?> DetailAsync(
        InventoryContext context,
        Guid id,
        CancellationToken cancellationToken
    ) =>
        context
            .ElectronicDeviceTags.AsNoTracking()
            .Where(t => t.Id == id)
            .Select(t => new TagDetail(
                t.Id,
                t.Name,
                t.Description,
                t.Types.OrderBy(l => l.ElectronicDeviceType.Name)
                    .Select(l => new TypeRef(l.ElectronicDeviceTypeId, l.ElectronicDeviceType.Name))
                    .ToList()
            ))
            .FirstOrDefaultAsync(cancellationToken);
}
