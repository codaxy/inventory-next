using System.ComponentModel.DataAnnotations;
using Codaxy.Inventory.App.Licenses.Licenses;
using Codaxy.Inventory.App.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Licenses.Activations;

/// <summary>
/// What activating takes: seats of one volume, for a person where the volume is per user and for an
/// electronic device otherwise. Nothing else about an activation ever changes but its deactivation.
/// </summary>
public sealed record ActivationForm(
    [property: Required(ErrorMessage = "Choose a volume.")] Guid? VolumeId,
    Guid? PersonId,
    Guid? DeviceId,
    [property: Required(ErrorMessage = "Give the activation date.")] DateOnly? ActivationDate,
    [property: Range(1, 100000, ErrorMessage = "Activate at least one seat.")] int Quantity = 1
);

/// <param name="Date">On or after the day it was activated.</param>
public sealed record DeactivationForm(
    [property: Required(ErrorMessage = "Give the deactivation date.")] DateOnly? Date
);

public sealed record Named(Guid Id, string Name);

/// <param name="InUse">Seats of the volume taken by activations still active, this one included.</param>
public sealed record VolumeSummary(Guid Id, string Type, int TypeId, int Quantity, int InUse);

public sealed record LicenseSummary(
    Guid Id,
    int? Number,
    string Name,
    string? Vendor,
    string? Type,
    string? Model,
    string? ExpirationModel,
    DateOnly? ExpirationDate,
    string? Expiry,
    string? Location,
    string? Url
);

public sealed record DeviceRef(Guid Id, string Name, int? Number);

/// <summary>An activation as its page shows it, with the licence it draws on.</summary>
public sealed record ActivationDetail(
    Guid Id,
    Named Software,
    LicenseSummary License,
    VolumeSummary Volume,
    Named? Person,
    DeviceRef? Device,
    int Quantity,
    DateOnly ActivationDate,
    DateOnly? DeactivationDate
);

internal static class Activations
{
    public const int PerUser = 1;

    public static IResult Problem(string field, string message) =>
        Results.ValidationProblem(new Dictionary<string, string[]> { [field] = [message] });

    public static IResult Conflict(string title) =>
        Results.Problem(statusCode: StatusCodes.Status409Conflict, title: title);

    public static async Task<ActivationDetail?> DetailAsync(
        InventoryContext context,
        Guid id,
        DateOnly today,
        CancellationToken cancellationToken
    )
    {
        var row = await context
            .Activations.AsNoTracking()
            .Where(a => a.Id == id)
            .Select(a => new
            {
                a.Id,
                Software = new Named(a.Volume.SoftwareOrServiceId, a.Volume.SoftwareOrService.Name),
                License = new
                {
                    Id = a.Volume.LicenseId,
                    a.Volume.License.Asset.InventoryNumber,
                    a.Volume.License.Asset.Name,
                    Vendor = a.Volume.License.Asset.Vendor.Name,
                    Type = a.Volume.License.LicenseType.Text,
                    Model = a.Volume.License.LicenseModel.Text,
                    ExpirationModel = a.Volume.License.LicenseExpirationModel.Text,
                    a.Volume.License.SubscriptionExpirationDate,
                    Location = a.Volume.License.Asset.Location.Name,
                    a.Volume.License.Asset.URL,
                },
                Volume = new VolumeSummary(
                    a.VolumeId,
                    a.Volume.VolumeType.Text,
                    a.Volume.VolumeTypeId,
                    a.Volume.Quantity,
                    a.Volume.Activations.Where(o => o.DeactivationDate == null).Sum(o => o.Quantity)
                ),
                Person = a.PersonId == null ? null : new Named(a.Person.Id, a.Person.Name),
                Device = a.AssetId == null
                    ? null
                    : new DeviceRef(a.Asset.Id, a.Asset.Name, a.Asset.InventoryNumber),
                a.Quantity,
                a.ActivationDate,
                a.DeactivationDate,
            })
            .FirstOrDefaultAsync(cancellationToken);

        return row is null
            ? null
            : new ActivationDetail(
                row.Id,
                row.Software,
                new LicenseSummary(
                    row.License.Id,
                    row.License.InventoryNumber,
                    row.License.Name,
                    row.License.Vendor,
                    row.License.Type,
                    row.License.Model,
                    row.License.ExpirationModel,
                    row.License.SubscriptionExpirationDate,
                    Expiry.Status(row.License.SubscriptionExpirationDate, today),
                    row.License.Location,
                    row.License.URL
                ),
                row.Volume,
                row.Person,
                row.Device,
                row.Quantity,
                row.ActivationDate,
                row.DeactivationDate
            );
    }
}
