namespace Codaxy.Inventory.App.Shared.Links;

/// <summary>Bound from the configuration's top level: what belongs to the deployment as a whole.</summary>
public sealed class ApplicationOptions
{
    /// <summary>
    /// The address people open the application at, for a link that leaves it — an email's. Empty,
    /// there are none: the server cannot know the address it is reached by.
    /// </summary>
    public string ApplicationUrl { get; set; } = "";

    /// <summary>The application's front page, or null without its address.</summary>
    public string? Home => ApplicationUrl == "" ? null : $"{ApplicationUrl.TrimEnd('/')}/";

    public string? Problem =>
        ApplicationUrl != ""
        && !(
            Uri.TryCreate(ApplicationUrl, UriKind.Absolute, out var url)
            && url.Scheme is "http" or "https"
        )
            ? $"ApplicationUrl is not an http or https address: {ApplicationUrl}."
            : null;
}
