using Codaxy.Inventory.App.Shared.Settings;

namespace Codaxy.Inventory.App.Dashboard;

/// <summary>The dashboard's windows, in days, each on its own.</summary>
public static class DashboardSettings
{
    /// <summary>How far back a lapsed subscription is news; one older was dealt with, or dropped.</summary>
    public static readonly SettingDefinition<int> SubscriptionsExpiredDays = Window(
        "dashboard.subscriptions_expired_days",
        15
    );

    /// <summary>
    /// How far ahead a subscription's end is shown. The dashboard's own: the licenses list's "expiring
    /// soon" filter and badge keep <c>Expiry.SoonDays</c>.
    /// </summary>
    public static readonly SettingDefinition<int> SubscriptionsExpiringDays = Window(
        "dashboard.subscriptions_expiring_days",
        45
    );

    /// <summary>How far back an ended warranty is news.</summary>
    public static readonly SettingDefinition<int> WarrantiesEndedDays = Window(
        "dashboard.warranties_ended_days",
        15
    );

    /// <summary>How far ahead a warranty's end is shown: time to have a fault repaired under it.</summary>
    public static readonly SettingDefinition<int> WarrantiesEndingDays = Window(
        "dashboard.warranties_ending_days",
        30
    );

    /// <summary>At least a day: zero or less would show nothing, or everything ever.</summary>
    private static SettingDefinition<int> Window(string key, int days) =>
        new(key, days, value => value < 1 ? "At least 1 day." : null);
}
