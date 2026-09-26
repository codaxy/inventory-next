using System.ComponentModel.DataAnnotations;
using Codaxy.Inventory.App.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Licenses.SoftwareServices;

/// <summary>What creating and editing a software or service take.</summary>
public sealed record SoftwareOrServiceForm(
    [property:
        Required(ErrorMessage = "Give it a name."),
        StringLength(200, ErrorMessage = "A name is at most 200 characters.")
    ]
        string? Name,
    [property: Required(ErrorMessage = "Choose a category.")] Guid? CategoryId,
    [property: Required(ErrorMessage = "Choose a manufacturer.")] Guid? ManufacturerId,
    [property: StringLength(500, ErrorMessage = "A URL is at most 500 characters.")] string? Url
);

/// <summary>A software or service as its page shows it.</summary>
public sealed record SoftwareOrServiceDetail(
    Guid Id,
    string Name,
    Ref Category,
    Ref Manufacturer,
    string? Url,
    int VolumeCount
);

public sealed record Ref(Guid Id, string Name);

internal static class SoftwareServices
{
    private static IResult Problem(string field, string message) =>
        Results.ValidationProblem(new Dictionary<string, string[]> { [field] = [message] });

    /// <summary>
    /// The category and manufacturer exist, and no other entry holds the name, whatever its case —
    /// the original's grid refused a duplicate. Checked here, not by an index: the schema is frozen,
    /// so two saves at once can still both pass.
    /// </summary>
    public static async Task<IResult?> CheckAsync(
        InventoryContext context,
        SoftwareOrServiceForm form,
        Guid? except,
        CancellationToken cancellationToken
    )
    {
        if (
            !await context.SoftwareOrServiceCategories.AnyAsync(
                c => c.Id == form.CategoryId,
                cancellationToken
            )
        )
            return Problem("categoryId", "That category no longer exists.");

        if (
            !await context.Manufacturers.AnyAsync(
                m => m.Id == form.ManufacturerId,
                cancellationToken
            )
        )
            return Problem("manufacturerId", "That manufacturer no longer exists.");

        var lower = form.Name!.Trim().ToLowerInvariant();
        if (
            await context.SoftwareOrServices.AnyAsync(
                s => s.Id != except && s.Name.ToLower() == lower,
                cancellationToken
            )
        )
            return Problem("name", "A software or service with this name already exists.");

        return null;
    }

    public static void Apply(SoftwareOrService entry, SoftwareOrServiceForm form)
    {
        entry.Name = form.Name!.Trim();
        entry.SoftwareOrServiceCategoryId = form.CategoryId!.Value;
        entry.ManufacturerId = form.ManufacturerId!.Value;
        entry.Url = string.IsNullOrWhiteSpace(form.Url) ? null : form.Url.Trim();
    }

    public static Task<SoftwareOrServiceDetail?> DetailAsync(
        InventoryContext context,
        Guid id,
        CancellationToken cancellationToken
    ) =>
        context
            .SoftwareOrServices.AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => new SoftwareOrServiceDetail(
                s.Id,
                s.Name,
                new Ref(s.SoftwareOrServiceCategoryId, s.SoftwareOrServiceCategory.Name),
                new Ref(s.ManufacturerId, s.Manufacturer.Name),
                s.Url,
                s.Volumes.Count
            ))
            .FirstOrDefaultAsync(cancellationToken);
}
