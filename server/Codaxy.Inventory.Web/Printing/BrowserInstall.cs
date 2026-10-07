using PuppeteerSharp;

namespace Codaxy.Inventory.Web.Printing;

/// <summary>
/// <c>--install-browser &lt;dir&gt;</c>: downloads the Chrome headless shell this PuppeteerSharp
/// release is tested against under <c>dir</c>, links it as <c>dir/chrome</c>, puts its list of system
/// libraries at <c>dir/deb.deps</c> for the image to install, and exits. The image runs it once when
/// it is built — never the application at runtime — so the browser always matches the library
/// driving it. Not a distribution's package: Ubuntu's <c>chromium</c>, the base image's, is a stub
/// for a snap, which a container cannot run. The headless shell, not Chrome: it is built for this,
/// and needs no GTK — half the size of Chrome and its libraries.
/// </summary>
public static class BrowserInstall
{
    public const string Command = "--install-browser";

    public static async Task RunAsync(string directory)
    {
        var fetcher = new BrowserFetcher(
            new BrowserFetcherOptions
            {
                Browser = SupportedBrowser.ChromeHeadlessShell,
                Path = Path.Combine(directory, "cache"),
            }
        );
        var installed = await fetcher.DownloadAsync();
        var executable = installed.GetExecutablePath();

        var link = Path.Combine(directory, "chrome");
        File.Delete(link);
        File.CreateSymbolicLink(link, executable);
        File.Copy(
            Path.Combine(Path.GetDirectoryName(executable)!, "deb.deps"),
            Path.Combine(directory, "deb.deps"),
            overwrite: true
        );
        Console.WriteLine($"Chrome headless shell {installed.BuildId} at {link}");
    }
}
