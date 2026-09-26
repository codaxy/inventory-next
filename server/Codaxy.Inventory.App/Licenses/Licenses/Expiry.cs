namespace Codaxy.Inventory.App.Licenses.Licenses;

/// <summary>
/// Where a subscription stands on a day: <c>expired</c> before it, <c>soon</c> within the next
/// fifteen days, <c>regular</c> after; nothing without a date. A licence expiring today is still valid
/// today.
/// </summary>
public static class Expiry
{
    public const int SoonDays = 15;

    public static readonly string[] Statuses = ["expired", "soon", "regular", "none"];

    public static string? Status(DateOnly? date, DateOnly today) =>
        date switch
        {
            null => null,
            { } d when d < today => "expired",
            { } d when d < today.AddDays(SoonDays) => "soon",
            _ => "regular",
        };

    public static DateOnly Today(TimeProvider clock) =>
        DateOnly.FromDateTime(clock.GetUtcNow().UtcDateTime);
}
