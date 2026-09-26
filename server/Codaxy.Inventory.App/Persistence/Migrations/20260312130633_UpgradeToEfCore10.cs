using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpgradeToEfCore10 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // No-op: the snapshot diff is purely cosmetic (timestamp with/without time zone).
            // The database columns are already correct because
            // Npgsql.EnableLegacyTimestampBehavior was always active at runtime.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) { }
    }
}
