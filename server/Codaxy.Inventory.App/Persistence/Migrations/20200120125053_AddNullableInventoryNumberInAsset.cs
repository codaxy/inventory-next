using Microsoft.EntityFrameworkCore.Migrations;

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    public partial class AddNullableInventoryNumberInAsset : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "InventoryNumber",
                table: "Asset",
                nullable: true,
                oldClrType: typeof(int)
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "InventoryNumber",
                table: "Asset",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true
            );
        }
    }
}
