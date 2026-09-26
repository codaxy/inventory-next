using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    public partial class AddForeignKeyBEntityInAssetTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "BusinessEntity", table: "Asset");

            migrationBuilder.AddColumn<Guid>(
                name: "BusinessEntityId",
                table: "Asset",
                nullable: false,
                defaultValue: new Guid("7db152cd-2f48-4f53-94f8-37a84fadf8f0")
            );

            migrationBuilder.CreateIndex(
                name: "IX_Asset_BusinessEntityId",
                table: "Asset",
                column: "BusinessEntityId"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Asset_BusinessEntity_BusinessEntityId",
                table: "Asset",
                column: "BusinessEntityId",
                principalTable: "BusinessEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Asset_BusinessEntity_BusinessEntityId",
                table: "Asset"
            );

            migrationBuilder.DropIndex(name: "IX_Asset_BusinessEntityId", table: "Asset");

            migrationBuilder.DropColumn(name: "BusinessEntityId", table: "Asset");

            migrationBuilder.AddColumn<string>(
                name: "BusinessEntity",
                table: "Asset",
                nullable: true
            );
        }
    }
}
