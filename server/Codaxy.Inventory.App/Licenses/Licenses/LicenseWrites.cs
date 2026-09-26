using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Assets;
using Codaxy.Inventory.App.Shared.Volumes;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Licenses.Licenses;

/// <summary>What creating, editing and reading a licence share.</summary>
internal static class LicenseWrites
{
    /// <summary>The seeded asset type every licence carries.</summary>
    public const string AssetType = "Licenses";

    private static IResult Problem(string field, string message) =>
        Results.ValidationProblem(new Dictionary<string, string[]> { [field] = [message] });

    /// <summary>Every id the form names exists, and every volume to add is complete.</summary>
    public static async Task<IResult?> CheckAsync(
        InventoryContext context,
        LicenseForm form,
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
                "licenseTypeId",
                form.LicenseTypeId,
                id => context.LicenseTypes.AnyAsync(x => x.Id == id, cancellationToken)
            ),
            (
                "licenseModelId",
                form.LicenseModelId,
                id => context.LicenseModels.AnyAsync(x => x.Id == id, cancellationToken)
            ),
            (
                "expirationModelId",
                form.ExpirationModelId,
                id => context.LicenseExpirationModels.AnyAsync(x => x.Id == id, cancellationToken)
            ),
            (
                "currencyId",
                form.CurrencyId,
                id => context.Currencies.AnyAsync(x => x.Id == id, cancellationToken)
            ),
            (
                "periodId",
                form.PeriodId,
                id => context.Periods.AnyAsync(x => x.Id == id, cancellationToken)
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
                return Problem(field, "That choice no longer exists.");

        foreach (var volume in (form.Volumes ?? []).Where(v => v.Id is null))
        {
            if (
                volume.SoftwareOrServiceId is null
                || volume.VolumeTypeId is null
                || volume.Quantity is null
            )
                return Problem(
                    "volumes",
                    "A volume needs a software or service, a type and a quantity."
                );

            if (volume.Quantity < 1)
                return Problem("volumes", "A volume has at least one seat.");

            if (volume.Description?.Length > 1000)
                return Problem("volumes", "A volume's description is at most 1000 characters.");

            if (
                !await context.SoftwareOrServices.AnyAsync(
                    s => s.Id == volume.SoftwareOrServiceId,
                    cancellationToken
                )
            )
                return Problem("volumes", "A volume's software or service no longer exists.");

            if (
                !await context.VolumeTypes.AnyAsync(
                    t => t.Id == volume.VolumeTypeId,
                    cancellationToken
                )
            )
                return Problem("volumes", "A volume's type no longer exists.");
        }

        return null;
    }

    private static string? Text(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    /// <summary>The form's fields onto the asset and the licence, the importance computed from the weights.</summary>
    public static async Task ApplyAsync(
        InventoryContext context,
        Asset asset,
        License license,
        LicenseForm form,
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
        asset.ImportanceId = await AssetWrites.ImportanceAsync(
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
        asset.LastModified = AssetWrites.Now(clock);

        license.LicenseTypeId = form.LicenseTypeId;
        license.LicenseModelId = form.LicenseModelId;
        license.LicenseExpirationModelId = form.ExpirationModelId;
        license.SubscriptionExpirationDate = form.ExpirationDate;
        license.SubscriptionFee = form.SubscriptionFee;
        license.CurrencyId = form.CurrencyId;
        license.PeriodId = form.PeriodId;
        license.AutoRenew = form.AutoRenew;
        license.ManagementConsoleUrl = Text(form.ManagementConsoleUrl);
        license.RegistrationNumber = Text(form.RegistrationNumber);
        license.KeyIdentifier = Text(form.KeyIdentifier);
    }

    /// <summary>What still stands on each of the given volumes, by volume: what keeps it from going.</summary>
    public static async Task<Dictionary<Guid, string>> HeldAsync(
        InventoryContext context,
        IReadOnlyCollection<Guid> volumes,
        CancellationToken cancellationToken
    )
    {
        var counts = await context
            .Volumes.Where(v => volumes.Contains(v.Id))
            .Select(v => new
            {
                v.Id,
                Software = v.SoftwareOrService.Name,
                Activations = v.Activations.Count,
                Clouds = context.Clouds.Count(c => c.VolumeId == v.Id),
                Softwares = context.Softwares.Count(s => s.VolumeId == v.Id),
            })
            .ToListAsync(cancellationToken);

        return counts
            .Select(c => (c.Id, Reason: Held(c.Software, c.Activations, c.Clouds, c.Softwares)))
            .Where(c => c.Reason is not null)
            .ToDictionary(c => c.Id, c => c.Reason!);
    }

    private static string? Held(string software, int activations, int clouds, int softwares)
    {
        var parts = new List<string>();
        if (activations > 0)
            parts.Add(activations == 1 ? "an activation" : $"{activations} activations");
        if (clouds > 0)
            parts.Add(clouds == 1 ? "a cloud" : $"{clouds} clouds");
        if (softwares > 0)
            parts.Add(softwares == 1 ? "a software entry" : $"{softwares} software entries");

        return parts.Count == 0 ? null : $"The {software} volume has {string.Join(" and ", parts)}";
    }

    /// <summary>
    /// The volumes made to match the form's: one without an id is added, one the form leaves out is
    /// removed, one it keeps stays as it was. A volume anything stands on is not removed — its foreign
    /// keys cascade, and would take the activations, clouds and software with it.
    /// </summary>
    public static async Task<IResult?> ReconcileVolumesAsync(
        InventoryContext context,
        License license,
        LicenseForm form,
        CancellationToken cancellationToken
    )
    {
        var volumes = license.Volumes ??= [];
        var kept = (form.Volumes ?? [])
            .Where(v => v.Id is not null)
            .Select(v => v.Id!.Value)
            .ToHashSet();
        var removed = volumes.Where(v => !kept.Contains(v.Id)).ToList();

        var held = await HeldAsync(context, removed.Select(v => v.Id).ToList(), cancellationToken);
        if (held.Count > 0)
            return Problem("volumes", $"{held.Values.First()}, so it cannot be removed.");

        foreach (var volume in removed)
        {
            volumes.Remove(volume);
            context.Volumes.Remove(volume);
        }

        // Added to the context, not only to the collection: a tracked licence's new child that already
        // carries its key is taken for an existing row, and saved as an update that matches nothing.
        foreach (var volume in (form.Volumes ?? []).Where(v => v.Id is null))
        {
            var added = new Volume
            {
                Id = Guid.CreateVersion7(),
                LicenseId = license.AssetId,
                SoftwareOrServiceId = volume.SoftwareOrServiceId!.Value,
                VolumeTypeId = volume.VolumeTypeId!.Value,
                Quantity = volume.Quantity!.Value,
                Description = Text(volume.Description),
            };
            volumes.Add(added);
            context.Volumes.Add(added);
        }

        return null;
    }

    public static async Task<LicenseDetail?> DetailAsync(
        InventoryContext context,
        Guid id,
        DateOnly today,
        CancellationToken cancellationToken
    )
    {
        var detail = await context
            .Licenses.AsNoTracking()
            .Where(l => l.AssetId == id)
            .Select(l => new LicenseDetail(
                l.AssetId,
                l.Asset.InventoryNumber,
                l.Asset.Name,
                l.Asset.InvoiceNumber,
                new Ref(l.Asset.VendorId, l.Asset.Vendor.Name),
                l.Asset.PurchaseValue,
                l.Asset.PurchaseDate,
                l.Asset.Description,
                new Ref(l.Asset.PersonId, l.Asset.Person.Name),
                l.Asset.ConfidentialityId == null
                    ? null
                    : new Ref(l.Asset.Confidentiality.Id, l.Asset.Confidentiality.Level),
                l.Asset.IntegrityId == null
                    ? null
                    : new Ref(l.Asset.Integrity.Id, l.Asset.Integrity.Level),
                l.Asset.AvailabilityId == null
                    ? null
                    : new Ref(l.Asset.Availability.Id, l.Asset.Availability.Level),
                l.Asset.ImportanceId == null
                    ? null
                    : new Ref(l.Asset.Importance.Id, l.Asset.Importance.Level),
                l.Asset.Incomplete,
                l.LicenseTypeId == null ? null : new Ref(l.LicenseType.Id, l.LicenseType.Text),
                l.LicenseModelId == null ? null : new Ref(l.LicenseModel.Id, l.LicenseModel.Text),
                l.LicenseExpirationModelId == null
                    ? null
                    : new Ref(l.LicenseExpirationModel.Id, l.LicenseExpirationModel.Text),
                l.SubscriptionExpirationDate,
                null,
                l.SubscriptionFee,
                l.CurrencyId == null ? null : new Ref(l.Currency.Id, l.Currency.Text),
                l.PeriodId == null ? null : new Ref(l.Period.Id, l.Period.Text),
                l.AutoRenew ?? false,
                l.Asset.BusinessEntityId == null
                    ? null
                    : new Ref(l.Asset.BusinessEntity.Id, l.Asset.BusinessEntity.Text),
                l.ManagementConsoleUrl,
                l.RegistrationNumber,
                l.KeyIdentifier,
                l.Asset.LocationId == null
                    ? null
                    : new Ref(l.Asset.Location.Id, l.Asset.Location.Name),
                l.Asset.URL,
                l.Asset.LastModified,
                l.Volumes.OrderBy(v => v.SoftwareOrService.Name)
                    .ThenBy(v => v.Id)
                    .Select(v => new VolumeDetail(
                        v.Id,
                        new Ref(v.SoftwareOrServiceId, v.SoftwareOrService.Name),
                        new VolumeTypeRef(v.VolumeTypeId, v.VolumeType.Text),
                        v.Quantity,
                        v.Description,
                        v.Activations.Where(a => a.DeactivationDate == null).Sum(a => a.Quantity),
                        v.Activations.Count,
                        null
                    ))
                    .ToList()
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (detail is null)
            return null;

        var held = await HeldAsync(
            context,
            detail.Volumes.Select(v => v.Id).ToList(),
            cancellationToken
        );

        return detail with
        {
            Expiry = Expiry.Status(detail.ExpirationDate, today),
            Volumes =
            [
                .. detail.Volumes.Select(v => v with { Held = held.GetValueOrDefault(v.Id) }),
            ],
        };
    }
}
