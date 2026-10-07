using System.Net;
using System.Net.Sockets;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.Tests.Integration.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UglyToad.PdfPig;

namespace Codaxy.Inventory.Tests.Integration;

/// <summary>
/// A test that prints through a real browser, against the built client. Skipped unless
/// <c>INVENTORY_TEST_CHROMIUM</c> names a Chromium binary and the client has been built into
/// <c>wwwroot</c> — CI does both. Not <c>Pdf__ChromiumPath</c> itself: every fixture reads the
/// environment, and the rest are written against a server with no browser.
/// </summary>
public sealed class ChromiumFactAttribute : FactAttribute
{
    public const string Variable = "INVENTORY_TEST_CHROMIUM";

    public ChromiumFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(Variable)))
            Skip = $"{Variable} names no Chromium binary.";
        else if (!Directory.Exists(ChromiumApplication.BuiltClient))
            Skip = "The client is not built into wwwroot.";
    }
}

/// <summary>
/// The application on a real port — Chromium opens it as a browser does — with a printer. The seeded
/// person holds a device and signs in as themselves.
/// </summary>
public class ChromiumApplication : InventoryApplication
{
    public const string Email = "seed@codaxy.com";

    /// <summary>The build's bundles: <c>wwwroot</c> holds a stub shell without them.</summary>
    public static readonly string BuiltClient = Path.GetFullPath(
        Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            "Codaxy.Inventory.Web",
            "wwwroot",
            "assets"
        )
    );

    public Guid Person { get; private set; }

    /// <summary>
    /// A free port, taken up front: the factory's clients go to <c>ClientOptions.BaseAddress</c>,
    /// which knows nothing of a port Kestrel picks for itself.
    /// </summary>
    public ChromiumApplication()
    {
        var probe = new TcpListener(IPAddress.Loopback, 0);
        probe.Start();
        var port = ((IPEndPoint)probe.LocalEndpoint).Port;
        probe.Stop();

        UseKestrel(port);
        ClientOptions.BaseAddress = new Uri($"http://127.0.0.1:{port}");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.UseSetting(
            "Pdf:ChromiumPath",
            Environment.GetEnvironmentVariable(ChromiumFactAttribute.Variable) ?? ""
        );
        builder.UseSetting("Pdf:Sandbox", "false");
        builder.UseSetting("Handover:Place", "Banja Luka");
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<InventoryContext>();
        Person = (await Seed.BasicsAsync(context)).Person;
        await Seed.DeviceAsync(context, "Seed laptop", holdsLicenses: true);
        (await context.Persons.SingleAsync(p => p.Id == Person)).Email = Email;
        await context.SaveChangesAsync();
    }
}

public class ChromiumPrintingTests(ChromiumApplication app) : IClassFixture<ChromiumApplication>
{
    [ChromiumFact]
    public async Task Prints_the_handover_sheet_as_the_caller_sees_it()
    {
        var client = await app.ClientAsync(ChromiumApplication.Email);

        var response = await client.GetAsync(
            $"/api/company/people/{app.Person}/handover.pdf?tz=Europe/Belgrade"
        );

        response.EnsureSuccessStatusCode();
        using var pdf = PdfDocument.Open(await response.Content.ReadAsByteArrayAsync());
        var text = string.Join(" ", pdf.GetPages().Select(p => p.Text));
        var today = TimeZoneInfo.ConvertTime(
            DateTimeOffset.UtcNow,
            TimeZoneInfo.FindSystemTimeZoneById("Europe/Belgrade")
        );
        Assert.Contains("Seed laptop", text);
        Assert.Contains("Mjesto:Banja Luka", text.Replace(": ", ":"));
        Assert.Contains($"{today:dd.MM.yyyy}.", text);
        Assert.Contains("Kontrolor:Seed person", text.Replace(": ", ":"));
        // The sheet only: nothing of the shell around it.
        Assert.DoesNotContain("Handover sheet", text);
    }
}
