using Codaxy.Inventory.App.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Company.Locations.Options;

/// <summary>The countries, and their cities and states, a location picks from — seeded codebooks.</summary>
public static class Endpoint
{
    public static void Map(RouteGroupBuilder locations) => locations.MapGet("/options", Handle);

    public sealed record Country(string Id, string Text);

    /// <param name="CountryCode">The country it is in: what the form narrows the list by.</param>
    public sealed record InCountry(Guid Id, string Text, string CountryCode);

    public sealed record Response(
        IReadOnlyList<Country> Countries,
        IReadOnlyList<InCountry> Cities,
        IReadOnlyList<InCountry> States
    );

    private static async Task<IResult> Handle(
        InventoryContext context,
        CancellationToken cancellationToken
    ) =>
        Results.Ok(
            new Response(
                await context
                    .Countries.AsNoTracking()
                    .OrderBy(c => c.Name)
                    .Select(c => new Country(c.Code, c.Name))
                    .ToListAsync(cancellationToken),
                await context
                    .Cities.AsNoTracking()
                    .OrderBy(c => c.Name)
                    .Select(c => new InCountry(c.Id, c.Name, c.CountryCode))
                    .ToListAsync(cancellationToken),
                await context
                    .States.AsNoTracking()
                    .OrderBy(s => s.Name)
                    .Select(s => new InCountry(s.Id, s.Name, s.CountryCode))
                    .ToListAsync(cancellationToken)
            )
        );
}
