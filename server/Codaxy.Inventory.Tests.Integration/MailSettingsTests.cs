using System.Net;
using System.Net.Http.Json;
using Codaxy.Inventory.App.Administration.Roles;
using Codaxy.Inventory.App.Administration.Settings.TestEmail;
using Codaxy.Inventory.App.Shared.Email;
using Codaxy.Inventory.Tests.Integration.Infrastructure;
using Microsoft.AspNetCore.Hosting;

namespace Codaxy.Inventory.Tests.Integration;

/// <summary>A relay that signs in, so what the screen is told can be checked for the password.</summary>
public class MailSettingsApplication : InventoryApplication
{
    public const string Password = "never-shown-4471";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.UseSetting("Smtp:Host", "smtp.example.com");
        builder.UseSetting("Smtp:Port", "587");
        builder.UseSetting("Smtp:From", "noreply@codaxy.com");
        builder.UseSetting("Smtp:FromName", "Codaxy Inventory");
        builder.UseSetting("Smtp:UserName", "noreply@codaxy.com");
        builder.UseSetting("Smtp:Password", Password);
        builder.UseSetting("ApplicationUrl", "https://inventory.example.com");
    }
}

public class MailSettingsTests(MailSettingsApplication app) : IClassFixture<MailSettingsApplication>
{
    private const string Admin = "mail.admin@codaxy.com";

    private Task<HttpClient> SystemAdminAsync() => app.ClientAsync(Admin, BuiltInRoles.SystemAdmin);

    [Fact]
    public async Task Shows_how_mail_is_sent_but_never_the_password()
    {
        var response = await (await SystemAdminAsync()).GetAsync(
            "/api/administration/settings/mail"
        );
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(
            new EmailRelay(
                "smtp.example.com",
                587,
                "noreply@codaxy.com",
                "Codaxy Inventory",
                "noreply@codaxy.com"
            ),
            await response.Content.ReadFromJsonAsync<EmailRelay>()
        );
        Assert.DoesNotContain(MailSettingsApplication.Password, body);
    }

    [Fact]
    public async Task A_test_email_goes_to_whoever_asked_for_it()
    {
        var client = await SystemAdminAsync();
        app.Emails.Clear();

        var result = await (
            await client.PostAsync("/api/administration/settings/mail/test", null)
        ).Content.ReadFromJsonAsync<Endpoint.Response>();

        Assert.Equal(new Endpoint.Response(Admin, true, null), result);
        var sent = Assert.Single(app.Emails.All);
        Assert.Equal([Admin], sent.To);
        Assert.Equal(Endpoint.Subject, sent.Subject);
        Assert.Contains(
            "Sent through smtp.example.com:587 from Administration → Settings "
                + "(https://inventory.example.com/administration/settings).",
            sent.Body
        );
        Assert.Contains(
            "<a href=\"https://inventory.example.com/administration/settings\">Administration → Settings</a>",
            sent.Html
        );
    }

    [Fact]
    public async Task A_failed_test_says_what_the_relay_said()
    {
        var client = await SystemAdminAsync();

        app.Emails.Failing = true;
        try
        {
            var result = await (
                await client.PostAsync("/api/administration/settings/mail/test", null)
            ).Content.ReadFromJsonAsync<Endpoint.Response>();

            Assert.Equal(new Endpoint.Response(Admin, false, "The relay is down."), result);
        }
        finally
        {
            app.Emails.Failing = false;
        }
    }

    [Theory]
    [InlineData("GET", "/api/administration/settings/mail")]
    [InlineData("POST", "/api/administration/settings/mail/test")]
    public async Task Both_take_settings_manage(string method, string path)
    {
        var client = await app.ClientAsync("plain.admin@codaxy.com", BuiltInRoles.Admin);

        var response = await client.SendAsync(new(new HttpMethod(method), path));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
