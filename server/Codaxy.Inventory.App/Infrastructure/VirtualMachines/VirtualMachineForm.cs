using System.ComponentModel.DataAnnotations;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Assets;
using Codaxy.Inventory.App.Shared.Validation;
using Codaxy.Inventory.App.Shared.Volumes;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Infrastructure.VirtualMachines;

/// <summary>What creating and editing a virtual machine take.</summary>
public sealed record VirtualMachineForm(
    [property:
        Required(ErrorMessage = "Give the virtual machine a name."),
        StringLength(200, ErrorMessage = "A name is at most 200 characters.")
    ]
        string? Name,
    [property: StringLength(100, ErrorMessage = "An address is at most 100 characters.")]
        string? IpAddress
);

public sealed record InformationRow(Guid Id, string Name, string? Type);

/// <summary>A virtual machine as its page shows it, with the information kept on it.</summary>
public sealed record VirtualMachineDetail(
    Guid Id,
    string Name,
    string? IpAddress,
    Section<InformationRow> Information
);

internal static class VirtualMachineWrites
{
    public static async Task<IResult?> CheckAsync(
        InventoryContext context,
        VirtualMachineForm form,
        Guid? except,
        CancellationToken cancellationToken
    )
    {
        return await Unique.CheckAsync(
            context.VirtualMachines.Where(t => t.Id != except),
            t => t.Name,
            form.Name!,
            "name",
            "A virtual machine with this name already exists.",
            cancellationToken
        );
    }

    private static string? Text(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    public static void Apply(VirtualMachine entity, VirtualMachineForm form)
    {
        entity.Name = form.Name!.Trim();
        entity.IPAddress = Text(form.IpAddress);
    }

    public static async Task<VirtualMachineDetail?> DetailAsync(
        InventoryContext context,
        Guid id,
        CancellationToken cancellationToken
    )
    {
        var entity = await context
            .VirtualMachines.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (entity is null)
            return null;

        return new VirtualMachineDetail(
            id,
            entity.Name,
            entity.IPAddress,
            await AssetHoldings.SectionAsync(
                context
                    .Informations.AsNoTracking()
                    .Where(i => i.InformationLocations.Any(l => l.VirtualMachineId == id)),
                q =>
                    q.OrderBy(i => i.Name)
                        .ThenBy(i => i.Id)
                        .Select(i => new InformationRow(i.Id, i.Name, i.InformationType.Name)),
                cancellationToken
            )
        );
    }
}
