using Codaxy.Inventory.App.Administration.Roles;
using Codaxy.Inventory.App.Administration.Users;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.Web.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

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

        // Every save records what it changed; see AuditLogInterceptor.
        services.AddHttpContextAccessor();
        services.AddSingleton<ICurrentUser, HttpCurrentUser>();
        services.AddSingleton<AuditLogInterceptor>();

        return services.AddDbContext<InventoryContext>(
            (provider, options) =>
                options
                    .UseNpgsql(connectionString)
                    .AddInterceptors(provider.GetRequiredService<AuditLogInterceptor>())
        );
    }

    /// <summary>
    /// Brings the database up to date, fills the codebooks and the built-in roles, and creates the
    /// configured users, governed by <c>Database:MigrateOnStartup</c>, which covers all of it. A
    /// database left without an active System Admin stops the start.
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
        BuiltInRoles.SeedAsync(context, CancellationToken.None).GetAwaiter().GetResult();
        SeedUsers
            .ApplyAsync(
                context,
                scope.ServiceProvider.GetRequiredService<IOptions<UserOptions>>().Value,
                TimeProvider.System
            )
            .GetAwaiter()
            .GetResult();
    }
}
