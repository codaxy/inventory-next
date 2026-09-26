using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    public partial class AddNullableColumnInFurniture : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Furniture_Manufacturer_ManufacturerId",
                table: "Furniture"
            );

            migrationBuilder.AlterColumn<DateTime>(
                name: "WarrantyExpirationDate",
                table: "Furniture",
                nullable: true,
                oldClrType: typeof(DateTime)
            );

            migrationBuilder.AlterColumn<DateTime>(
                name: "ManufacturingDate",
                table: "Furniture",
                nullable: true,
                oldClrType: typeof(DateTime)
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "ManufacturerId",
                table: "Furniture",
                nullable: true,
                oldClrType: typeof(Guid)
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Furniture_Manufacturer_ManufacturerId",
                table: "Furniture",
                column: "ManufacturerId",
                principalTable: "Manufacturer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Furniture_Manufacturer_ManufacturerId",
                table: "Furniture"
            );

            migrationBuilder.AlterColumn<DateTime>(
                name: "WarrantyExpirationDate",
                table: "Furniture",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldNullable: true
            );

            migrationBuilder.AlterColumn<DateTime>(
                name: "ManufacturingDate",
                table: "Furniture",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldNullable: true
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "ManufacturerId",
                table: "Furniture",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Furniture_Manufacturer_ManufacturerId",
                table: "Furniture",
                column: "ManufacturerId",
                principalTable: "Manufacturer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );
        }
    }
}
