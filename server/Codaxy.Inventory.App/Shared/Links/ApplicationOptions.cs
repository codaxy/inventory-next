namespace Codaxy.Inventory.App.Shared.Links;

/// <summary>Bound from the configuration's top level: what belongs to the deployment as a whole.</summary>
public sealed class ApplicationOptions
{
    /// <summary>
    /// The address people open the application at, for a link that leaves it — an email's. Empty,
    /// there are none: the server cannot know the address it is reached by.
    /// </summary>
    public string ApplicationUrl { get; set; } = "";

    public string? Problem =>
        ApplicationUrl != ""
        && !(
            Uri.TryCreate(ApplicationUrl, UriKind.Absolute, out var url)
            && url.Scheme is "http" or "https"
        )
            ? $"ApplicationUrl is not an http or https address: {ApplicationUrl}."
            : null;
}
