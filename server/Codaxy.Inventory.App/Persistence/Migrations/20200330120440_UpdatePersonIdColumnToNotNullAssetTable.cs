using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    public partial class UpdatePersonIdColumnToNotNullAssetTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_Asset_Person_PersonId", table: "Asset");

            migrationBuilder.AlterColumn<Guid>(
                name: "PersonId",
                table: "Asset",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Asset_Person_PersonId",
                table: "Asset",
                column: "PersonId",
                principalTable: "Person",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_Asset_Person_PersonId", table: "Asset");

            migrationBuilder.AlterColumn<Guid>(
                name: "PersonId",
                table: "Asset",
                nullable: true,
                oldClrType: typeof(Guid)
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Asset_Person_PersonId",
                table: "Asset",
                column: "PersonId",
                principalTable: "Person",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );
        }
    }
}
