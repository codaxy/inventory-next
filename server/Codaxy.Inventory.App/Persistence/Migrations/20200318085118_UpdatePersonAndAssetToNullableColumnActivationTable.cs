using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    public partial class UpdatePersonAndAssetToNullableColumnActivationTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Activation_Asset_AssetId",
                table: "Activation"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_Activation_Person_PersonId",
                table: "Activation"
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "PersonId",
                table: "Activation",
                nullable: true,
                oldClrType: typeof(Guid)
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "AssetId",
                table: "Activation",
                nullable: true,
                oldClrType: typeof(Guid)
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Activation_Asset_AssetId",
                table: "Activation",
                column: "AssetId",
                principalTable: "Asset",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Activation_Person_PersonId",
                table: "Activation",
                column: "PersonId",
                principalTable: "Person",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Activation_Asset_AssetId",
                table: "Activation"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_Activation_Person_PersonId",
                table: "Activation"
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "PersonId",
                table: "Activation",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "AssetId",
                table: "Activation",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Activation_Asset_AssetId",
                table: "Activation",
                column: "AssetId",
                principalTable: "Asset",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Activation_Person_PersonId",
                table: "Activation",
                column: "PersonId",
                principalTable: "Person",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );
        }
    }
}
