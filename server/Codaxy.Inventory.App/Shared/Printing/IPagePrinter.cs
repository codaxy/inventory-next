namespace Codaxy.Inventory.App.Shared.Printing;

/// <summary>
/// Prints one of the client's own pages to a PDF, as the caller sees it: the page is the PDF's only
/// layout, its <c>@media print</c> and <c>@page</c> rules the file's. Registered only where the host
/// is configured for it, so an endpoint takes it as optional.
/// </summary>
public interface IPagePrinter
{
    /// <summary>
    /// The page at <paramref name="path"/> — an application path, <c>/company/people/…</c> — signed in
    /// as <paramref name="caller"/>, its clock in <paramref name="timeZone"/>. The page says when it
    /// is ready by setting <c>data-print</c> to <c>ready</c>, or to <c>failed</c>.
    /// </summary>
    /// <exception cref="PagePrintException">The page failed, or never became ready.</exception>
    Task<byte[]> PrintAsync(
        HttpContext caller,
        string path,
        TimeZoneInfo timeZone,
        CancellationToken cancellationToken
    );
}

public sealed class PagePrintException(string message, Exception? inner = null)
    : Exception(message, inner);
