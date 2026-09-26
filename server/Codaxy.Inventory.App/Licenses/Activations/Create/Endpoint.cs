using Codaxy.Inventory.App.Licenses.Licenses;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Licenses.Activations.Create;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder activations) => activations.MapPost("/", Handle);

    /// <summary>
    /// Seats of a volume for one assignee: a person where the volume is per user, an electronic device
    /// whose type holds licences otherwise. Seats past the volume's quantity are allowed — the
    /// original only warned, and over-allocation is recorded rather than prevented.
    /// </summary>
    private static async Task<IResult> Handle(
        [FromBody] ActivationForm form,
        InventoryContext context,
        TimeProvider clock,
        CancellationToken cancellationToken
    )
    {
        if (!MiniValidator.IsValid(form, out var problem))
            return problem;

        var volumeType = await context
            .Volumes.Where(v => v.Id == form.VolumeId)
            .Select(v => (int?)v.VolumeTypeId)
            .FirstOrDefaultAsync(cancellationToken);

        if (volumeType is null)
            return Activations.Problem("volumeId", "That volume no longer exists.");

        if (volumeType == Activations.PerUser)
        {
            if (form.PersonId is null)
                return Activations.Problem("personId", "Choose who the seats are for.");

            if (!await context.Persons.AnyAsync(p => p.Id == form.PersonId, cancellationToken))
                return Activations.Problem("personId", "That person no longer exists.");

            if (form.DeviceId is not null)
                return Activations.Problem(
                    "deviceId",
                    "A per-user volume is activated for a person."
                );
        }
        else
        {
            if (form.DeviceId is null)
                return Activations.Problem("deviceId", "Choose the device the seats are for.");

            var holds = await context
                .ElectronicDevices.Where(d => d.AssetId == form.DeviceId)
                .Select(d =>
                    (bool?)(d.ElectronicDeviceType != null && d.ElectronicDeviceType.HoldLicences)
                )
                .FirstOrDefaultAsync(cancellationToken);

            if (holds is null)
                return Activations.Problem("deviceId", "That device no longer exists.");

            if (holds is false)
                return Activations.Problem(
                    "deviceId",
                    "Devices of this type cannot hold licences."
                );

            if (form.PersonId is not null)
                return Activations.Problem("personId", "This volume is activated for a device.");
        }

        var activation = new Activation
        {
            Id = Guid.CreateVersion7(),
            VolumeId = form.VolumeId!.Value,
            PersonId = form.PersonId,
            AssetId = form.DeviceId,
            Quantity = form.Quantity,
            ActivationDate = form.ActivationDate!.Value,
        };
        context.Activations.Add(activation);

        await context.SaveChangesAsync(cancellationToken);

        return Results.Created(
            $"/api/licenses/activations/{activation.Id}",
            await Activations.DetailAsync(
                context,
                activation.Id,
                Expiry.Today(clock),
                cancellationToken
            )
        );
    }
}
