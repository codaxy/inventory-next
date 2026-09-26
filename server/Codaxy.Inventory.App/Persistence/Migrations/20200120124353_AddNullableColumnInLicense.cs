using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    public partial class AddNullableColumnInLicense : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_License_Manufacturer_ManufacturerId",
                table: "License"
            );

            migrationBuilder.DropForeignKey(name: "FK_License_Vendor_VendorId", table: "License");

            migrationBuilder.AlterColumn<Guid>(
                name: "VendorId",
                table: "License",
                nullable: true,
                oldClrType: typeof(Guid)
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "ManufacturerId",
                table: "License",
                nullable: true,
                oldClrType: typeof(Guid)
            );

            migrationBuilder.AlterColumn<int>(
                name: "Consumed",
                table: "License",
                nullable: true,
                oldClrType: typeof(int)
            );

            migrationBuilder.AlterColumn<int>(
                name: "Available",
                table: "License",
                nullable: true,
                oldClrType: typeof(int)
            );

            migrationBuilder.AddForeignKey(
                name: "FK_License_Manufacturer_ManufacturerId",
                table: "License",
                column: "ManufacturerId",
                principalTable: "Manufacturer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.AddForeignKey(
                name: "FK_License_Vendor_VendorId",
                table: "License",
                column: "VendorId",
                principalTable: "Vendor",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_License_Manufacturer_ManufacturerId",
                table: "License"
            );

            migrationBuilder.DropForeignKey(name: "FK_License_Vendor_VendorId", table: "License");

            migrationBuilder.AlterColumn<Guid>(
                name: "VendorId",
                table: "License",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "ManufacturerId",
                table: "License",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true
            );

            migrationBuilder.AlterColumn<int>(
                name: "Consumed",
                table: "License",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true
            );

            migrationBuilder.AlterColumn<int>(
                name: "Available",
                table: "License",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true
            );

            migrationBuilder.AddForeignKey(
                name: "FK_License_Manufacturer_ManufacturerId",
                table: "License",
                column: "ManufacturerId",
                principalTable: "Manufacturer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_License_Vendor_VendorId",
                table: "License",
                column: "VendorId",
                principalTable: "Vendor",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );
        }
    }
}
