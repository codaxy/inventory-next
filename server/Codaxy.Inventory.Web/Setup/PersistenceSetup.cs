using Codaxy.Inventory.App.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.Web.Setup;

public static class PersistenceSetup
{
    public static IServiceCollection AddInventoryPersistence(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // Checked here rather than left to Npgsql, which reports a missing connection string as a
        // malformed one and sends the reader looking for a typo that is not there.
        var connectionString = configuration.GetConnectionString("PostgreSQL");

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                "ConnectionStrings:PostgreSQL is not set. Supply it in appsettings.Development.json, "
                    + "in user secrets, or as ConnectionStrings__PostgreSQL in the environment."
            );

        return services.AddDbContext<InventoryContext>(options =>
            options.UseNpgsql(connectionString)
        );
    }

    /// <summary>
    /// Brings the database up to date and fills the codebooks, governed by
    /// <c>Database:MigrateOnStartup</c>; seeding follows migrating and the setting covers both.
    ///
    /// Two applications share this database and both migrate it at startup. Their histories are
    /// identical, so <c>Migrate()</c> finds nothing to do from whichever starts second, and EF
    /// serialises concurrent calls — see docs/engineering/co-existence.md.
    /// </summary>
    public static void MigrateAndSeed(this WebApplication app)
    {
        if (!app.Configuration.GetValue("Database:MigrateOnStartup", true))
            return;

        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<InventoryContext>();

        // The history table predates the naming migration and no migration covers it, so its columns
        // are renamed before EF reads them.
        MigrationsHistoryNaming.Apply(context.Database);
        context.Database.Migrate();

        new InventoryContextSeedData(context).SeedData();
    }
}
