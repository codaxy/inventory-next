using System.Net.Http.Json;
using Codaxy.Inventory.App.Dashboard;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Access;
using Codaxy.Inventory.App.Shared.Documents;
using Codaxy.Inventory.App.Shared.Settings;
using Codaxy.Inventory.Tests.Integration.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using DocumentSettingsResponse = Codaxy.Inventory.App.Shared.Documents.Settings.Endpoint.Response;

namespace Codaxy.Inventory.Tests.Integration;

public class SettingSeedApplication : InventoryApplication;

/// <summary>What seeding adds to a database, what it leaves, and what a reader gets back.</summary>
public class SettingSeedTests(SettingSeedApplication app) : IClassFixture<SettingSeedApplication>
{
    private async Task<T> WithContextAsync<T>(Func<InventoryContext, Task<T>> act)
    {
        using var scope = app.Services.CreateScope();
        return await act(scope.ServiceProvider.GetRequiredService<InventoryContext>());
    }

    /// <summary>But the domains, which every fixture narrows to codaxy.com.</summary>
    [Fact]
    public async Task Every_setting_is_seeded_with_its_default() =>
        Assert.Equal(
            SettingCatalogue
                .All.Where(d => d != SignInSettings.AllowedDomains)
                .Select(d => (d.Key, d.DefaultJson))
                .OrderBy(s => s.Key),
            await WithContextAsync(async context =>
                (await context.Settings.AsNoTracking().ToListAsync())
                    .Where(s => s.Key != SignInSettings.AllowedDomains.Key)
                    .Select(s => (s.Key, Normalised(s.Value)))
                    .OrderBy(s => s.Key)
                    .ToList()
            )
        );

    [Fact]
    public async Task Seeding_again_restores_a_missing_setting_and_keeps_a_changed_one()
    {
        var key = DocumentSettings.Place.Key;
        var restored = DashboardSettings.WarrantiesEndingDays.Key;

        var (place, days) = await WithContextAsync(async context =>
        {
            await Seed.SettingAsync(context, DocumentSettings.Place, "Novi Sad");
            await context.Settings.Where(s => s.Key == restored).ExecuteDeleteAsync();

            await SettingCatalogue.SeedAsync(context, CancellationToken.None);

            var stored = await context
                .Settings.AsNoTracking()
                .ToDictionaryAsync(s => s.Key, s => s.Value);
            return (stored[key], stored[restored]);
        });

        Assert.Equal(("\"Novi Sad\"", "30"), (Normalised(place), Normalised(days)));

        await WithContextAsync(async context =>
        {
            await Seed.SettingAsync(context, DocumentSettings.Place, "");
            return 0;
        });
    }

    [Fact]
    public async Task A_value_edited_by_hand_into_nonsense_reads_as_its_default()
    {
        var language = DocumentSettings.DefaultLanguage.Key;
        await WithContextAsync(context =>
            context
                .Settings.Where(s => s.Key == language)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.Value, "\"klingon\""))
        );

        var settings = await (await app.ClientAsync()).GetFromJsonAsync<DocumentSettingsResponse>(
            "/api/documents/settings"
        );

        await WithContextAsync(context =>
            context
                .Settings.Where(s => s.Key == language)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.Value, "\"en\""))
        );
        Assert.Equal(new DocumentSettingsResponse("en", ""), settings);
    }

    /// <summary>PostgreSQL writes <c>jsonb</c> back in its own spacing.</summary>
    private static string Normalised(string json) =>
        System.Text.Json.JsonSerializer.Serialize(System.Text.Json.JsonDocument.Parse(json));
}
