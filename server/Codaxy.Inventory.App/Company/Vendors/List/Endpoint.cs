using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Paging;
using Codaxy.Inventory.App.Shared.Search;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Company.Vendors.List;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder vendors) => vendors.MapGet("/", Handle);

    /// <param name="Q">Free text over name, contact person, email, VAT and registration number.</param>
    /// <param name="Sort"><c>name</c> (default), <c>assets</c>; <c>-</c> for descending.</param>
    public sealed record Query(string? Q, string? Sort, int? Page, int? PageSize);

    /// <param name="Assets">Every asset bought from the vendor.</param>
    public sealed record Item(
        Guid Id,
        string Name,
        string? ContactPerson,
        string? Email,
        string? Phone,
        int Assets
    );

    private static async Task<IResult> Handle(
        [AsParameters] Query query,
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        if (Paging.Read(query.Page, query.PageSize, out var window) is { } problem)
            return problem;

        if (query.Sort is not (null or "name" or "-name" or "assets" or "-assets"))
            return Results.ValidationProblem(
                new Dictionary<string, string[]> { ["sort"] = ["Sort by name or assets."] }
            );

        var vendors = context.Vendors.AsNoTracking();
        foreach (var term in FreeText.Terms(query.Q))
        {
            var pattern = FreeText.Pattern(term);
            vendors = vendors.Where(v =>
                EF.Functions.ILike(v.Name, pattern, FreeText.Escape)
                || EF.Functions.ILike(v.ContactPerson, pattern, FreeText.Escape)
                || EF.Functions.ILike(v.Email, pattern, FreeText.Escape)
                || EF.Functions.ILike(v.VATNumber, pattern, FreeText.Escape)
                || EF.Functions.ILike(v.RegistrationNumber, pattern, FreeText.Escape)
            );
        }

        var assets = context.Assets;
        var ordered = query.Sort switch
        {
            "-name" => vendors.OrderByDescending(v => v.Name),
            "assets" => vendors
                .OrderBy(v => assets.Count(a => a.VendorId == v.Id))
                .ThenBy(v => v.Name),
            "-assets" => vendors
                .OrderByDescending(v => assets.Count(a => a.VendorId == v.Id))
                .ThenBy(v => v.Name),
            _ => vendors.OrderBy(v => v.Name),
        };

        return Results.Ok(
            await ordered
                .ThenBy(v => v.Id)
                .Select(v => new Item(
                    v.Id,
                    v.Name,
                    v.ContactPerson,
                    v.Email,
                    v.Phone ?? v.MobilePhone,
                    assets.Count(a => a.VendorId == v.Id)
                ))
                .ToPageAsync(window, cancellationToken)
        );
    }
}
