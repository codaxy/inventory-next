using Codaxy.Inventory.App.Licenses.Licenses;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Paging;
using Codaxy.Inventory.App.Shared.Search;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Licenses.Activations.List;

public static class Endpoint
{
    public static void Map(RouteGroupBuilder activations) => activations.MapGet("/", Handle);

    /// <param name="Q">Free text over software, licence, person, device name and number.</param>
    /// <param name="Status"><c>active</c> or <c>deactivated</c>.</param>
    /// <param name="Expiry">The licence's: <c>expired</c>, <c>soon</c>, <c>regular</c> or <c>none</c>.</param>
    /// <param name="Sort"><c>-activated</c> (default), <c>activated</c>, <c>software</c>, <c>license</c>, <c>assignee</c>, <c>deactivated</c>; <c>-</c> for descending.</param>
    public sealed record Query(
        string? Q,
        Guid? SoftwareId,
        Guid? LicenseId,
        string? Status,
        string? Expiry,
        string? Sort,
        int? Page,
        int? PageSize
    );

    /// <param name="Assignee">The person's name, or the device's.</param>
    public sealed record Item(
        Guid Id,
        string Software,
        Guid LicenseId,
        string License,
        string? Assignee,
        int? DeviceNumber,
        bool ForDevice,
        int Quantity,
        DateOnly ActivationDate,
        DateOnly? DeactivationDate,
        DateOnly? ExpirationDate,
        string? Expiry
    );

    private static readonly string[] Keys =
    [
        "activated",
        "software",
        "license",
        "assignee",
        "deactivated",
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

        var errors = new Dictionary<string, string[]>();
        if (query.Sort is not null && !Keys.Contains(query.Sort.TrimStart('-')))
            errors["sort"] = ["Sort by activated, software, license, assignee or deactivated."];
        if (query.Status is not (null or "active" or "deactivated"))
            errors["status"] = ["A status is active or deactivated."];
        if (query.Expiry is not null && !Licenses.Expiry.Statuses.Contains(query.Expiry))
            errors["expiry"] = ["Expiry is expired, soon, regular or none."];
        if (errors.Count > 0)
            return Results.ValidationProblem(errors);

        var today = Licenses.Expiry.Today(clock);
        var rows = context.Activations.AsNoTracking();

        foreach (var term in FreeText.Terms(query.Q))
        {
            var pattern = FreeText.Pattern(term);
            rows = rows.Where(a =>
                EF.Functions.ILike(a.Volume.SoftwareOrService.Name, pattern, FreeText.Escape)
                || EF.Functions.ILike(a.Volume.License.Asset.Name, pattern, FreeText.Escape)
                || EF.Functions.ILike(a.Person.Name, pattern, FreeText.Escape)
                || EF.Functions.ILike(a.Asset.Name, pattern, FreeText.Escape)
                || EF.Functions.ILike(a.Asset.InventoryNumber.ToString(), pattern, FreeText.Escape)
            );
        }

        if (query.SoftwareId is { } software)
            rows = rows.Where(a => a.Volume.SoftwareOrServiceId == software);

        if (query.LicenseId is { } license)
            rows = rows.Where(a => a.Volume.LicenseId == license);

        if (query.Status == "active")
            rows = rows.Where(a => a.DeactivationDate == null);
        else if (query.Status == "deactivated")
            rows = rows.Where(a => a.DeactivationDate != null);

        var soon = today.AddDays(Licenses.Expiry.SoonDays);
        rows = query.Expiry switch
        {
            "expired" => rows.Where(a => a.Volume.License.SubscriptionExpirationDate < today),
            "soon" => rows.Where(a =>
                a.Volume.License.SubscriptionExpirationDate >= today
                && a.Volume.License.SubscriptionExpirationDate < soon
            ),
            "regular" => rows.Where(a => a.Volume.License.SubscriptionExpirationDate >= soon),
            "none" => rows.Where(a => a.Volume.License.SubscriptionExpirationDate == null),
            _ => rows,
        };

        var descending = query.Sort?.StartsWith('-') ?? true;
        var ordered = (query.Sort?.TrimStart('-') ?? "activated") switch
        {
            "software" => Order(rows, a => a.Volume.SoftwareOrService.Name, descending),
            "license" => Order(rows, a => a.Volume.License.Asset.Name, descending),
            "assignee" => Order(
                rows,
                a => a.Person != null ? a.Person.Name : a.Asset.Name,
                descending
            ),
            "deactivated" => Order(rows, a => a.DeactivationDate, descending),
            _ => Order(rows, a => a.ActivationDate, descending),
        };

        var page = await ordered
            .Select(a => new
            {
                a.Id,
                Software = a.Volume.SoftwareOrService.Name,
                a.Volume.LicenseId,
                License = a.Volume.License.Asset.Name,
                Assignee = a.PersonId != null ? a.Person.Name : a.Asset.Name,
                DeviceNumber = a.PersonId != null ? null : a.Asset.InventoryNumber,
                ForDevice = a.PersonId == null,
                a.Quantity,
                a.ActivationDate,
                a.DeactivationDate,
                a.Volume.License.SubscriptionExpirationDate,
            })
            .ToPageAsync(window, cancellationToken);

        return Results.Ok(
            new Page<Item>(
                page.Items.Select(a => new Item(
                        a.Id,
                        a.Software,
                        a.LicenseId,
                        a.License,
                        a.Assignee,
                        a.DeviceNumber,
                        a.ForDevice,
                        a.Quantity,
                        a.ActivationDate,
                        a.DeactivationDate,
                        a.SubscriptionExpirationDate,
                        Licenses.Expiry.Status(a.SubscriptionExpirationDate, today)
                    ))
                    .ToList(),
                page.Total
            )
        );
    }

    /// <summary>The column the reader chose, then the newest activation, then the id, so rows that sort equal keep one order across pages.</summary>
    private static IOrderedQueryable<Activation> Order<T>(
        IQueryable<Activation> rows,
        System.Linq.Expressions.Expression<Func<Activation, T>> key,
        bool descending
    ) =>
        (descending ? rows.OrderByDescending(key) : rows.OrderBy(key))
            .ThenByDescending(a => a.ActivationDate)
            .ThenBy(a => a.Id);
}
