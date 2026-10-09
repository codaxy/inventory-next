namespace Codaxy.Inventory.App.Dashboard;

public sealed class DashboardOptions
{
    public const string Section = "Dashboard";

    /// <summary>
    /// The locations an asset is moved to once it is written off or sold, by name. The data marks
    /// disposal only this way — an asset's status is never set — and names differ between databases,
    /// so each deployment says which. Empty, the dashboard has no section for seats left on them.
    /// </summary>
    public string[] DisposedLocations { get; set; } = [];

    /// <summary>How far back a lapsed subscription is news; one older was dealt with, or dropped.</summary>
    public int SubscriptionsExpiredDays { get; set; } = 45;

    /// <summary>
    /// How far ahead a subscription's end is shown. The dashboard's own: the licenses list's "expiring
    /// soon" filter and badge keep <c>Expiry.SoonDays</c>.
    /// </summary>
    public int SubscriptionsExpiringDays { get; set; } = 15;

    /// <summary>How far back an ended warranty is news.</summary>
    public int WarrantiesEndedDays { get; set; } = 45;

    /// <summary>How far ahead a warranty's end is shown: time to have a fault repaired under it.</summary>
    public int WarrantiesEndingDays { get; set; } = 30;

    /// <summary>
    /// Why these options stop the start, or null: every window at least a day, since zero or less
    /// would show nothing, or everything ever.
    /// </summary>
    public string? Problem =>
        new (string Name, int Days)[]
        {
            (nameof(SubscriptionsExpiredDays), SubscriptionsExpiredDays),
            (nameof(SubscriptionsExpiringDays), SubscriptionsExpiringDays),
            (nameof(WarrantiesEndedDays), WarrantiesEndedDays),
            (nameof(WarrantiesEndingDays), WarrantiesEndingDays),
        }.FirstOrDefault(w => w.Days < 1)
            is { Name: { } name }
            ? $"Dashboard:{name} must be at least 1."
            : null;

    public bool IsValid => Problem is null;
}
