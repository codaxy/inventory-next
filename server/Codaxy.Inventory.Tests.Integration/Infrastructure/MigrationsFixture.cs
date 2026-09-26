using Codaxy.Inventory.App.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Testcontainers.PostgreSql;

namespace Codaxy.Inventory.Tests.Integration.Infrastructure;

/// <summary>
/// A PostgreSQL container of its own, so the migration tests can build a schema from scratch without
/// touching the database the rest of the suite shares. Each test asks for a named database and gets an
/// empty one.
/// </summary>
public class MigrationsFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder(
        "postgres:16-alpine"
    ).Build();

    public async Task InitializeAsync() => await _postgres.StartAsync();

    public async Task DisposeAsync() => await _postgres.DisposeAsync();

    /// <summary>Creates an empty database and returns a context pointed at it.</summary>
    public async Task<InventoryContext> CreateEmptyDatabaseAsync(string name)
    {
        await using (var connection = new NpgsqlConnection(_postgres.GetConnectionString()))
        {
            await connection.OpenAsync();
            await using var drop = new NpgsqlCommand(
                $"DROP DATABASE IF EXISTS \"{name}\" WITH (FORCE)",
                connection
            );
            await drop.ExecuteNonQueryAsync();
            await using var create = new NpgsqlCommand($"CREATE DATABASE \"{name}\"", connection);
            await create.ExecuteNonQueryAsync();
        }

        var options = new DbContextOptionsBuilder<InventoryContext>()
            .UseNpgsql(ConnectionStringFor(name))
            .Options;

        return new InventoryContext(options);
    }

    private string ConnectionStringFor(string database) =>
        new NpgsqlConnectionStringBuilder(_postgres.GetConnectionString())
        {
            Database = database,
        }.ToString();
}
