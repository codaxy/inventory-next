using Microsoft.EntityFrameworkCore.Migrations;

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    public partial class SetRestricDelete : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_Volume_License_LicenseId", table: "Volume");

            migrationBuilder.AddForeignKey(
                name: "FK_Volume_License_LicenseId",
                table: "Volume",
                column: "LicenseId",
                principalTable: "License",
                principalColumn: "AssetId",
                onDelete: ReferentialAction.Restrict
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_Volume_License_LicenseId", table: "Volume");

            migrationBuilder.AddForeignKey(
                name: "FK_Volume_License_LicenseId",
                table: "Volume",
                column: "LicenseId",
                principalTable: "License",
                principalColumn: "AssetId",
                onDelete: ReferentialAction.Cascade
            );
        }
    }
}
