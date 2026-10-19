using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ExpiryAlertStages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_expiry_alert_asset_id_expiration_date",
                table: "expiry_alert");

            migrationBuilder.AddColumn<int>(
                name: "stage",
                table: "expiry_alert",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "ix_expiry_alert_asset_id_expiration_date_stage",
                table: "expiry_alert",
                columns: new[] { "asset_id", "expiration_date", "stage" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_expiry_alert_asset_id_expiration_date_stage",
                table: "expiry_alert");

            // The index coming back allows one row per asset and date: the reminders' rows go first.
            migrationBuilder.Sql("delete from expiry_alert where stage > 0;");

            migrationBuilder.DropColumn(
                name: "stage",
                table: "expiry_alert");

            migrationBuilder.CreateIndex(
                name: "ix_expiry_alert_asset_id_expiration_date",
                table: "expiry_alert",
                columns: new[] { "asset_id", "expiration_date" },
                unique: true);
        }
    }
}
