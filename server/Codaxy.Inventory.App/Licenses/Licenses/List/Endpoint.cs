using System.Linq.Expressions;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Paging;
using Codaxy.Inventory.App.Shared.Search;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Licenses.Licenses.List;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder licenses) => licenses.MapGet("/", Handle);

    /// <param name="Q">Free text over number, name, vendor, invoice and registration number.</param>
    /// <param name="PurchasedFrom">Inclusive.</param>
    /// <param name="PurchasedTo">Exclusive, so consecutive ranges neither overlap nor leave a gap.</param>
    /// <param name="Expiry"><c>expired</c>, <c>soon</c>, <c>regular</c> or <c>none</c>.</param>
    /// <param name="Sort"><c>-modified</c> (default), <c>number</c>, <c>name</c>, <c>vendor</c>, <c>value</c>, <c>purchased</c>, <c>expires</c>, <c>modified</c>; <c>-</c> for descending.</param>
    public sealed record Query(
        string? Q,
        Guid? VendorId,
        DateOnly? PurchasedFrom,
        DateOnly? PurchasedTo,
        string? Expiry,
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
        string Vendor,
        decimal PurchaseValue,
        DateOnly PurchaseDate,
        DateOnly? ExpirationDate,
        string? Expiry,
        DateTimeOffset LastModified
    );

    private static readonly string[] Keys =
    [
        "number",
        "name",
        "vendor",
        "value",
        "purchased",
        "expires",
        "modified",
    ];

    private static async Task<IResult> Handle(
        [AsParameters] Query query,
        InventoryContext context,
        TimeProvider clock,
        CancellationToken cancellationToken
    )
    {
        if (Paging.Read(query.Page, query.PageSize, out var window) is { } problem)
            return problem;

        if (Refuse(query) is { } refused)
            return refused;

        var today = Licenses.Expiry.Today(clock);

        var page = await Rows(context, query, today)
            .Select(l => new Item(
                l.AssetId,
                l.Asset.InventoryNumber,
                l.Asset.Name,
                l.Asset.Incomplete,
                l.Asset.Vendor.Name,
                l.Asset.PurchaseValue,
                l.Asset.PurchaseDate,
                l.SubscriptionExpirationDate,
                null,
                l.Asset.LastModified
            ))
            .ToPageAsync(window, cancellationToken);

        return Results.Ok(
            page with
            {
                Items =
                [
                    .. page.Items.Select(i =>
                        i with
                        {
                            Expiry = Licenses.Expiry.Status(i.ExpirationDate, today),
                        }
                    ),
                ],
            }
        );
    }

    /// <summary>A sort or expiry outside the convention; the problem to answer with, or none.</summary>
    internal static IResult? Refuse(Query query)
    {
        var errors = new Dictionary<string, string[]>();
        if (query.Sort is not null && !Keys.Contains(query.Sort.TrimStart('-')))
            errors["sort"] =
            [
                "Sort by number, name, vendor, value, purchased, expires or modified.",
            ];
        if (query.Expiry is not null && !Licenses.Expiry.Statuses.Contains(query.Expiry))
            errors["expiry"] = ["Expiry is expired, soon, regular or none."];
        return errors.Count > 0 ? Results.ValidationProblem(errors) : null;
    }

    /// <summary>
    /// The licences the query selects, in its order — every one, for the page to take its window of
    /// and the export to write whole, so the two can never disagree about what the list shows.
    /// </summary>
    internal static IOrderedQueryable<License> Rows(
        InventoryContext context,
        Query query,
        DateOnly today
    )
    {
        var licenses = context.Licenses.AsNoTracking();

        foreach (var term in FreeText.Terms(query.Q))
        {
            var pattern = FreeText.Pattern(term);
            licenses = licenses.Where(l =>
                EF.Functions.ILike(l.Asset.InventoryNumber.ToString()!, pattern, FreeText.Escape)
                || EF.Functions.ILike(l.Asset.Name, pattern, FreeText.Escape)
                || EF.Functions.ILike(l.Asset.Vendor.Name, pattern, FreeText.Escape)
                || EF.Functions.ILike(l.Asset.InvoiceNumber, pattern, FreeText.Escape)
                || EF.Functions.ILike(l.RegistrationNumber, pattern, FreeText.Escape)
            );
        }

        if (query.VendorId is { } vendor)
            licenses = licenses.Where(l => l.Asset.VendorId == vendor);

        if (query.PurchasedFrom is { } from)
            licenses = licenses.Where(l => l.Asset.PurchaseDate >= from);

        if (query.PurchasedTo is { } to)
            licenses = licenses.Where(l => l.Asset.PurchaseDate < to);

        if (query.Incomplete is { } incomplete)
            licenses = licenses.Where(l => l.Asset.Incomplete == incomplete);

        var soon = today.AddDays(Licenses.Expiry.SoonDays);
        licenses = query.Expiry switch
        {
            "expired" => licenses.Where(l => l.SubscriptionExpirationDate < today),
            "soon" => licenses.Where(l =>
                l.SubscriptionExpirationDate >= today && l.SubscriptionExpirationDate < soon
            ),
            "regular" => licenses.Where(l => l.SubscriptionExpirationDate >= soon),
            "none" => licenses.Where(l => l.SubscriptionExpirationDate == null),
            _ => licenses,
        };

        var descending = query.Sort?.StartsWith('-') ?? true;
        return (query.Sort?.TrimStart('-') ?? "modified") switch
        {
            "number" => Order(licenses, l => l.Asset.InventoryNumber, descending),
            "name" => Order(licenses, l => l.Asset.Name, descending),
            "vendor" => Order(licenses, l => l.Asset.Vendor.Name, descending),
            "value" => Order(licenses, l => l.Asset.PurchaseValue, descending),
            "purchased" => Order(licenses, l => l.Asset.PurchaseDate, descending),
            "expires" => Order(licenses, l => l.SubscriptionExpirationDate, descending),
            _ => Order(licenses, l => l.Asset.LastModified, descending),
        };
    }

    /// <summary>The reader's column, then the id, so licences that sort equal keep one order across pages.</summary>
    private static IOrderedQueryable<License> Order<T>(
        IQueryable<License> licenses,
        Expression<Func<License, T>> key,
        bool descending
    ) =>
        (descending ? licenses.OrderByDescending(key) : licenses.OrderBy(key)).ThenBy(l =>
            l.AssetId
        );
}
