using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.Tests.Integration.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Codaxy.Inventory.Tests.Integration;

/// <summary>
/// The rest of the suite builds its schema with <c>EnsureCreated</c>, which reads the model and never
/// runs a migration. These tests are the only thing that executes one, so a migration that does not
/// apply, or that has stopped describing the model, fails here or nowhere.
/// </summary>
public class MigrationsTests(MigrationsFixture fixture) : IClassFixture<MigrationsFixture>
{
    [Fact]
    public async Task Every_migration_applies_to_an_empty_database()
    {
        await using var context = await fixture.CreateEmptyDatabaseAsync("migrations_apply");

        await context.Database.MigrateAsync();

        var expected = context.Database.GetMigrations().ToList();
        var applied = (await context.Database.GetAppliedMigrationsAsync()).ToList();
        var pending = (await context.Database.GetPendingMigrationsAsync()).ToList();

        Assert.NotEmpty(expected);
        Assert.Equal(expected, applied);
        Assert.Empty(pending);
    }

    [Fact]
    public async Task Migrations_still_describe_the_model()
    {
        await using var context = await fixture.CreateEmptyDatabaseAsync("migrations_model");

        Assert.False(
            context.Database.HasPendingModelChanges(),
            "The model has changed without a migration. Run `dotnet ef migrations add <Name> "
                + "--project Codaxy.Inventory.App --startup-project Codaxy.Inventory.Web` in server/."
        );
    }

    /// <summary>
    /// The model and the migrations can agree while the SQL a migration actually runs does not — a
    /// hand-edited migration is invisible to <c>HasPendingModelChanges</c>. This compares the two
    /// schemas as the database reports them.
    /// </summary>
    [Fact]
    public async Task Migrated_schema_matches_the_schema_the_model_builds()
    {
        await using var migrated = await fixture.CreateEmptyDatabaseAsync("schema_migrated");
        await migrated.Database.MigrateAsync();

        await using var created = await fixture.CreateEmptyDatabaseAsync("schema_created");
        await created.Database.EnsureCreatedAsync();

        var fromMigrations = await ReadSchemaAsync(migrated);
        var fromModel = await ReadSchemaAsync(created);

        var onlyInMigrations = fromMigrations.Except(fromModel).ToList();
        var onlyInModel = fromModel.Except(fromMigrations).ToList();

        Assert.True(
            onlyInMigrations.Count == 0 && onlyInModel.Count == 0,
            Describe(onlyInMigrations, onlyInModel)
        );
    }

    private static string Describe(List<string> onlyInMigrations, List<string> onlyInModel)
    {
        var lines = new List<string> { "The migrated schema and the model's schema differ." };
        lines.AddRange(onlyInMigrations.Select(x => $"  migrations only: {x}"));
        lines.AddRange(onlyInModel.Select(x => $"  model only:      {x}"));
        return string.Join(Environment.NewLine, lines);
    }

    /// <summary>
    /// Columns, constraints and indexes as PostgreSQL reports them. The migrations history table is
    /// excluded: only the migrated database has one.
    /// </summary>
    private static async Task<HashSet<string>> ReadSchemaAsync(InventoryContext context)
    {
        const string sql = """
            SELECT 'column   ' || table_name || '.' || column_name
                   || ' ' || data_type
                   || CASE WHEN is_nullable = 'YES' THEN ' null' ELSE ' not null' END
                   || COALESCE(' default ' || column_default, '')
            FROM information_schema.columns
            WHERE table_schema = 'public' AND table_name <> '__EFMigrationsHistory'
            UNION ALL
            SELECT 'constraint ' || conrelid::regclass::text || ' ' || conname
                   || ' ' || pg_get_constraintdef(oid)
            FROM pg_constraint
            WHERE connamespace = 'public'::regnamespace
              AND conrelid::regclass::text NOT LIKE '%__EFMigrationsHistory%'
            UNION ALL
            SELECT 'index    ' || indexdef
            FROM pg_indexes
            WHERE schemaname = 'public' AND tablename <> '__EFMigrationsHistory'
            """;

        var schema = new HashSet<string>();

        await using var connection = new NpgsqlConnection(context.Database.GetConnectionString());
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            schema.Add(reader.GetString(0));

        return schema;
    }
}
