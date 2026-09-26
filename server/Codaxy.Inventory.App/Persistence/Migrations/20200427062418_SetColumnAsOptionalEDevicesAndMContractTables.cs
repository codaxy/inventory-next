using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    public partial class SetColumnAsOptionalEDevicesAndMContractTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ElectronicDevice_Manufacturer_ManufacturerId",
                table: "ElectronicDevice"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceContract_Asset_AssetId",
                table: "MaintenanceContract"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceContract_MaintenanceType_MaintenanceTypeId",
                table: "MaintenanceContract"
            );

            migrationBuilder.AlterColumn<DateTime>(
                name: "ServiceDueDate",
                table: "MaintenanceContract",
                nullable: true,
                oldClrType: typeof(DateTime)
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "MaintenanceTypeId",
                table: "MaintenanceContract",
                nullable: true,
                oldClrType: typeof(Guid)
            );

            migrationBuilder.AlterColumn<DateTime>(
                name: "ExpirationDate",
                table: "MaintenanceContract",
                nullable: true,
                oldClrType: typeof(DateTime)
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "AssetId",
                table: "MaintenanceContract",
                nullable: true,
                oldClrType: typeof(Guid)
            );

            migrationBuilder.AlterColumn<DateTime>(
                name: "ManufacturingDate",
                table: "ElectronicDevice",
                nullable: true,
                oldClrType: typeof(DateTime)
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "ManufacturerId",
                table: "ElectronicDevice",
                nullable: true,
                oldClrType: typeof(Guid)
            );

            migrationBuilder.AlterColumn<DateTime>(
                name: "GuaranteeExpirationDate",
                table: "ElectronicDevice",
                nullable: true,
                oldClrType: typeof(DateTime)
            );

            migrationBuilder.AddForeignKey(
                name: "FK_ElectronicDevice_Manufacturer_ManufacturerId",
                table: "ElectronicDevice",
                column: "ManufacturerId",
                principalTable: "Manufacturer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceContract_Asset_AssetId",
                table: "MaintenanceContract",
                column: "AssetId",
                principalTable: "Asset",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceContract_MaintenanceType_MaintenanceTypeId",
                table: "MaintenanceContract",
                column: "MaintenanceTypeId",
                principalTable: "MaintenanceType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ElectronicDevice_Manufacturer_ManufacturerId",
                table: "ElectronicDevice"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceContract_Asset_AssetId",
                table: "MaintenanceContract"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceContract_MaintenanceType_MaintenanceTypeId",
                table: "MaintenanceContract"
            );

            migrationBuilder.AlterColumn<DateTime>(
                name: "ServiceDueDate",
                table: "MaintenanceContract",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldNullable: true
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "MaintenanceTypeId",
                table: "MaintenanceContract",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true
            );

            migrationBuilder.AlterColumn<DateTime>(
                name: "ExpirationDate",
                table: "MaintenanceContract",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldNullable: true
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "AssetId",
                table: "MaintenanceContract",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true
            );

            migrationBuilder.AlterColumn<DateTime>(
                name: "ManufacturingDate",
                table: "ElectronicDevice",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldNullable: true
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "ManufacturerId",
                table: "ElectronicDevice",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true
            );

            migrationBuilder.AlterColumn<DateTime>(
                name: "GuaranteeExpirationDate",
                table: "ElectronicDevice",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldNullable: true
            );

            migrationBuilder.AddForeignKey(
                name: "FK_ElectronicDevice_Manufacturer_ManufacturerId",
                table: "ElectronicDevice",
                column: "ManufacturerId",
                principalTable: "Manufacturer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceContract_Asset_AssetId",
                table: "MaintenanceContract",
                column: "AssetId",
                principalTable: "Asset",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceContract_MaintenanceType_MaintenanceTypeId",
                table: "MaintenanceContract",
                column: "MaintenanceTypeId",
                principalTable: "MaintenanceType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );
        }
    }
}
