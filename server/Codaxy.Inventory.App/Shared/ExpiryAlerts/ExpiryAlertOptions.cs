namespace Codaxy.Inventory.App.Shared.ExpiryAlerts;

public sealed class ExpiryAlertOptions
{
    public const string Section = "ExpiryAlerts";

    /// <summary>How often what is due is checked; a date entered inside the lead alerts at the next.</summary>
    public TimeSpan Interval { get; set; } = TimeSpan.FromHours(1);

    /// <summary>
    /// The address people open the application at, for a link to each record in the email; empty,
    /// there are none — the server cannot know the address it is reached by.
    /// </summary>
    public string ApplicationUrl { get; set; } = "";

    public string? Problem =>
        Interval <= TimeSpan.Zero ? "ExpiryAlerts:Interval must be longer than zero."
        : ApplicationUrl != ""
        && !(
            Uri.TryCreate(ApplicationUrl, UriKind.Absolute, out var url)
            && url.Scheme is "http" or "https"
        )
            ? $"ExpiryAlerts:ApplicationUrl is not an http or https address: {ApplicationUrl}."
        : null;
}
