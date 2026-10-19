namespace Codaxy.Inventory.App.Shared.ExpiryAlerts;

public sealed class ExpiryAlertOptions
{
    public const string Section = "ExpiryAlerts";

    /// <summary>How often what is due is checked; a date entered inside the lead alerts at the next.</summary>
    public TimeSpan Interval { get; set; } = TimeSpan.FromHours(1);

    public string? Problem =>
        Interval <= TimeSpan.Zero ? "ExpiryAlerts:Interval must be longer than zero." : null;
}
