using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.Tests.Integration.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.Tests.Integration;

/// <summary>
/// The history table predates the snake_case naming and no migration covers it, so this runs before
/// <c>Migrate()</c> at startup. If it stops working, every database written before that migration
/// locks the application out entirely.
/// </summary>
public class MigrationsHistoryNamingTests(MigrationsFixture fixture)
    : IClassFixture<MigrationsFixture>
{
    [Fact]
    public async Task Renames_a_history_table_written_before_the_naming_migration()
    {
        await using var context = await fixture.CreateEmptyDatabaseAsync("history_pascal");

        await context.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE "__EFMigrationsHistory" (
                "MigrationId" character varying(150) NOT NULL,
                "ProductVersion" character varying(32) NOT NULL,
                CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
            )
            """
        );

        MigrationsHistoryNaming.Apply(context.Database);

        var columns = await ColumnsOfHistoryTable(context);

        Assert.Contains("migration_id", columns);
        Assert.DoesNotContain("MigrationId", columns);
    }

    [Fact]
    public async Task Leaves_a_history_table_that_is_already_renamed()
    {
        await using var context = await fixture.CreateEmptyDatabaseAsync("history_snake");

        await context.Database.MigrateAsync();

        MigrationsHistoryNaming.Apply(context.Database);

        Assert.Contains("migration_id", await ColumnsOfHistoryTable(context));
    }

    [Fact]
    public async Task Does_nothing_when_there_is_no_history_table_yet()
    {
        await using var context = await fixture.CreateEmptyDatabaseAsync("history_absent");

        MigrationsHistoryNaming.Apply(context.Database);

        Assert.Empty(await ColumnsOfHistoryTable(context));
    }

    private static async Task<List<string>> ColumnsOfHistoryTable(InventoryContext context)
    {
        var columns = new List<string>();

        await using var connection = context.Database.GetDbConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT column_name FROM information_schema.columns
            WHERE table_schema = 'public' AND table_name = '__EFMigrationsHistory'
            """;

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
            columns.Add(reader.GetString(0));

        return columns;
    }
}
