using System.ComponentModel.DataAnnotations;
using Codaxy.Inventory.App.Informations.Tags;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Assets;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Informations.Items;

/// <summary>
/// Where a piece of information is kept: exactly one of a device, a virtual machine, a software
/// entry, a cloud subscription, a physical location or an address. A saved one is named by its id alone — kept as
/// it is — and a new one by its kind and target.
/// </summary>
/// <param name="Kind"><c>device</c>, <c>virtualMachine</c>, <c>software</c>, <c>cloudSubscription</c>, <c>location</c> or <c>url</c>.</param>
public sealed record InformationLocationForm(Guid? Id, string? Kind, Guid? TargetId, string? Url);

/// <summary>What creating and editing a piece of information take; the importance is the server's.</summary>
public sealed record InformationForm(
    [property:
        Required(ErrorMessage = "Give the information a name."),
        StringLength(200, ErrorMessage = "A name is at most 200 characters.")
    ]
        string? Name,
    [property: Required(ErrorMessage = "Choose the type.")] Guid? TypeId,
    [property: Required(ErrorMessage = "Choose the assignee.")] Guid? PersonId,
    [property: StringLength(200, ErrorMessage = "At most 200 characters.")] string? Author,
    [property: StringLength(1000, ErrorMessage = "At most 1000 characters.")] string? AccessRights,
    bool PersonalInformation,
    bool ClientsPersonalInformation,
    bool Incomplete,
    [property: StringLength(1000, ErrorMessage = "A description is at most 1000 characters.")]
        string? Description,
    [property: StringLength(1000, ErrorMessage = "A note is at most 1000 characters.")]
        string? Note,
    Guid? ConfidentialityId,
    Guid? IntegrityId,
    Guid? AvailabilityId,
    Guid? ProjectId,
    IReadOnlyList<Guid>? TagIds,
    IReadOnlyList<InformationLocationForm>? Locations
);

/// <param name="Target">The device's, machine's, software's, cloud subscription's or place's name, or the address.</param>
/// <param name="Number">A device's inventory number.</param>
public sealed record LocationRow(Guid Id, string Kind, Guid? TargetId, string Target, int? Number);

public sealed record InformationDetail(
    Guid Id,
    string Name,
    AssetRef Type,
    AssetRef Person,
    string? Author,
    string? AccessRights,
    bool PersonalInformation,
    bool ClientsPersonalInformation,
    bool Incomplete,
    string? Description,
    string? Note,
    AssetRef? Confidentiality,
    AssetRef? Integrity,
    AssetRef? Availability,
    AssetRef? Importance,
    AssetRef? Project,
    IReadOnlyList<AssetRef> Tags,
    IReadOnlyList<LocationRow> Locations
);

internal static class InformationWrites
{
    public static readonly string[] Kinds =
    [
        "device",
        "virtualMachine",
        "software",
        "cloudSubscription",
        "location",
        "url",
    ];

    private static string? Text(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    /// <summary>Every choice exists; every new location is one kind with one target; a kept one is this record's.</summary>
    public static async Task<IResult?> CheckAsync(
        InventoryContext context,
        InformationForm form,
        Guid? id,
        CancellationToken cancellationToken
    )
    {
        var errors = new Dictionary<string, string[]>();
        void Missing(string field) => errors[field] = ["That choice no longer exists."];

        if (!await context.InformationTypes.AnyAsync(t => t.Id == form.TypeId, cancellationToken))
            Missing("typeId");
        if (!await context.Persons.AnyAsync(p => p.Id == form.PersonId, cancellationToken))
            Missing("personId");
        if (
            form.ProjectId is { } project
            && !await context.Projects.AnyAsync(p => p.Id == project, cancellationToken)
        )
            Missing("projectId");
        if (
            form.ConfidentialityId is { } c
            && !await context.Confidentialities.AnyAsync(x => x.Id == c, cancellationToken)
        )
            Missing("confidentialityId");
        if (
            form.IntegrityId is { } i
            && !await context.Integrities.AnyAsync(x => x.Id == i, cancellationToken)
        )
            Missing("integrityId");
        if (
            form.AvailabilityId is { } a
            && !await context.Availabilities.AnyAsync(x => x.Id == a, cancellationToken)
        )
            Missing("availabilityId");

        var tags = (form.TagIds ?? []).Distinct().ToList();
        if (
            await context.InformationTags.CountAsync(t => tags.Contains(t.Id), cancellationToken)
            != tags.Count
        )
            Missing("tagIds");

        var saved = id is null
            ? []
            : await context
                .InformationLocations.Where(l => l.InformationId == id)
                .Select(l => l.Id)
                .ToListAsync(cancellationToken);
        foreach (var location in form.Locations ?? [])
        {
            if (location.Id is { } kept)
            {
                if (!saved.Contains(kept))
                    errors["locations"] = ["A location is no longer this information's."];
                continue;
            }
            var problem = location.Kind switch
            {
                "url" => string.IsNullOrWhiteSpace(location.Url) ? "Give the address."
                : location.Url.Trim().Length > 500 ? "An address is at most 500 characters."
                : null,
                "device" => await context.ElectronicDevices.AnyAsync(
                    d => d.AssetId == location.TargetId,
                    cancellationToken
                )
                    ? null
                    : "Choose the device.",
                "virtualMachine" => await context.VirtualMachines.AnyAsync(
                    v => v.Id == location.TargetId,
                    cancellationToken
                )
                    ? null
                    : "Choose the virtual machine.",
                "software" => await context.Softwares.AnyAsync(
                    s => s.Id == location.TargetId,
                    cancellationToken
                )
                    ? null
                    : "Choose the software.",
                "cloudSubscription" => await context.Clouds.AnyAsync(
                    x => x.Id == location.TargetId,
                    cancellationToken
                )
                    ? null
                    : "Choose the cloud subscription.",
                "location" => await context.Locations.AnyAsync(
                    l => l.Id == location.TargetId,
                    cancellationToken
                )
                    ? null
                    : "Choose the location.",
                _ => "Choose where it is kept.",
            };
            if (problem is not null)
                errors["locations"] = [problem];
        }

        return errors.Count > 0 ? Results.ValidationProblem(errors) : null;
    }

    public static async Task ApplyAsync(
        InventoryContext context,
        Information information,
        InformationForm form,
        CancellationToken cancellationToken
    )
    {
        information.Name = form.Name!.Trim();
        information.InformationTypeId = form.TypeId!.Value;
        information.PersonId = form.PersonId!.Value;
        information.Author = Text(form.Author);
        information.AccessRights = Text(form.AccessRights);
        information.PersonalInformation = form.PersonalInformation;
        information.ClientsPersonalInformation = form.ClientsPersonalInformation;
        information.Incomplete = form.Incomplete;
        information.Description = Text(form.Description);
        information.Note = Text(form.Note);
        information.ConfidentialityId = form.ConfidentialityId;
        information.IntegrityId = form.IntegrityId;
        information.AvailabilityId = form.AvailabilityId;
        information.ImportanceId = await AssetWrites.ImportanceAsync(
            context,
            form.ConfidentialityId,
            form.IntegrityId,
            form.AvailabilityId,
            cancellationToken
        );
        information.ProjectId = form.ProjectId;

        // The tag links: those no longer chosen go, the newly chosen are added.
        var chosen = (form.TagIds ?? []).Distinct().ToHashSet();
        var links = await context
            .InformationTagInformations.Where(l => l.InformationId == information.Id)
            .ToListAsync(cancellationToken);
        context.InformationTagInformations.RemoveRange(
            links.Where(l => !chosen.Contains(l.InformationTagId))
        );
        foreach (var tag in chosen.Except(links.Select(l => l.InformationTagId)))
            context.InformationTagInformations.Add(
                new InformationTagInformation
                {
                    InformationId = information.Id,
                    InformationTagId = tag,
                }
            );

        // Saved locations kept by id, the rest removed; new ones added — to the context itself, since
        // a child with its key already set would otherwise be taken for an existing row.
        var kept = (form.Locations ?? [])
            .Where(l => l.Id is not null)
            .Select(l => l.Id!.Value)
            .ToHashSet();
        var saved = await context
            .InformationLocations.Where(l => l.InformationId == information.Id)
            .ToListAsync(cancellationToken);
        context.InformationLocations.RemoveRange(saved.Where(l => !kept.Contains(l.Id)));
        foreach (var l in (form.Locations ?? []).Where(l => l.Id is null))
            context.InformationLocations.Add(
                new InformationLocation
                {
                    Id = Guid.CreateVersion7(),
                    InformationId = information.Id,
                    ElectronicDeviceId = l.Kind == "device" ? l.TargetId : null,
                    VirtualMachineId = l.Kind == "virtualMachine" ? l.TargetId : null,
                    SoftwareId = l.Kind == "software" ? l.TargetId : null,
                    CloudId = l.Kind == "cloudSubscription" ? l.TargetId : null,
                    PhysicalLocationId = l.Kind == "location" ? l.TargetId : null,
                    URL = l.Kind == "url" ? l.Url!.Trim() : null,
                }
            );
    }

    public static async Task<InformationDetail?> DetailAsync(
        InventoryContext context,
        Guid id,
        CancellationToken cancellationToken
    )
    {
        var i = await context
            .Informations.AsNoTracking()
            .Where(i => i.Id == id)
            .Select(i => new InformationDetail(
                i.Id,
                i.Name,
                new AssetRef(i.InformationTypeId, i.InformationType.Name),
                new AssetRef(i.PersonId, i.Person.Name),
                i.Author,
                i.AccessRights,
                i.PersonalInformation == true,
                i.ClientsPersonalInformation == true,
                i.Incomplete == true,
                i.Description,
                i.Note,
                i.ConfidentialityId == null
                    ? null
                    : new AssetRef(i.ConfidentialityId.Value, i.Confidentiality.Level),
                i.IntegrityId == null ? null : new AssetRef(i.IntegrityId.Value, i.Integrity.Level),
                i.AvailabilityId == null
                    ? null
                    : new AssetRef(i.AvailabilityId.Value, i.Availability.Level),
                i.ImportanceId == null
                    ? null
                    : new AssetRef(i.ImportanceId.Value, i.Importance.Level),
                i.ProjectId == null ? null : new AssetRef(i.ProjectId.Value, i.Project.Name),
                i.Tags.OrderBy(t => t.InformationTag.Name)
                    .Select(t => new AssetRef(t.InformationTagId, t.InformationTag.Name))
                    .ToList(),
                i.InformationLocations.OrderBy(l => l.Id)
                    .Select(l => new LocationRow(
                        l.Id,
                        l.ElectronicDeviceId != null ? "device"
                            : l.VirtualMachineId != null ? "virtualMachine"
                            : l.SoftwareId != null ? "software"
                            : l.CloudId != null ? "cloudSubscription"
                            : l.PhysicalLocationId != null ? "location"
                            : "url",
                        l.ElectronicDeviceId
                            ?? l.VirtualMachineId
                            ?? l.SoftwareId
                            ?? l.CloudId
                            ?? l.PhysicalLocationId,
                        l.ElectronicDeviceId != null ? l.ElectronicDevice.Asset.Name
                            : l.VirtualMachineId != null ? l.VirtualMachine.Name
                            : l.SoftwareId != null ? l.Software.Name
                            : l.CloudId != null ? l.Cloud.Name
                            : l.PhysicalLocationId != null ? l.PhysicalLocation.Name
                            : l.URL ?? "",
                        l.ElectronicDeviceId != null
                            ? l.ElectronicDevice.Asset.InventoryNumber
                            : null
                    ))
                    .ToList()
            ))
            .FirstOrDefaultAsync(cancellationToken);
        return i;
    }
}
