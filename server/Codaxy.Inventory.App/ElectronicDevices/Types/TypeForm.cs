using System.ComponentModel.DataAnnotations;
using Codaxy.Inventory.App.ElectronicDevices.Tags;
using Codaxy.Inventory.App.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.ElectronicDevices.Types;

/// <summary>What creating and editing a type take: its words, whether it holds licences, its tags.</summary>
public sealed record TypeForm(
    [property:
        Required(ErrorMessage = "Give the type a name."),
        StringLength(100, ErrorMessage = "A name is at most 100 characters.")
    ]
        string? Name,
    bool HoldsLicences,
    [property: StringLength(1000, ErrorMessage = "A description is at most 1000 characters.")]
        string? Description,
    IReadOnlyList<Guid>? TagIds
);

/// <summary>A type as its page shows it.</summary>
public sealed record TypeDetail(
    Guid Id,
    string Name,
    bool HoldsLicences,
    string? Description,
    IReadOnlyList<TagRef> Tags,
    int DeviceCount
);

public sealed record TagRef(Guid Id, string Name);

internal static class DeviceTypes
{
    /// <summary>Every id names a tag; the problem to answer with when one does not.</summary>
    public static async Task<IResult?> CheckTagsAsync(
        InventoryContext context,
        IReadOnlyList<Guid> ids,
        CancellationToken cancellationToken
    )
    {
        var known = await context
            .ElectronicDeviceTags.Where(t => ids.Contains(t.Id))
            .CountAsync(cancellationToken);

        return known == ids.Count
            ? null
            : Results.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    ["tagIds"] = ["A tag in the list no longer exists."],
                }
            );
    }

    /// <summary>
    /// No other type holds the name, whatever its case — the original refused duplicates too. Checked
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
        var taken = await context.ElectronicDeviceTypes.AnyAsync(
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

    /// <summary>
    /// The type's fields from the form, and its links made to match the form's tags: missing ones
    /// added, extra ones removed, the rest untouched. The tag editor writes the same pairs from its
    /// side.
    /// </summary>
    public static void Apply(
        InventoryContext context,
        ElectronicDeviceType type,
        TypeForm form,
        IReadOnlyList<Guid> tagIds
    )
    {
        type.Name = form.Name!.Trim();
        type.HoldLicences = form.HoldsLicences;
        type.Description = string.IsNullOrWhiteSpace(form.Description)
            ? null
            : form.Description.Trim();

        var links = type.Tags ??= [];

        foreach (var link in links.Where(l => !tagIds.Contains(l.ElectronicDeviceTagId)).ToList())
        {
            links.Remove(link);
            context.Remove(link);
        }

        foreach (var id in tagIds.Where(id => links.All(l => l.ElectronicDeviceTagId != id)))
            links.Add(
                new ElectronicDeviceTypeElectronicDeviceTag
                {
                    ElectronicDeviceTagId = id,
                    ElectronicDeviceType = type,
                }
            );
    }

    public static Task<TypeDetail?> DetailAsync(
        InventoryContext context,
        Guid id,
        CancellationToken cancellationToken
    ) =>
        context
            .ElectronicDeviceTypes.AsNoTracking()
            .Where(t => t.Id == id)
            .Select(t => new TypeDetail(
                t.Id,
                t.Name,
                t.HoldLicences,
                t.Description,
                t.Tags.OrderBy(l => l.ElectronicDeviceTag.Name)
                    .Select(l => new TagRef(l.ElectronicDeviceTagId, l.ElectronicDeviceTag.Name))
                    .ToList(),
                context.ElectronicDevices.Count(d => d.ElectronicDeviceTypeId == t.Id)
            ))
            .FirstOrDefaultAsync(cancellationToken);
}
