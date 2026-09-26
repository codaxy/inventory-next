using System.ComponentModel.DataAnnotations;
using Codaxy.Inventory.App.Licenses.Licenses;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Assets;
using Codaxy.Inventory.App.Shared.Validation;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Company.Locations;

/// <summary>What creating and editing a location take, in the schema's lengths.</summary>
public sealed record LocationForm(
    [property:
        Required(ErrorMessage = "Give the location a name."),
        StringLength(50, ErrorMessage = "A name is at most 50 characters.")
    ]
        string? Name,
    [property: StringLength(1000, ErrorMessage = "A description is at most 1000 characters.")]
        string? Description,
    [property: Required(ErrorMessage = "Choose the country.")] string? CountryCode,
    Guid? StateId,
    [property: Required(ErrorMessage = "Choose the city.")] Guid? CityId,
    [property: StringLength(8, ErrorMessage = "A postal code is at most 8 characters.")]
        string? PostalCode,
    [property:
        Required(ErrorMessage = "Give the street."),
        StringLength(50, ErrorMessage = "A street is at most 50 characters.")
    ]
        string? Street,
    [property: Range(0, 100000, ErrorMessage = "A house number is a whole number, 0 or more.")]
        int? HouseNumber,
    [property: Range(-20, 500, ErrorMessage = "A floor is between -20 and 500.")] int? Floor,
    [property: StringLength(30, ErrorMessage = "A room is at most 30 characters.")] string? Room
);

public sealed record CountryRef(string Code, string Name);

public sealed record InformationRow(Guid Id, string Name, string? Type);

/// <summary>A location as its page shows it: the address, the assets there, the information stored there.</summary>
public sealed record LocationDetail(
    Guid Id,
    string Name,
    string? Description,
    CountryRef Country,
    AssetRef? State,
    AssetRef City,
    string? PostalCode,
    string? Street,
    int? HouseNumber,
    int? Floor,
    string? Room,
    AssetSections Assets,
    Section<InformationRow> Information
);

internal static class LocationWrites
{
    /// <summary>Every choice exists, and the city and state are of the chosen country; then the name is free.</summary>
    public static async Task<IResult?> CheckAsync(
        InventoryContext context,
        LocationForm form,
        Guid? except,
        CancellationToken cancellationToken
    )
    {
        var errors = new Dictionary<string, string[]>();
        if (!await context.Countries.AnyAsync(c => c.Code == form.CountryCode, cancellationToken))
            errors["countryCode"] = ["That choice no longer exists."];
        else
        {
            var city = await context.Cities.FirstOrDefaultAsync(
                c => c.Id == form.CityId,
                cancellationToken
            );
            if (city is null)
                errors["cityId"] = ["That choice no longer exists."];
            else if (city.CountryCode != form.CountryCode)
                errors["cityId"] = ["That city is not in the chosen country."];

            if (form.StateId is { } stateId)
            {
                var state = await context.States.FirstOrDefaultAsync(
                    s => s.Id == stateId,
                    cancellationToken
                );
                if (state is null)
                    errors["stateId"] = ["That choice no longer exists."];
                else if (state.CountryCode != form.CountryCode)
                    errors["stateId"] = ["That state is not in the chosen country."];
            }
        }
        if (errors.Count > 0)
            return Results.ValidationProblem(errors);

        return await Unique.CheckAsync(
            context.Locations.Where(l => l.Id != except),
            l => l.Name,
            form.Name!,
            "name",
            "A location with this name already exists.",
            cancellationToken
        );
    }

    private static string? Text(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    public static void Apply(Location location, LocationForm form)
    {
        location.Name = form.Name!.Trim();
        location.Description = Text(form.Description);
        location.CountryCode = form.CountryCode!;
        location.StateId = form.StateId;
        location.CityId = form.CityId!.Value;
        location.PostalCode = Text(form.PostalCode);
        location.Street = Text(form.Street);
        location.HouseNumber = form.HouseNumber;
        location.Floor = form.Floor;
        location.Room = Text(form.Room);
    }

    public static async Task<LocationDetail?> DetailAsync(
        InventoryContext context,
        Guid id,
        TimeProvider clock,
        CancellationToken cancellationToken
    )
    {
        var l = await context
            .Locations.AsNoTracking()
            .Where(l => l.Id == id)
            .Select(l => new
            {
                l.Name,
                l.Description,
                Country = new CountryRef(l.CountryCode, l.Country.Name),
                State = l.StateId == null ? null : new AssetRef(l.StateId.Value, l.State.Name),
                City = new AssetRef(l.CityId, l.City.Name),
                l.PostalCode,
                l.Street,
                l.HouseNumber,
                l.Floor,
                l.Room,
            })
            .FirstOrDefaultAsync(cancellationToken);
        if (l is null)
            return null;

        return new LocationDetail(
            id,
            l.Name,
            l.Description,
            l.Country,
            l.State,
            l.City,
            l.PostalCode,
            l.Street,
            l.HouseNumber,
            l.Floor,
            l.Room,
            await AssetHoldings.SectionsAsync(
                context,
                a => a.LocationId == id,
                Expiry.Today(clock),
                cancellationToken
            ),
            await AssetHoldings.SectionAsync(
                context
                    .Informations.AsNoTracking()
                    .Where(i => i.InformationLocations.Any(x => x.PhysicalLocationId == id)),
                q =>
                    q.OrderBy(i => i.Name)
                        .ThenBy(i => i.Id)
                        .Select(i => new InformationRow(i.Id, i.Name, i.InformationType.Name)),
                cancellationToken
            )
        );
    }
}
