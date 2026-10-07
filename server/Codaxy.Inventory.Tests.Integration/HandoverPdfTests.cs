using System.Net;
using System.Net.Http.Json;
using Codaxy.Inventory.App.Company.People;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Printing;
using Codaxy.Inventory.Tests.Integration.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Codaxy.Inventory.Tests.Integration;

using Handover = App.Company.People.Handover.Endpoint.Response;

/// <summary>A printer that prints nothing: it records what it was asked and answers a stub.</summary>
public sealed class RecordingPagePrinter : IPagePrinter
{
    public static readonly byte[] Stub = "%PDF-stub"u8.ToArray();

    public (string Path, string Zone, bool Session)? Last { get; private set; }
    public bool Fail { get; set; }

    public Task<byte[]> PrintAsync(
        HttpContext caller,
        string path,
        TimeZoneInfo timeZone,
        CancellationToken cancellationToken
    )
    {
        Last = (path, timeZone.Id, caller.Request.Cookies.ContainsKey("inventory.session"));
        return Fail ? throw new PagePrintException("Stubbed failure.") : Task.FromResult(Stub);
    }
}

/// <summary>Ana holds a device; the printer is the recording stub; the place is Banja Luka.</summary>
public class HandoverPdfApplication : InventoryApplication
{
    public RecordingPagePrinter Printer { get; } = new();
    public Guid Ana { get; private set; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.UseSetting("Handover:Place", " Banja Luka ");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IPagePrinter>();
            services.AddSingleton<IPagePrinter>(Printer);
        });
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<InventoryContext>();
        Ana = (await Seed.BasicsAsync(context)).Person;
        await Seed.DeviceAsync(context, "Ana's laptop", holdsLicenses: true);
    }
}

public class HandoverPdfTests(HandoverPdfApplication app) : IClassFixture<HandoverPdfApplication>
{
    private string Url(Guid id, string query = "?tz=Europe/Belgrade") =>
        $"/api/company/people/{id}/handover.pdf{query}";

    [Fact]
    public async Task Refuses_a_caller_without_a_session() =>
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            (await app.CreateClient().GetAsync(Url(app.Ana))).StatusCode
        );

    [Fact]
    public async Task The_sheet_offers_a_pdf_and_fills_in_the_place()
    {
        var sheet = (
            await (await app.ClientAsync()).GetFromJsonAsync<Handover>(
                $"/api/company/people/{app.Ana}/handover"
            )
        )!;

        Assert.Equal(("Banja Luka", true), (sheet.Place, sheet.Pdf));
    }

    [Fact]
    public async Task Prints_the_sheets_page_as_the_caller_in_their_zone()
    {
        var response = await (await app.ClientAsync()).GetAsync(Url(app.Ana));

        response.EnsureSuccessStatusCode();
        Assert.Equal("application/pdf", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(
            "Handover sheet - Seed person.pdf",
            response.Content.Headers.ContentDisposition?.FileNameStar
        );
        Assert.Equal(RecordingPagePrinter.Stub, await response.Content.ReadAsByteArrayAsync());
        Assert.Equal(
            ($"/company/people/{app.Ana}/handover", "Europe/Belgrade", true),
            app.Printer.Last
        );
    }

    [Fact]
    public async Task Prints_in_utc_when_no_zone_is_named()
    {
        (await (await app.ClientAsync()).GetAsync(Url(app.Ana, ""))).EnsureSuccessStatusCode();

        Assert.Equal("UTC", app.Printer.Last?.Zone);
    }

    [Fact]
    public async Task Refuses_a_zone_it_does_not_know()
    {
        var response = await (await app.ClientAsync()).GetAsync(Url(app.Ana, "?tz=Mars/Olympus"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains(
            "tz",
            (await response.Content.ReadFromJsonAsync<ValidationProblemDetails>())!.Errors.Keys
        );
    }

    [Fact]
    public async Task Answers_not_found_for_a_person_that_does_not_exist() =>
        Assert.Equal(
            HttpStatusCode.NotFound,
            (await (await app.ClientAsync()).GetAsync(Url(Guid.CreateVersion7()))).StatusCode
        );

    [Fact]
    public async Task Says_so_when_the_page_cannot_be_printed()
    {
        app.Printer.Fail = true;
        try
        {
            var response = await (await app.ClientAsync()).GetAsync(Url(app.Ana));

            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.Equal(
                "The handover sheet could not be printed.",
                (await response.Content.ReadFromJsonAsync<ProblemDetails>())!.Title
            );
        }
        finally
        {
            app.Printer.Fail = false;
        }
    }
}
