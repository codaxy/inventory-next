using Codaxy.Inventory.App.Administration.Roles;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Access;
using Codaxy.Inventory.App.Shared.Settings;
using Codaxy.Inventory.Web.Auth;
using Codaxy.Inventory.Web.Setup;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;

namespace Codaxy.Inventory.Tests.Integration.Infrastructure;

/// <summary>
/// The application as it runs, against a real PostgreSQL container, with the mail relay replaced so a
/// test can read the one-time code, and the built-in roles seeded. The `Testing` environment is what skips migrating and seeding;
/// the schema here comes from the model, and the migrations are proven separately by
/// <see cref="MigrationsTests"/>.
/// </summary>
public class InventoryApplication : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer postgres = new PostgreSqlBuilder(
        "postgres:16-alpine"
    ).Build();

    public RecordingEmailSender Emails { get; } = new();

    /// <summary>Its own ring per run, so a test never writes to the configured path.</summary>
    public string KeyRingPath { get; } =
        Path.Combine(Path.GetTempPath(), "inventory-next-tests", Guid.CreateVersion7().ToString());

    /// <summary>
    /// Its own web root per run, holding a stand-in shell. The client is built into its own folder and
    /// copied into wwwroot by the image, so a checkout has no shell; the routing is the application's
    /// behaviour and worth a test, the file is the build's job. Not written into the source tree's
    /// wwwroot: fixtures run in parallel, and one deleting the file there as it ends pulls the shell
    /// from under the rest.
    /// </summary>
    public string WebRootPath { get; } =
        Path.Combine(
            Path.GetTempPath(),
            "inventory-next-tests",
            Guid.CreateVersion7().ToString(),
            "wwwroot"
        );

    /// <summary>Its own log folder per run, so a test never writes into the source tree.</summary>
    public string ServerLogPath { get; } =
        Path.Combine(
            Path.GetTempPath(),
            "inventory-next-tests",
            Guid.CreateVersion7().ToString(),
            "logs"
        );

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // UseSetting rather than ConfigureAppConfiguration: the latter is applied after Program has
        // read its configuration, so a setting the application checks at startup would not be there
        // yet.
        builder.UseSetting("ConnectionStrings:PostgreSQL", postgres.GetConnectionString());
        builder.UseSetting("Auth:OneTimeCode:Enabled", "true");
        builder.UseSetting("DataProtection:KeyRingPath", KeyRingPath);
        builder.UseSetting("ServerLog:Path", ServerLogPath);

        Directory.CreateDirectory(WebRootPath);
        File.WriteAllText(
            Path.Combine(WebRootPath, "index.html"),
            "<!doctype html><title>Inventory</title>"
        );
        builder.UseWebRoot(WebRootPath);

        // The schema here comes from the model through EnsureCreated, and the codebooks stay empty so
        // a test seeds exactly what it needs. Migrations are proven against their own database by
        // MigrationsTests; running them for every fixture would prove the same thing more slowly.
        builder.UseSetting("Database:MigrateOnStartup", "false");

        // Every request in the suite arrives from the same address, so the caller limit would count
        // whole test classes as one attacker. RateLimitTests sets its own.
        builder.UseSetting("RateLimit:SignIn:Permits", "1000");

        // A test signs in more than once a minute, which a real caller has no reason to.
        builder.UseSetting("Auth:OneTimeCode:Cooldown", "00:00:00");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IEmailSender>();
            services.AddSingleton<IEmailSender>(Emails);
        });
    }

    public virtual async Task InitializeAsync()
    {
        await postgres.StartAsync();

        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<InventoryContext>();
        await context.Database.EnsureCreatedAsync();

        // The roles and the settings are the application's, not a test's: every database has them.
        await BuiltInRoles.SeedAsync(context, CancellationToken.None);
        await SettingCatalogue.SeedAsync(context, CancellationToken.None);
        await Seed.SettingAsync(context, SignInSettings.AllowedDomains, ["codaxy.com"]);
    }

    public new async Task DisposeAsync()
    {
        if (Directory.Exists(KeyRingPath))
            Directory.Delete(KeyRingPath, recursive: true);

        if (Directory.Exists(ServerLogPath))
            Directory.Delete(ServerLogPath, recursive: true);

        if (Directory.Exists(WebRootPath))
            Directory.Delete(WebRootPath, recursive: true);

        await postgres.DisposeAsync();
        await base.DisposeAsync();
    }
}
