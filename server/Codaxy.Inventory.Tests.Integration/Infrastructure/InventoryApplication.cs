using Codaxy.Inventory.App.Persistence;
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
/// test can read the one-time code. The `Testing` environment is what skips migrating and seeding;
/// the schema here comes from the model, and the migrations are proven separately by
/// <see cref="MigrationsTests"/>.
/// </summary>
public class InventoryApplication : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer postgres = new PostgreSqlBuilder(
        "postgres:16-alpine"
    ).Build();

    public RecordingEmailSender Emails { get; } = new();

    private string? stubbedShell;

    /// <summary>Its own ring per run, so a test never writes to the configured path.</summary>
    public string KeyRingPath { get; } =
        Path.Combine(Path.GetTempPath(), "inventory-next-tests", Guid.CreateVersion7().ToString());

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
        builder.UseSetting("Auth:AllowedDomains:0", "codaxy.com");
        builder.UseSetting("Auth:OneTimeCode:Enabled", "true");
        builder.UseSetting("DataProtection:KeyRingPath", KeyRingPath);
        builder.UseSetting("ServerLog:Path", ServerLogPath);

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
        StubTheShell();

        await postgres.StartAsync();

        using var scope = Services.CreateScope();
        await scope
            .ServiceProvider.GetRequiredService<InventoryContext>()
            .Database.EnsureCreatedAsync();
    }

    public new async Task DisposeAsync()
    {
        if (stubbedShell is not null)
            File.Delete(stubbedShell);

        if (Directory.Exists(KeyRingPath))
            Directory.Delete(KeyRingPath, recursive: true);

        if (Directory.Exists(ServerLogPath))
            Directory.Delete(ServerLogPath, recursive: true);

        await postgres.DisposeAsync();
        await base.DisposeAsync();
    }

    /// <summary>
    /// The client is built into its own folder and copied into wwwroot by the image, so a source
    /// checkout has no shell to fall back to. The routing is the application's behaviour and is worth
    /// a test; the file is the build's job, so one is put there when it is missing.
    /// </summary>
    private void StubTheShell()
    {
        var shell = Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            "Codaxy.Inventory.Web",
            "wwwroot",
            "index.html"
        );

        shell = Path.GetFullPath(shell);

        if (File.Exists(shell))
            return;

        File.WriteAllText(shell, "<!doctype html><title>Inventory</title>");
        stubbedShell = shell;
    }
}
