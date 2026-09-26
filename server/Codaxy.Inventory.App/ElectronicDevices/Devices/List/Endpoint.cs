using System.Linq.Expressions;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Paging;
using Codaxy.Inventory.App.Shared.Search;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.ElectronicDevices.Devices.List;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder devices) => devices.MapGet("/", Handle);

    /// <param name="Q">Free text over number, name, model name and code, serial number and description.</param>
    /// <param name="TagId">A tag of the device's type.</param>
    /// <param name="PersonId">The assignee.</param>
    /// <param name="PurchasedTo">Exclusive, so consecutive ranges neither overlap nor leave a gap.</param>
    /// <param name="Sort"><c>-modified</c> (default), <c>number</c>, <c>name</c>, <c>model</c>, <c>assignee</c>, <c>location</c>, <c>type</c>, <c>manufacturer</c>, <c>modified</c>; <c>-</c> for descending.</param>
    public sealed record Query(
        string? Q,
        Guid? TypeId,
        Guid? TagId,
        Guid? PersonId,
        Guid? VendorId,
        Guid? LocationId,
        Guid? ManufacturerId,
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
        string? ModelName,
        string Assignee,
        string? Location,
        string? Type,
        string? Manufacturer,
        string? ModelCode,
        string? SerialNumber,
        DateTimeOffset LastModified
    );

    private static readonly string[] Keys =
    [
        "number",
        "name",
        "model",
        "assignee",
        "location",
        "type",
        "manufacturer",
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
                .Select(d => new Item(
                    d.AssetId,
                    d.Asset.InventoryNumber,
                    d.Asset.Name,
                    d.Asset.Incomplete,
                    d.ModelName,
                    d.Asset.Person.Name,
                    d.Asset.LocationId == null ? null : d.Asset.Location.Name,
                    d.ElectronicDeviceTypeId == null ? null : d.ElectronicDeviceType.Name,
                    d.ManufacturerId == null ? null : d.Manufacturer.Name,
                    d.ModelCode,
                    d.SerialNumber,
                    d.Asset.LastModified
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
                        "Sort by number, name, model, assignee, location, type, manufacturer or modified.",
                    ],
                }
            )
            : null;

    /// <summary>
    /// The devices the query selects, in its order — every one, for the page to take its window of and
    /// the export to write whole, so the two can never disagree about what the list shows.
    /// </summary>
    internal static IOrderedQueryable<ElectronicDevice> Rows(InventoryContext context, Query query)
    {
        var rows = context.ElectronicDevices.AsNoTracking();

        foreach (var term in FreeText.Terms(query.Q))
        {
            if (FreeText.Id(term) is { } id)
            {
                rows = rows.Where(d =>
                    d.AssetId == id
                    || d.ElectronicDeviceTypeId == id
                    || d.ManufacturerId == id
                    || d.Asset.VendorId == id
                    || d.Asset.PersonId == id
                    || d.Asset.LocationId == id
                    || d.Asset.BusinessEntityId == id
                    || d.Asset.AssetTypeId == id
                    || d.Asset.AssetSubstatusId == id
                    || d.Asset.ConfidentialityId == id
                    || d.Asset.IntegrityId == id
                    || d.Asset.AvailabilityId == id
                    || d.Asset.ImportanceId == id
                );
                continue;
            }

            var pattern = FreeText.Pattern(term);
            rows = rows.Where(d =>
                EF.Functions.ILike(d.Asset.InventoryNumber.ToString()!, pattern, FreeText.Escape)
                || EF.Functions.ILike(d.Asset.Name, pattern, FreeText.Escape)
                || EF.Functions.ILike(d.ModelName, pattern, FreeText.Escape)
                || EF.Functions.ILike(d.ModelCode, pattern, FreeText.Escape)
                || EF.Functions.ILike(d.SerialNumber, pattern, FreeText.Escape)
                || EF.Functions.ILike(d.Asset.Description, pattern, FreeText.Escape)
            );
        }

        if (query.TypeId is { } type)
            rows = rows.Where(d => d.ElectronicDeviceTypeId == type);
        if (query.TagId is { } tag)
            rows = rows.Where(d =>
                d.ElectronicDeviceType.Tags.Any(t => t.ElectronicDeviceTagId == tag)
            );
        if (query.PersonId is { } person)
            rows = rows.Where(d => d.Asset.PersonId == person);
        if (query.VendorId is { } vendor)
            rows = rows.Where(d => d.Asset.VendorId == vendor);
        if (query.LocationId is { } location)
            rows = rows.Where(d => d.Asset.LocationId == location);
        if (query.ManufacturerId is { } manufacturer)
            rows = rows.Where(d => d.ManufacturerId == manufacturer);
        if (query.PurchasedFrom is { } from)
            rows = rows.Where(d => d.Asset.PurchaseDate >= from);
        if (query.PurchasedTo is { } to)
            rows = rows.Where(d => d.Asset.PurchaseDate < to);
        if (query.Incomplete is { } incomplete)
            rows = rows.Where(d => d.Asset.Incomplete == incomplete);

        var descending = query.Sort?.StartsWith('-') ?? true;
        return (query.Sort?.TrimStart('-') ?? "modified") switch
        {
            "number" => Order(rows, d => d.Asset.InventoryNumber, descending),
            "name" => Order(rows, d => d.Asset.Name, descending),
            "model" => Order(rows, d => d.ModelName, descending),
            "assignee" => Order(rows, d => d.Asset.Person.Name, descending),
            "location" => Order(rows, d => d.Asset.Location.Name, descending),
            "type" => Order(rows, d => d.ElectronicDeviceType.Name, descending),
            "manufacturer" => Order(rows, d => d.Manufacturer.Name, descending),
            _ => Order(rows, d => d.Asset.LastModified, descending),
        };
    }

    /// <summary>The reader's column, then the id, so devices that sort equal keep one order across pages.</summary>
    private static IOrderedQueryable<ElectronicDevice> Order<T>(
        IQueryable<ElectronicDevice> rows,
        Expression<Func<ElectronicDevice, T>> key,
        bool descending
    ) => (descending ? rows.OrderByDescending(key) : rows.OrderBy(key)).ThenBy(d => d.AssetId);
}
