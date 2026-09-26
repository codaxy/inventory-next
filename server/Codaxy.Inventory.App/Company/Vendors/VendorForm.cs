using System.ComponentModel.DataAnnotations;
using Codaxy.Inventory.App.Licenses.Licenses;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Assets;
using Codaxy.Inventory.App.Shared.Validation;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Company.Vendors;

/// <summary>What creating and editing a vendor take; everything but the name is optional.</summary>
public sealed record VendorForm(
    [property:
        Required(ErrorMessage = "Give the vendor a name."),
        StringLength(200, ErrorMessage = "A name is at most 200 characters.")
    ]
        string? Name,
    [property: StringLength(200, ErrorMessage = "At most 200 characters.")] string? Location,
    [property: StringLength(200, ErrorMessage = "At most 200 characters.")]
        string? RegistrationNumber,
    [property: StringLength(200, ErrorMessage = "At most 200 characters.")] string? VatNumber,
    [property: StringLength(500, ErrorMessage = "At most 500 characters.")] string? Web,
    [property: StringLength(200, ErrorMessage = "At most 200 characters.")] string? ContactPerson,
    [property: StringLength(200, ErrorMessage = "At most 200 characters.")] string? MobilePhone,
    [property: StringLength(200, ErrorMessage = "At most 200 characters.")] string? Phone,
    [property:
        StringLength(200, ErrorMessage = "At most 200 characters."),
        EmailAddress(ErrorMessage = "That is not an email address.")
    ]
        string? Email
);

/// <param name="Asset">What the contract covers.</param>
public sealed record ContractRow(
    Guid Id,
    Guid? AssetId,
    string? Asset,
    int? AssetNumber,
    string? ContractNumber,
    DateOnly? ExpirationDate
);

/// <summary>A vendor as its page shows it: its details, what was bought from it, its contracts.</summary>
public sealed record VendorDetail(
    Guid Id,
    string Name,
    string? Location,
    string? RegistrationNumber,
    string? VatNumber,
    string? Web,
    string? ContactPerson,
    string? MobilePhone,
    string? Phone,
    string? Email,
    AssetSections Assets,
    Section<ContractRow> Contracts
);

internal static class VendorWrites
{
    public static Task<IResult?> CheckAsync(
        InventoryContext context,
        VendorForm form,
        Guid? except,
        CancellationToken cancellationToken
    ) =>
        Unique.CheckAsync(
            context.Vendors.Where(v => v.Id != except),
            v => v.Name,
            form.Name!,
            "name",
            "A vendor with this name already exists.",
            cancellationToken
        );

    private static string? Text(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    public static void Apply(Vendor vendor, VendorForm form)
    {
        vendor.Name = form.Name!.Trim();
        vendor.Location = Text(form.Location);
        vendor.RegistrationNumber = Text(form.RegistrationNumber);
        vendor.VATNumber = Text(form.VatNumber);
        vendor.Web = Text(form.Web);
        vendor.ContactPerson = Text(form.ContactPerson);
        vendor.MobilePhone = Text(form.MobilePhone);
        vendor.Phone = Text(form.Phone);
        vendor.Email = Text(form.Email);
    }

    public static async Task<VendorDetail?> DetailAsync(
        InventoryContext context,
        Guid id,
        DateOnly today,
        CancellationToken cancellationToken
    )
    {
        var vendor = await context
            .Vendors.AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
        if (vendor is null)
            return null;

        return new VendorDetail(
            vendor.Id,
            vendor.Name,
            vendor.Location,
            vendor.RegistrationNumber,
            vendor.VATNumber,
            vendor.Web,
            vendor.ContactPerson,
            vendor.MobilePhone,
            vendor.Phone,
            vendor.Email,
            await AssetHoldings.SectionsAsync(
                context,
                a => a.VendorId == id,
                today,
                cancellationToken
            ),
            await AssetHoldings.SectionAsync(
                context.MaintenanceContracts.AsNoTracking().Where(c => c.VendorId == id),
                q =>
                    q.OrderBy(c => c.Asset.Name)
                        .ThenBy(c => c.Id)
                        .Select(c => new ContractRow(
                            c.Id,
                            c.AssetId,
                            c.Asset.Name,
                            c.Asset.InventoryNumber,
                            c.ContractNumber,
                            c.ExpirationDate
                        )),
                cancellationToken
            )
        );
    }

    public static DateOnly Today(TimeProvider clock) => Expiry.Today(clock);
}
