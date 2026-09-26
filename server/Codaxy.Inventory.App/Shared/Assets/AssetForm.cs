using Codaxy.Inventory.App.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Shared.Assets;

/// <summary>
/// The asset's fields, as every asset's form carries them flat beside its own: a licence's, a piece of
/// furniture's. The number, the type and the importance are the server's; the last-modified time is
/// echoed on an update so an edit made meanwhile is not overwritten.
/// </summary>
public interface IAssetForm
{
    string? Name { get; }
    string? InvoiceNumber { get; }
    Guid? VendorId { get; }
    decimal? PurchaseValue { get; }
    DateOnly? PurchaseDate { get; }
    string? Description { get; }
    Guid? PersonId { get; }
    Guid? ConfidentialityId { get; }
    Guid? IntegrityId { get; }
    Guid? AvailabilityId { get; }
    bool Incomplete { get; }
    Guid? BusinessEntityId { get; }
    Guid? LocationId { get; }
    string? Url { get; }
    DateTimeOffset? LastModified { get; }
}

public sealed record AssetRef(Guid Id, string Name);

public sealed record AssetOption(Guid Id, string Text);

/// <param name="Weight">What the importance is computed from.</param>
public sealed record WeightedOption(Guid Id, string Text, int Weight);

/// <summary>The pickers every asset form shows.</summary>
public sealed record AssetOptions(
    IReadOnlyList<AssetOption> Vendors,
    IReadOnlyList<AssetOption> People,
    IReadOnlyList<WeightedOption> Confidentialities,
    IReadOnlyList<WeightedOption> Integrities,
    IReadOnlyList<WeightedOption> Availabilities,
    IReadOnlyList<AssetOption> BusinessEntities,
    IReadOnlyList<AssetOption> Locations
);

public static partial class AssetWrites
{
    private static string? Text(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    /// <summary>
    /// The asset's rules, added to the errors the form's own attributes found: one set of messages for
    /// every kind of asset.
    /// </summary>
    public static Dictionary<string, string[]> Validate(
        IAssetForm form,
        Dictionary<string, string[]> errors
    )
    {
        void Add(string field, string message) => errors.TryAdd(field, [message]);

        if (string.IsNullOrWhiteSpace(form.Name))
            Add("name", "Give it a name.");
        else if (form.Name.Trim().Length > 300)
            Add("name", "A name is at most 300 characters.");

        if (form.InvoiceNumber?.Length > 200)
            Add("invoiceNumber", "An invoice number is at most 200 characters.");
        if (form.VendorId is null)
            Add("vendorId", "Choose the vendor.");
        if (form.PurchaseValue is null)
            Add("purchaseValue", "Give the purchase value.");
        else if (form.PurchaseValue is < 0 or > 1_000_000_000)
            Add("purchaseValue", "A purchase value is not negative.");
        if (form.PurchaseDate is null)
            Add("purchaseDate", "Give the purchase date.");
        if (form.Description?.Length > 1000)
            Add("description", "A description is at most 1000 characters.");
        if (form.PersonId is null)
            Add("personId", "Choose who holds it.");
        if (form.Url?.Length > 500)
            Add("url", "A URL is at most 500 characters.");

        return errors;
    }

    /// <summary>Every id the asset's fields name exists; the problem to answer with when one does not.</summary>
    public static async Task<IResult?> CheckAsync(
        InventoryContext context,
        IAssetForm form,
        CancellationToken cancellationToken
    )
    {
        var checks = new (string Field, Guid? Id, Func<Guid, Task<bool>> Exists)[]
        {
            (
                "vendorId",
                form.VendorId,
                id => context.Vendors.AnyAsync(x => x.Id == id, cancellationToken)
            ),
            (
                "personId",
                form.PersonId,
                id => context.Persons.AnyAsync(x => x.Id == id, cancellationToken)
            ),
            (
                "confidentialityId",
                form.ConfidentialityId,
                id => context.Confidentialities.AnyAsync(x => x.Id == id, cancellationToken)
            ),
            (
                "integrityId",
                form.IntegrityId,
                id => context.Integrities.AnyAsync(x => x.Id == id, cancellationToken)
            ),
            (
                "availabilityId",
                form.AvailabilityId,
                id => context.Availabilities.AnyAsync(x => x.Id == id, cancellationToken)
            ),
            (
                "businessEntityId",
                form.BusinessEntityId,
                id => context.BusinessEntities.AnyAsync(x => x.Id == id, cancellationToken)
            ),
            (
                "locationId",
                form.LocationId,
                id => context.Locations.AnyAsync(x => x.Id == id, cancellationToken)
            ),
        };

        foreach (var (field, id, exists) in checks)
            if (id is { } value && !await exists(value))
                return Results.ValidationProblem(
                    new Dictionary<string, string[]> { [field] = ["That choice no longer exists."] }
                );

        return null;
    }

    /// <summary>The form's asset fields onto the asset, the importance computed from the weights.</summary>
    public static async Task ApplyAsync(
        InventoryContext context,
        Asset asset,
        IAssetForm form,
        TimeProvider clock,
        CancellationToken cancellationToken
    )
    {
        asset.Name = form.Name!.Trim();
        asset.InvoiceNumber = Text(form.InvoiceNumber);
        asset.VendorId = form.VendorId!.Value;
        asset.PurchaseValue = form.PurchaseValue!.Value;
        asset.PurchaseDate = form.PurchaseDate!.Value;
        asset.Description = Text(form.Description);
        asset.PersonId = form.PersonId!.Value;
        asset.ConfidentialityId = form.ConfidentialityId;
        asset.IntegrityId = form.IntegrityId;
        asset.AvailabilityId = form.AvailabilityId;
        asset.ImportanceId = await ImportanceAsync(
            context,
            form.ConfidentialityId,
            form.IntegrityId,
            form.AvailabilityId,
            cancellationToken
        );
        asset.Incomplete = form.Incomplete;
        asset.BusinessEntityId = form.BusinessEntityId;
        asset.LocationId = form.LocationId;
        asset.URL = Text(form.Url);
        asset.LastModified = Now(clock);
    }

    /// <summary>
    /// An update's precondition: the time the form was loaded with is still the asset's. Absent, it is
    /// a validation problem; changed, someone saved since and it is a conflict.
    /// </summary>
    public static IResult? CheckUnchanged(Asset asset, IAssetForm form, string what) =>
        form.LastModified is null
            ? Results.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    ["lastModified"] = [$"Send the last-modified time the {what} was loaded with."],
                }
            )
        : asset.LastModified != form.LastModified
            ? Results.Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: $"Someone changed this {what} since you opened it."
            )
        : null;

    public static async Task<AssetOptions> OptionsAsync(
        InventoryContext context,
        CancellationToken cancellationToken
    ) =>
        new(
            await context
                .Vendors.AsNoTracking()
                .OrderBy(x => x.Name)
                .Select(x => new AssetOption(x.Id, x.Name))
                .ToListAsync(cancellationToken),
            await context
                .Persons.AsNoTracking()
                .OrderBy(x => x.Name)
                .Select(x => new AssetOption(x.Id, x.Name))
                .ToListAsync(cancellationToken),
            await context
                .Confidentialities.AsNoTracking()
                .OrderBy(x => x.Weight)
                .Select(x => new WeightedOption(x.Id, x.Level, x.Weight))
                .ToListAsync(cancellationToken),
            await context
                .Integrities.AsNoTracking()
                .OrderBy(x => x.Weight)
                .Select(x => new WeightedOption(x.Id, x.Level, x.Weight))
                .ToListAsync(cancellationToken),
            await context
                .Availabilities.AsNoTracking()
                .OrderBy(x => x.Weight)
                .Select(x => new WeightedOption(x.Id, x.Level, x.Weight))
                .ToListAsync(cancellationToken),
            await context
                .BusinessEntities.AsNoTracking()
                .OrderBy(x => x.Text)
                .Select(x => new AssetOption(x.Id, x.Text))
                .ToListAsync(cancellationToken),
            await context
                .Locations.AsNoTracking()
                .OrderBy(x => x.Name)
                .Select(x => new AssetOption(x.Id, x.Name))
                .ToListAsync(cancellationToken)
        );
}
