using Microsoft.EntityFrameworkCore.Migrations;

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    public partial class UpdateInformationAndAssetTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Information",
                newName: "Incomplete"
            );

            migrationBuilder.RenameColumn(name: "Status", table: "Asset", newName: "Incomplete");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Incomplete",
                table: "Information",
                newName: "Status"
            );

            migrationBuilder.RenameColumn(name: "Incomplete", table: "Asset", newName: "Status");
        }
    }
}
