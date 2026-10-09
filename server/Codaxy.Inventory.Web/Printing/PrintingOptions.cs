namespace Codaxy.Inventory.Web.Printing;

public sealed class PrintingOptions
{
    public const string Section = "Pdf";

    /// <summary>
    /// The Chromium or Chrome binary that prints pages. Empty, nothing is printed and the endpoints
    /// that would answer with a PDF do not exist. Never downloaded at runtime.
    /// </summary>
    public string ChromiumPath { get; set; } = "";

    /// <summary>
    /// Chromium's own sandbox. Off only where it cannot start — an unprivileged container — which is
    /// tolerable because the browser loads nothing but this application's pages.
    /// </summary>
    public bool Sandbox { get; set; } = true;

    /// <summary>How long a page has to say it is ready.</summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(20);

    /// <summary>How long the browser stays open after its last print.</summary>
    public TimeSpan IdleClose { get; set; } = TimeSpan.FromMinutes(5);

    public bool Enabled => !string.IsNullOrWhiteSpace(ChromiumPath);

    /// <summary>Why these options stop the start, or null: a path that is set and names no file.</summary>
    public string? Problem =>
        Enabled && !File.Exists(ChromiumPath)
            ? $"Pdf:ChromiumPath names no file: {ChromiumPath}."
            : null;

    public bool IsValid => Problem is null;
}
