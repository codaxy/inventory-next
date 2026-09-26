#nullable disable

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Codaxy.Inventory.App.Persistence;

/// <summary>
/// The snake_case convention renames the migrations history columns along with every other
/// column, but EF reads that table to decide which migrations to apply — before it applies any.
/// A database written before the convention landed holds <c>MigrationId</c> and
/// <c>ProductVersion</c> while EF asks for <c>migration_id</c>, and no migration can fix that
/// because none of them gets to run.
///
/// So it is renamed here, before <c>Migrate()</c>. Idempotent, and a no-op on a database that
/// never had the old names. Deletable once every database has started at least once on this
/// version or later.
/// </summary>
public static class MigrationsHistoryNaming
{
    public const string Sql = """
        DO $$
        BEGIN
            IF EXISTS (
                SELECT 1 FROM information_schema.columns
                WHERE table_schema = 'public'
                  AND table_name = '__EFMigrationsHistory'
                  AND column_name = 'MigrationId'
            ) THEN
                ALTER TABLE "__EFMigrationsHistory" RENAME COLUMN "MigrationId" TO migration_id;
                ALTER TABLE "__EFMigrationsHistory" RENAME COLUMN "ProductVersion" TO product_version;
            END IF;
        END $$;
        """;

    public static void Apply(DatabaseFacade database) => database.ExecuteSqlRaw(Sql);
}
