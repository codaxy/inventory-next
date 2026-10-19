using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Codaxy.Inventory.App.Administration.Roles;
using Codaxy.Inventory.App.Shared.Access;
using Codaxy.Inventory.App.Shared.Settings;
using Codaxy.Inventory.Tests.Integration.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using DocumentSettingsResponse = Codaxy.Inventory.App.Shared.Documents.Settings.Endpoint.Response;
using Response = Codaxy.Inventory.App.Administration.Settings.List.Endpoint.Response;

namespace Codaxy.Inventory.Tests.Integration;

public class SettingsApplication : InventoryApplication;

/// <summary>Administration → Settings. Each test leaves the settings as it found them.</summary>
public class SettingsTests(SettingsApplication app) : IClassFixture<SettingsApplication>
{
    private const string Url = "/api/administration/settings";
    private const string Admin = "system.admin@codaxy.com";

    private Task<HttpClient> SystemAdminAsync() => app.ClientAsync(Admin, BuiltInRoles.SystemAdmin);

    private static Dictionary<string, object> Documents(object place, object language) =>
        new() { ["documents.place"] = place, ["documents.default_language"] = language };

    private static Dictionary<string, object> SignIn(params string[] domains) =>
        new() { ["sign_in.allowed_domains"] = domains };

    private static async Task<Dictionary<string, string[]>> ProblemsAsync(HttpResponseMessage r)
    {
        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
        return (
            await r.Content.ReadFromJsonAsync<ValidationProblemDetails>()
        )!.Errors.ToDictionary();
    }

    [Theory]
    [InlineData("GET", "")]
    [InlineData("PUT", "/documents")]
    public async Task Reading_and_saving_take_settings_manage(string method, string path)
    {
        var client = await app.ClientAsync("admin.only@codaxy.com", BuiltInRoles.Admin);

        var response = await client.SendAsync(
            new(new HttpMethod(method), Url + path)
            {
                Content = JsonContent.Create(Documents("Banja Luka", "en")),
            }
        );

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Every_setting_is_listed_in_the_catalogue_order_beside_its_default()
    {
        var settings = (await (await SystemAdminAsync()).GetFromJsonAsync<Response>(Url))!.Settings;

        Assert.Equal(SettingCatalogue.All.Select(d => d.Key), settings.Select(s => s.Key));
        var expired = settings.Single(s => s.Key == "dashboard.subscriptions_expired_days");
        Assert.Equal(("dashboard", 45), (expired.Group, expired.Default.GetInt32()));
    }

    [Fact]
    public async Task A_group_is_saved_tidied_and_read_at_once()
    {
        var client = await SystemAdminAsync();

        var saved = await client.PutAsJsonAsync(
            $"{Url}/documents",
            Documents("  Novi Sad ", "SR-LATN-BA")
        );
        var documents = await client.GetFromJsonAsync<DocumentSettingsResponse>(
            "/api/documents/settings"
        );
        await client.PutAsJsonAsync($"{Url}/documents", Documents("", "en"));

        Assert.Equal(HttpStatusCode.OK, saved.StatusCode);
        Assert.Equal(new DocumentSettingsResponse("sr-Latn-BA", "Novi Sad"), documents);
    }

    [Fact]
    public async Task A_group_with_any_problem_saves_none_of_it()
    {
        var client = await SystemAdminAsync();

        var problems = await ProblemsAsync(
            await client.PutAsJsonAsync(
                $"{Url}/dashboard",
                new Dictionary<string, object>
                {
                    ["dashboard.subscriptions_expired_days"] = 60,
                    ["dashboard.subscriptions_expiring_days"] = 0,
                    ["dashboard.warranties_ended_days"] = "soon",
                    ["dashboard.elsewhere"] = 1,
                }
            )
        );
        var after = (await client.GetFromJsonAsync<Response>(Url))!.Settings;

        Assert.Equal(
            new Dictionary<string, string[]>
            {
                ["dashboard.subscriptions_expiring_days"] = ["At least 1 day."],
                ["dashboard.warranties_ended_days"] = ["Not a valid value."],
                ["dashboard.warranties_ending_days"] = ["Required."],
                ["dashboard.elsewhere"] = ["Not a setting of dashboard."],
            },
            problems
        );
        Assert.Equal(
            45,
            after.Single(s => s.Key == "dashboard.subscriptions_expired_days").Value.GetInt32()
        );
    }

    [Fact]
    public async Task A_language_documents_are_not_printed_in_is_refused()
    {
        var problems = await ProblemsAsync(
            await (await SystemAdminAsync()).PutAsJsonAsync($"{Url}/documents", Documents("", "de"))
        );

        Assert.Equal(["Must be one of: en, sr-Latn-BA."], problems["documents.default_language"]);
    }

    [Fact]
    public async Task The_alerts_are_saved_tidied_and_read_as_their_definition_writes_them()
    {
        var client = await SystemAdminAsync();

        var saved = await client.PutAsJsonAsync(
            $"{Url}/alerts",
            new Dictionary<string, object>
            {
                ["alerts.recipients"] = new[] { " IT@codaxy.com ", "it@codaxy.com" },
                ["alerts.lead"] = new { unit = "Weeks", amount = 3 },
            }
        );
        var alerts = (await client.GetFromJsonAsync<Response>(Url))!
            .Settings.Where(s => s.Group == "alerts")
            .ToDictionary(s => s.Key, s => (s.Value.GetRawText(), s.Default.GetRawText()));
        await client.PutAsJsonAsync(
            $"{Url}/alerts",
            new Dictionary<string, object>
            {
                ["alerts.recipients"] = new[] { "admin@codaxy.com" },
                ["alerts.lead"] = new { amount = 2, unit = "months" },
            }
        );

        Assert.Equal(HttpStatusCode.OK, saved.StatusCode);
        // jsonb stores the lead's keys as {"unit","amount"}; the screen compares the text.
        Assert.Equal(
            new Dictionary<string, (string, string)>
            {
                ["alerts.recipients"] = ("[\"it@codaxy.com\"]", "[\"admin@codaxy.com\"]"),
                ["alerts.lead"] = (
                    "{\"amount\":3,\"unit\":\"weeks\"}",
                    "{\"amount\":2,\"unit\":\"months\"}"
                ),
            },
            alerts
        );
    }

    [Fact]
    public async Task An_unknown_group_is_not_found() =>
        Assert.Equal(
            HttpStatusCode.NotFound,
            (await (await SystemAdminAsync()).PutAsJsonAsync($"{Url}/weather", SignIn())).StatusCode
        );

    [Fact]
    public async Task Domains_are_saved_as_an_address_writes_them_and_apply_at_once()
    {
        var client = await SystemAdminAsync();

        var saved = await client.PutAsJsonAsync(
            $"{Url}/sign_in",
            SignIn(" @Codaxy.com", "codaxy.com", "Example.org ")
        );
        var outsider = await app.Client()
            .PostAsJsonAsync(
                "/api/auth/one-time-code/request",
                new { email = "someone@example.org" }
            );
        await client.PutAsJsonAsync($"{Url}/sign_in", SignIn("codaxy.com"));

        var domains = (await saved.Content.ReadFromJsonAsync<Response>())!
            .Settings.Single(s => s.Key == SignInSettings.AllowedDomains.Key)
            .Value.Deserialize<string[]>();
        Assert.Equal(["codaxy.com", "example.org"], domains);
        Assert.Equal(HttpStatusCode.NoContent, outsider.StatusCode);
    }

    [Fact]
    public async Task No_domains_lets_any_domain_ask_for_a_code()
    {
        var client = await SystemAdminAsync();

        await client.PutAsJsonAsync($"{Url}/sign_in", SignIn());
        var anyone = await app.Client()
            .PostAsJsonAsync(
                "/api/auth/one-time-code/request",
                new { email = "someone@anywhere.net" }
            );
        await client.PutAsJsonAsync($"{Url}/sign_in", SignIn("codaxy.com"));

        Assert.Equal(HttpStatusCode.NoContent, anyone.StatusCode);
    }

    [Fact]
    public async Task Domains_that_would_shut_out_a_system_admin_are_refused_naming_them()
    {
        var problems = await ProblemsAsync(
            await (await SystemAdminAsync()).PutAsJsonAsync($"{Url}/sign_in", SignIn("example.org"))
        );

        Assert.Equal(
            [$"System Admin {Admin} could no longer sign in."],
            problems[SignInSettings.AllowedDomains.Key]
        );
    }

    [Fact]
    public async Task What_is_not_a_domain_is_refused() =>
        Assert.Equal(
            ["\"codaxy\" is not a domain."],
            (
                await ProblemsAsync(
                    await (await SystemAdminAsync()).PutAsJsonAsync(
                        $"{Url}/sign_in",
                        SignIn("codaxy")
                    )
                )
            )[SignInSettings.AllowedDomains.Key]
        );
}
