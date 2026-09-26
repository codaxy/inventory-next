using Microsoft.EntityFrameworkCore.Migrations;

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    public partial class AddNavigationPropertyInAssetTypeAndLicenseTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_License_CurrencyId",
                table: "License",
                column: "CurrencyId"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_License_Currency_CurrencyId",
                table: "License",
                column: "CurrencyId",
                principalTable: "Currency",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_License_Currency_CurrencyId",
                table: "License"
            );

            migrationBuilder.DropIndex(name: "IX_License_CurrencyId", table: "License");
        }
    }
}
