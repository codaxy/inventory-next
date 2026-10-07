using System.Text.RegularExpressions;
using Codaxy.Inventory.App.Shared.Printing;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Options;
using PuppeteerSharp;

namespace Codaxy.Inventory.Web.Printing;

/// <summary>
/// Prints the client's pages in headless Chromium: one browser for the process, started on the first
/// print and closed once idle, and a fresh incognito context per print.
/// <para>
/// The context signs in with the caller's own session cookie, copied from the request, so the page
/// is the caller's — their name, and their rights once roles exist — and the cookie goes with the
/// context. Never a path that trusts a local caller instead: forwarded headers are trusted from any
/// sender, so anyone reaching the port can claim to be local.
/// </para>
/// </summary>
public sealed partial class ChromiumPagePrinter(
    IOptions<PrintingOptions> options,
    IServer server,
    IOptionsMonitor<CookieAuthenticationOptions> cookies,
    ILogger<ChromiumPagePrinter> log
) : IPagePrinter, IAsyncDisposable
{
    /// <summary>Prints at once; more wait. Each holds a page of the application in memory.</summary>
    private const int Slots = 2;

    private readonly PrintingOptions options = options.Value;
    private readonly SemaphoreSlim slots = new(Slots, Slots);
    private readonly SemaphoreSlim launching = new(1, 1);
    private IBrowser? browser;
    private Timer? idle;
    private int printing;

    public async Task<byte[]> PrintAsync(
        HttpContext caller,
        string path,
        TimeZoneInfo timeZone,
        CancellationToken cancellationToken
    )
    {
        await slots.WaitAsync(cancellationToken);
        Interlocked.Increment(ref printing);
        try
        {
            var chromium = await BrowserAsync(cancellationToken);
            var context = await chromium.CreateBrowserContextAsync();
            try
            {
                return await PrintAsync(context, caller, path, timeZone);
            }
            finally
            {
                await context.CloseAsync();
            }
        }
        catch (PuppeteerException e)
        {
            log.LogWarning(e, "Printing {Path} failed.", path);
            throw new PagePrintException("The page could not be printed.", e);
        }
        finally
        {
            if (Interlocked.Decrement(ref printing) == 0)
                idle?.Change(options.IdleClose, Timeout.InfiniteTimeSpan);
            slots.Release();
        }
    }

    private async Task<byte[]> PrintAsync(
        IBrowserContext context,
        HttpContext caller,
        string path,
        TimeZoneInfo timeZone
    )
    {
        var origin = Origin();
        var timeout = (int)options.Timeout.TotalMilliseconds;
        var page = await context.NewPageAsync();

        await page.SetCookieAsync(SessionCookies(caller, origin));
        await page.EmulateTimezoneAsync(IanaId(timeZone));

        // Only this machine: the application, and in development its module server. Nothing the page
        // links to or embeds elsewhere is fetched.
        await page.SetRequestInterceptionAsync(true);
        page.Request += async (_, e) =>
        {
            if (IsLocal(e.Request.Url))
                await e.Request.ContinueAsync();
            else
                await e.Request.AbortAsync();
        };

        await page.GoToAsync(
            new Uri(origin, path).ToString(),
            new NavigationOptions { Timeout = timeout, WaitUntil = [WaitUntilNavigation.Load] }
        );
        var marked = await page.WaitForSelectorAsync(
            "[data-print=ready], [data-print=failed]",
            new WaitForSelectorOptions { Timeout = timeout }
        );
        if (await marked.EvaluateFunctionAsync<string>("e => e.dataset.print") != "ready")
            throw new PagePrintException("The page could not load what it prints.");

        return await page.PdfDataAsync(
            new PdfOptions { PreferCSSPageSize = true, PrintBackground = true }
        );
    }

    /// <summary>
    /// The session cookie, and its chunks where the handler split it, set for the address Chromium
    /// opens. Never logged.
    /// </summary>
    private CookieParam[] SessionCookies(HttpContext caller, Uri origin)
    {
        var name = cookies.Get(CookieAuthenticationDefaults.AuthenticationScheme).Cookie.Name!;
        return
        [
            .. caller
                .Request.Cookies.Where(c =>
                    c.Key == name || c.Key.StartsWith(name + "C", StringComparison.Ordinal)
                )
                .Select(c => new CookieParam
                {
                    Name = c.Key,
                    Value = c.Value,
                    Url = origin.ToString(),
                    Path = "/",
                    HttpOnly = true,
                }),
        ];
    }

    /// <summary>
    /// Where this process listens, as Chromium on the same machine reaches it: plain HTTP where it
    /// listens on both, and a wildcard host — <c>http://+:8080</c>, <c>http://[::]:8080</c> — as the
    /// loopback address.
    /// </summary>
    private Uri Origin()
    {
        var addresses =
            server.Features.Get<IServerAddressesFeature>()?.Addresses
            ?? throw new PagePrintException("The server says nothing of where it listens.");
        var address =
            addresses
                .Select(a => ListenAddress().Match(a))
                .Where(m => m.Success)
                .OrderBy(m => m.Groups["scheme"].Value == "http" ? 0 : 1)
                .FirstOrDefault()
            ?? throw new PagePrintException("The server listens on no address Chromium can open.");

        var host = address.Groups["host"].Value;
        if (host is "+" or "*" or "0.0.0.0" or "[::]" or "localhost")
            host = "127.0.0.1";

        return new Uri($"{address.Groups["scheme"].Value}://{host}{address.Groups["port"].Value}/");
    }

    private static bool IsLocal(string url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var uri)
        && (uri.Scheme is "data" or "blob" || uri.IsLoopback);

    private static string IanaId(TimeZoneInfo zone) =>
        zone.HasIanaId ? zone.Id
        : TimeZoneInfo.TryConvertWindowsIdToIanaId(zone.Id, out var iana) ? iana
        : "UTC";

    private async Task<IBrowser> BrowserAsync(CancellationToken cancellationToken)
    {
        await launching.WaitAsync(cancellationToken);
        try
        {
            idle?.Change(Timeout.Infinite, Timeout.Infinite);
            if (browser is { IsConnected: true })
                return browser;

            browser = await Puppeteer.LaunchAsync(
                new LaunchOptions
                {
                    ExecutablePath = options.ChromiumPath,
                    Headless = true,
                    Args =
                    [
                        // Docker's /dev/shm is 64 MB, which a page can outgrow.
                        "--disable-dev-shm-usage",
                        // The development server's certificate; every request is to this machine.
                        "--ignore-certificate-errors",
                        .. options.Sandbox ? Array.Empty<string>() : ["--no-sandbox"],
                    ],
                }
            );
            idle ??= new Timer(_ => _ = CloseIdleAsync());
            return browser;
        }
        finally
        {
            launching.Release();
        }
    }

    private async Task CloseIdleAsync()
    {
        await launching.WaitAsync();
        try
        {
            if (Volatile.Read(ref printing) > 0 || browser is null)
                return;
            await browser.CloseAsync();
            browser = null;
        }
        catch (Exception e)
        {
            log.LogWarning(e, "Closing the idle browser failed.");
        }
        finally
        {
            launching.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (idle is not null)
            await idle.DisposeAsync();
        if (browser is not null)
            await browser.DisposeAsync();
    }

    [GeneratedRegex(@"^(?<scheme>https?)://(?<host>\[[^\]]*\]|[^:/]+)(?<port>:\d+)?")]
    private static partial Regex ListenAddress();
}
