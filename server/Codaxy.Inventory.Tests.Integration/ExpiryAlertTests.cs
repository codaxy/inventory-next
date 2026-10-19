using System.Net;
using System.Net.Http.Json;
using Codaxy.Inventory.App.Administration.Roles;
using Codaxy.Inventory.App.Company.Locations;
using Codaxy.Inventory.App.Company.Manufacturers;
using Codaxy.Inventory.App.Licenses.SoftwareServices;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.ExpiryAlerts;
using Codaxy.Inventory.Tests.Integration.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Codaxy.Inventory.Tests.Integration;

/// <summary>
/// Software to license and a disposal location; each test makes its own records, and leaves nothing
/// due behind it, since a check sends whatever is due in the database.
/// </summary>
public class ExpiryAlertApplication : InventoryApplication
{
    public const string Url = "https://inventory.example.com";
    public const string WrittenOff = "Written off";

    public static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

    public static readonly string[] Recipients = ["it@codaxy.com", "ops@codaxy.com"];

    private Guid software;

    public Guid Disposal { get; private set; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.UseSetting("ApplicationUrl", Url);
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<InventoryContext>();

        var category = new SoftwareOrServiceCategory { Id = Guid.CreateVersion7(), Name = "App" };
        var maker = new Manufacturer { Id = Guid.CreateVersion7(), Name = "Maker" };
        var suite = new SoftwareOrService
        {
            Id = Guid.CreateVersion7(),
            Name = "Suite",
            SoftwareOrServiceCategoryId = category.Id,
            ManufacturerId = maker.Id,
        };
        var city = Guid.CreateVersion7();
        Disposal = Guid.CreateVersion7();
        context.AddRange(category, maker, suite);
        context.Countries.Add(new Country { Code = "US", Name = "United States" });
        context.Cities.Add(
            new City
            {
                Id = city,
                Name = "New York",
                CountryCode = "US",
            }
        );
        context.Locations.Add(
            new Location
            {
                Id = Disposal,
                Name = WrittenOff,
                CountryCode = "US",
                CityId = city,
                Street = "N/A",
                IsDisposal = true,
            }
        );
        await context.SaveChangesAsync();
        software = suite.Id;
    }

    public async Task<Guid> LicenseAsync(string name, DateOnly? expires, bool autoRenew = false)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<InventoryContext>();
        var id = (
            await Seed.LicenseWithVolumeAsync(context, software, name, seats: 0, expires: expires)
        ).License;
        var license = await context.Licenses.SingleAsync(l => l.AssetId == id);
        license.AutoRenew = autoRenew;
        await context.SaveChangesAsync();
        return id;
    }

    public async Task<Guid> DeviceAsync(string name, DateOnly? warranty, Guid? location = null)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<InventoryContext>();
        var id = await Seed.DeviceAsync(context, name, holdsLicenses: false);
        var asset = await context
            .Assets.Include(a => a.ElectronicDevice)
            .SingleAsync(a => a.Id == id);
        asset.ElectronicDevice.WarrantyExpirationDate = warranty;
        asset.LocationId = location;
        await context.SaveChangesAsync();
        return id;
    }

    public async Task MoveLicenseAsync(Guid id, DateOnly expires)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<InventoryContext>();
        var license = await context.Licenses.SingleAsync(l => l.AssetId == id);
        license.SubscriptionExpirationDate = expires;
        await context.SaveChangesAsync();
    }

    public async Task SettingsAsync(
        string[] recipients,
        AlertLead lead,
        params AlertLead[] reminders
    )
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<InventoryContext>();
        await Seed.SettingAsync(context, ExpiryAlertSettings.Recipients, recipients);
        await Seed.SettingAsync(context, ExpiryAlertSettings.Lead, lead);
        await Seed.SettingAsync(context, ExpiryAlertSettings.Reminders, reminders);
    }

    /// <summary>The stages recorded as sent for an asset's date, in order.</summary>
    public async Task<int[]> StagesAsync(Guid asset)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<InventoryContext>();
        return await context
            .ExpiryAlerts.Where(a => a.AssetId == asset)
            .OrderBy(a => a.Stage)
            .Select(a => a.Stage)
            .ToArrayAsync();
    }

    /// <summary>One check, and the email it sent, if any.</summary>
    public async Task<(int Alerted, SentEmail? Email)> CheckAsync()
    {
        Emails.Clear();
        using var scope = Services.CreateScope();
        var alerted = await scope
            .ServiceProvider.GetRequiredService<ExpiryAlertCheck>()
            .SendDueAsync(CancellationToken.None);
        return (alerted, Emails.All.SingleOrDefault());
    }
}

public class ExpiryAlertTests(ExpiryAlertApplication app) : IClassFixture<ExpiryAlertApplication>
{
    private static DateOnly Today => ExpiryAlertApplication.Today;

    private static readonly AlertLead TwoMonths = new(2, "months");

    [Fact]
    public async Task Alerts_what_ends_within_the_lead_in_one_email_to_every_recipient()
    {
        await app.SettingsAsync(ExpiryAlertApplication.Recipients, TwoMonths);
        await app.LicenseAsync("Ends today", Today);
        await app.LicenseAsync("Ends on the lead's last day", Today.AddMonths(2));
        await app.DeviceAsync("Warranty in 20 days", Today.AddDays(20));
        await app.LicenseAsync("Ends the day after the lead", Today.AddMonths(2).AddDays(1));
        await app.LicenseAsync("Ended yesterday", Today.AddDays(-1));
        await app.LicenseAsync("Never ends", null);
        await app.DeviceAsync("No warranty", null);

        var (_, email) = await app.CheckAsync();

        Assert.NotNull(email);
        Assert.Equal(ExpiryAlertApplication.Recipients, email.To);
        Assert.Contains("Ends today", email.Body);
        Assert.Contains("Ends on the lead's last day", email.Body);
        Assert.Contains("Warranty in 20 days", email.Body);
        Assert.DoesNotContain("Ends the day after the lead", email.Body);
        Assert.DoesNotContain("Ended yesterday", email.Body);
        Assert.DoesNotContain("Never ends", email.Body);
        Assert.DoesNotContain("No warranty", email.Body);
    }

    [Fact]
    public async Task Alerts_a_date_once()
    {
        await app.SettingsAsync(ExpiryAlertApplication.Recipients, TwoMonths);
        await app.LicenseAsync("Alerted once", Today.AddDays(5));

        var first = await app.CheckAsync();
        var second = await app.CheckAsync();

        Assert.Contains("Alerted once", first.Email!.Body);
        Assert.Equal(0, second.Alerted);
        Assert.Null(second.Email);
    }

    [Fact]
    public async Task A_moved_date_alerts_again()
    {
        await app.SettingsAsync(ExpiryAlertApplication.Recipients, TwoMonths);
        var license = await app.LicenseAsync("Renewed", Today.AddDays(5));
        await app.CheckAsync();

        await app.MoveLicenseAsync(license, Today.AddDays(30));
        var (_, email) = await app.CheckAsync();

        Assert.Contains("Renewed", email!.Body);
    }

    [Fact]
    public async Task Says_what_renews_automatically_and_what_is_disposed_of()
    {
        await app.SettingsAsync(ExpiryAlertApplication.Recipients, TwoMonths);
        await app.LicenseAsync("Auto-renewing", Today.AddDays(7), autoRenew: true);
        await app.DeviceAsync("Scrapped", Today.AddDays(7), app.Disposal);

        var (_, email) = await app.CheckAsync();

        Assert.Contains(
            "Renews automatically on that date unless it is cancelled.",
            Section(email!.Body, "Auto-renewing")
        );
        Assert.Contains(
            $"Disposed of, at {ExpiryAlertApplication.WrittenOff}.",
            Section(email.Body, "Scrapped")
        );
    }

    [Fact]
    public async Task Links_each_record()
    {
        await app.SettingsAsync(ExpiryAlertApplication.Recipients, TwoMonths);
        var license = await app.LicenseAsync("Linked license", Today.AddDays(3));
        var device = await app.DeviceAsync("Linked device", Today.AddDays(3));

        var (_, email) = await app.CheckAsync();

        Assert.Contains($"{ExpiryAlertApplication.Url}/licenses/{license}", email!.Body);
        Assert.Contains($"{ExpiryAlertApplication.Url}/electronic-devices/{device}", email.Body);
        Assert.Contains($"href=\"{ExpiryAlertApplication.Url}/licenses/{license}\"", email.Html);
    }

    [Fact]
    public async Task Without_recipients_nothing_is_sent_or_recorded()
    {
        await app.SettingsAsync([], TwoMonths);
        await app.LicenseAsync("Waiting for someone", Today.AddDays(9));

        var unsent = await app.CheckAsync();
        Assert.Equal(0, unsent.Alerted);
        Assert.Null(unsent.Email);

        await app.SettingsAsync(ExpiryAlertApplication.Recipients, TwoMonths);
        Assert.Contains("Waiting for someone", (await app.CheckAsync()).Email!.Body);
    }

    [Fact]
    public async Task A_failed_send_records_nothing_so_the_next_check_sends_it()
    {
        await app.SettingsAsync(ExpiryAlertApplication.Recipients, TwoMonths);
        await app.LicenseAsync("Relay was down", Today.AddDays(11));

        app.Emails.Failing = true;
        try
        {
            await Assert.ThrowsAsync<IOException>(app.CheckAsync);
        }
        finally
        {
            app.Emails.Failing = false;
        }

        Assert.Contains("Relay was down", (await app.CheckAsync()).Email!.Body);
    }

    [Fact]
    public async Task The_lead_may_be_weeks()
    {
        await app.SettingsAsync(ExpiryAlertApplication.Recipients, new(1, "weeks"));
        await app.DeviceAsync("Within the week", Today.AddDays(7));
        await app.DeviceAsync("Past the week", Today.AddDays(8));

        var (_, email) = await app.CheckAsync();

        Assert.Contains("Within the week", email!.Body);
        Assert.DoesNotContain("Past the week", email.Body);

        // Within two months, so a later check in this class sends it; this one does, now.
        await app.SettingsAsync(ExpiryAlertApplication.Recipients, TwoMonths);
        Assert.Contains("Past the week", (await app.CheckAsync()).Email!.Body);
    }

    [Fact]
    public async Task A_reminder_follows_the_first_notice_once_its_point_is_reached()
    {
        await app.SettingsAsync(ExpiryAlertApplication.Recipients, TwoMonths);
        var license = await app.LicenseAsync("Reminded later", Today.AddDays(10));
        Assert.Contains("Reminded later", (await app.CheckAsync()).Email!.Body);

        // Ten days out is within a reminder at two weeks: it is that reminder's turn.
        await app.SettingsAsync(
            ExpiryAlertApplication.Recipients,
            TwoMonths,
            new AlertLead(14, "days")
        );
        var (_, email) = await app.CheckAsync();

        Assert.Contains(
            "Reminder: subscription expires on",
            Section(email!.Body, "Reminded later")
        );
        Assert.Equal(new[] { 0, 1 }, await app.StagesAsync(license));
        Assert.Equal(0, (await app.CheckAsync()).Alerted);
    }

    [Fact]
    public async Task A_record_gets_the_furthest_stage_it_has_reached_not_the_ones_it_skipped()
    {
        await app.SettingsAsync(
            ExpiryAlertApplication.Recipients,
            TwoMonths,
            new AlertLead(14, "days"),
            new AlertLead(2, "days")
        );
        var device = await app.DeviceAsync("Entered two days out", Today.AddDays(2));

        var (_, email) = await app.CheckAsync();

        Assert.Single(email!.Body.Split('\n'), line => line.StartsWith("Entered two days out"));
        Assert.Contains("Reminder: warranty ends on", Section(email.Body, "Entered two days out"));
        Assert.Equal(new[] { 2 }, await app.StagesAsync(device));
        Assert.Equal(0, (await app.CheckAsync()).Alerted);
    }

    [Fact]
    public async Task Deleting_an_asset_takes_its_alerts()
    {
        await app.SettingsAsync(ExpiryAlertApplication.Recipients, TwoMonths);
        var device = await app.DeviceAsync("Deleted after its alert", Today.AddDays(4));
        await app.CheckAsync();

        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<InventoryContext>();
        await context.Assets.Where(a => a.Id == device).ExecuteDeleteAsync();

        Assert.False(await context.ExpiryAlerts.AnyAsync(a => a.AssetId == device));
    }

    /// <summary>A record's lines in the body: from its name to the blank line after it.</summary>
    private static string Section(string body, string name)
    {
        var start = body.IndexOf(name, StringComparison.Ordinal);
        var end = body.IndexOf("\n\n", start, StringComparison.Ordinal);
        return body[start..(end < 0 ? body.Length : end)];
    }
}

/// <summary>The hosted check, running as the application runs it, every fifth of a second.</summary>
public class ExpiryAlertWorkerApplication : ExpiryAlertApplication
{
    protected override bool RunsExpiryAlertWorker => true;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.UseSetting("ExpiryAlerts:Interval", "00:00:00.200");
    }
}

public class ExpiryAlertWorkerTests(ExpiryAlertWorkerApplication app)
    : IClassFixture<ExpiryAlertWorkerApplication>
{
    [Fact]
    public async Task Sends_what_becomes_due_at_its_next_check()
    {
        await app.LicenseAsync("Entered inside the lead", ExpiryAlertApplication.Today.AddDays(3));
        await app.SettingsAsync(ExpiryAlertApplication.Recipients, new(2, "months"));

        var deadline = DateTime.UtcNow.AddSeconds(10);
        while (
            DateTime.UtcNow < deadline
            && !app.Emails.All.Any(e => e.Body.Contains("Entered inside the lead"))
        )
            await Task.Delay(100);

        Assert.Single(app.Emails.All, e => e.Body.Contains("Entered inside the lead"));
    }
}

/// <summary>The hosted check on its hourly interval, so only being asked runs it in time.</summary>
public class ExpiryAlertOnSaveApplication : ExpiryAlertApplication
{
    protected override bool RunsExpiryAlertWorker => true;
}

public class ExpiryAlertOnSaveTests(ExpiryAlertOnSaveApplication app)
    : IClassFixture<ExpiryAlertOnSaveApplication>
{
    [Fact]
    public async Task Saving_the_alert_settings_checks_at_once()
    {
        await app.LicenseAsync(
            "Due when someone is named",
            ExpiryAlertApplication.Today.AddDays(3)
        );
        var client = await app.ClientAsync("settings.admin@codaxy.com", BuiltInRoles.SystemAdmin);

        var saved = await client.PutAsJsonAsync(
            "/api/administration/settings/alerts",
            new Dictionary<string, object>
            {
                ["alerts.recipients"] = new[] { "it@codaxy.com" },
                ["alerts.lead"] = new { amount = 2, unit = "months" },
                ["alerts.reminders"] = Array.Empty<object>(),
            }
        );

        Assert.Equal(HttpStatusCode.OK, saved.StatusCode);
        var deadline = DateTime.UtcNow.AddSeconds(10);
        while (
            DateTime.UtcNow < deadline
            && !app.Emails.All.Any(e => e.Body.Contains("Due when someone is named"))
        )
            await Task.Delay(100);

        Assert.Single(app.Emails.All, e => e.Body.Contains("Due when someone is named"));
    }
}
