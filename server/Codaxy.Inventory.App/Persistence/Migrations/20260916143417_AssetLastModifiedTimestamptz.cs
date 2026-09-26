using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AssetLastModifiedTimestamptz : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Written as raw SQL rather than AlterColumn: Postgres converts a naive
            // timestamp using the session time zone, and every value in this column was
            // written as UTC. The explicit USING says so instead of relying on the
            // server being configured for UTC.
            migrationBuilder.Sql(
                """
                ALTER TABLE "Asset"
                    ALTER COLUMN "LastModified" TYPE timestamp with time zone
                    USING "LastModified" AT TIME ZONE 'UTC';
                """
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ALTER TABLE "Asset"
                    ALTER COLUMN "LastModified" TYPE timestamp without time zone
                    USING "LastModified" AT TIME ZONE 'UTC';
                """
            );
        }
    }
}
