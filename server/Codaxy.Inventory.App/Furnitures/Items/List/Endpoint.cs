using System.Linq.Expressions;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Paging;
using Codaxy.Inventory.App.Shared.Search;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Furnitures.Items.List;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder furniture) => furniture.MapGet("/", Handle);

    /// <param name="Q">Free text over number, name, model, assignee, location and vendor.</param>
    /// <param name="PurchasedFrom">Inclusive.</param>
    /// <param name="PurchasedTo">Exclusive, so consecutive ranges neither overlap nor leave a gap.</param>
    /// <param name="Sort"><c>-modified</c> (default), <c>number</c>, <c>name</c>, <c>assignee</c>, <c>location</c>, <c>type</c>, <c>vendor</c>, <c>value</c>, <c>modified</c>; <c>-</c> for descending.</param>
    public sealed record Query(
        string? Q,
        Guid? TypeId,
        Guid? VendorId,
        Guid? PersonId,
        Guid? LocationId,
        DateOnly? PurchasedFrom,
        DateOnly? PurchasedTo,
        bool? Incomplete,
        string? Sort,
        int? Page,
        int? PageSize
    );

    public sealed record Item(
        Guid Id,
        int? Number,
        string Name,
        bool Incomplete,
        string Assignee,
        string? Location,
        string? Type,
        string Vendor,
        decimal PurchaseValue,
        DateTimeOffset LastModified
    );

    private static readonly string[] Keys =
    [
        "number",
        "name",
        "assignee",
        "location",
        "type",
        "vendor",
        "value",
        "modified",
    ];

    private static async Task<IResult> Handle(
        [AsParameters] Query query,
        InventoryContext context,
        CancellationToken cancellationToken
    )
    {
        if (Paging.Read(query.Page, query.PageSize, out var window) is { } problem)
            return problem;

        if (Refuse(query) is { } refused)
            return refused;

        return Results.Ok(
            await Rows(context, query)
                .Select(f => new Item(
                    f.AssetId,
                    f.Asset.InventoryNumber,
                    f.Asset.Name,
                    f.Asset.Incomplete,
                    f.Asset.Person.Name,
                    f.Asset.LocationId == null ? null : f.Asset.Location.Name,
                    f.FurnitureTypeId == null ? null : f.FurnitureType.Name,
                    f.Asset.Vendor.Name,
                    f.Asset.PurchaseValue,
                    f.Asset.LastModified
                ))
                .ToPageAsync(window, cancellationToken)
        );
    }

    /// <summary>A sort outside the convention; the problem to answer with, or none.</summary>
    internal static IResult? Refuse(Query query) =>
        query.Sort is not null && !Keys.Contains(query.Sort.TrimStart('-'))
            ? Results.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    ["sort"] =
                    [
                        "Sort by number, name, assignee, location, type, vendor, value or modified.",
                    ],
                }
            )
            : null;

    /// <summary>
    /// The furniture the query selects, in its order — every piece, for the page to take its window of
    /// and the export to write whole, so the two can never disagree about what the list shows.
    /// </summary>
    internal static IOrderedQueryable<Furniture> Rows(InventoryContext context, Query query)
    {
        var rows = context.Furnitures.AsNoTracking();

        foreach (var term in FreeText.Terms(query.Q))
        {
            var pattern = FreeText.Pattern(term);
            rows = rows.Where(f =>
                EF.Functions.ILike(f.Asset.InventoryNumber.ToString()!, pattern, FreeText.Escape)
                || EF.Functions.ILike(f.Asset.Name, pattern, FreeText.Escape)
                || EF.Functions.ILike(f.Model, pattern, FreeText.Escape)
                || EF.Functions.ILike(f.Asset.Person.Name, pattern, FreeText.Escape)
                || EF.Functions.ILike(f.Asset.Location.Name, pattern, FreeText.Escape)
                || EF.Functions.ILike(f.Asset.Vendor.Name, pattern, FreeText.Escape)
            );
        }

        if (query.TypeId is { } type)
            rows = rows.Where(f => f.FurnitureTypeId == type);
        if (query.VendorId is { } vendor)
            rows = rows.Where(f => f.Asset.VendorId == vendor);
        if (query.PersonId is { } person)
            rows = rows.Where(f => f.Asset.PersonId == person);
        if (query.LocationId is { } location)
            rows = rows.Where(f => f.Asset.LocationId == location);
        if (query.PurchasedFrom is { } from)
            rows = rows.Where(f => f.Asset.PurchaseDate >= from);
        if (query.PurchasedTo is { } to)
            rows = rows.Where(f => f.Asset.PurchaseDate < to);
        if (query.Incomplete is { } incomplete)
            rows = rows.Where(f => f.Asset.Incomplete == incomplete);

        var descending = query.Sort?.StartsWith('-') ?? true;
        return (query.Sort?.TrimStart('-') ?? "modified") switch
        {
            "number" => Order(rows, f => f.Asset.InventoryNumber, descending),
            "name" => Order(rows, f => f.Asset.Name, descending),
            "assignee" => Order(rows, f => f.Asset.Person.Name, descending),
            "location" => Order(rows, f => f.Asset.Location.Name, descending),
            "type" => Order(rows, f => f.FurnitureType.Name, descending),
            "vendor" => Order(rows, f => f.Asset.Vendor.Name, descending),
            "value" => Order(rows, f => f.Asset.PurchaseValue, descending),
            _ => Order(rows, f => f.Asset.LastModified, descending),
        };
    }

    /// <summary>The reader's column, then the id, so pieces that sort equal keep one order across pages.</summary>
    private static IOrderedQueryable<Furniture> Order<T>(
        IQueryable<Furniture> rows,
        Expression<Func<Furniture, T>> key,
        bool descending
    ) => (descending ? rows.OrderByDescending(key) : rows.OrderBy(key)).ThenBy(f => f.AssetId);
}
