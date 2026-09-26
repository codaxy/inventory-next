using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    public partial class UpdateAssetAndLicenseColumnToNullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Asset_AssetSubstatus_AssetSubstatusId",
                table: "Asset"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_Asset_BusinessEntity_BusinessEntityId",
                table: "Asset"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_License_LicenseExpirationModel_LicenseExpirationModelId",
                table: "License"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_License_LicenseModel_LicenseModelId",
                table: "License"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_License_LicenseType_LicenseTypeId",
                table: "License"
            );

            migrationBuilder.AlterColumn<decimal>(
                name: "SubscriptionFee",
                table: "License",
                nullable: true,
                oldClrType: typeof(decimal)
            );

            migrationBuilder.AlterColumn<DateTime>(
                name: "SubscriptionExpirationDate",
                table: "License",
                nullable: true,
                oldClrType: typeof(DateTime)
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "LicenseTypeId",
                table: "License",
                nullable: true,
                oldClrType: typeof(Guid)
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "LicenseModelId",
                table: "License",
                nullable: true,
                oldClrType: typeof(Guid)
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "LicenseExpirationModelId",
                table: "License",
                nullable: true,
                oldClrType: typeof(Guid)
            );

            migrationBuilder.AlterColumn<bool>(
                name: "AutoRenew",
                table: "License",
                nullable: true,
                oldClrType: typeof(bool)
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "BusinessEntityId",
                table: "Asset",
                nullable: true,
                oldClrType: typeof(Guid)
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "AssetSubstatusId",
                table: "Asset",
                nullable: true,
                oldClrType: typeof(Guid)
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Asset_AssetSubstatus_AssetSubstatusId",
                table: "Asset",
                column: "AssetSubstatusId",
                principalTable: "AssetSubstatus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Asset_BusinessEntity_BusinessEntityId",
                table: "Asset",
                column: "BusinessEntityId",
                principalTable: "BusinessEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.AddForeignKey(
                name: "FK_License_LicenseExpirationModel_LicenseExpirationModelId",
                table: "License",
                column: "LicenseExpirationModelId",
                principalTable: "LicenseExpirationModel",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.AddForeignKey(
                name: "FK_License_LicenseModel_LicenseModelId",
                table: "License",
                column: "LicenseModelId",
                principalTable: "LicenseModel",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.AddForeignKey(
                name: "FK_License_LicenseType_LicenseTypeId",
                table: "License",
                column: "LicenseTypeId",
                principalTable: "LicenseType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Asset_AssetSubstatus_AssetSubstatusId",
                table: "Asset"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_Asset_BusinessEntity_BusinessEntityId",
                table: "Asset"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_License_LicenseExpirationModel_LicenseExpirationModelId",
                table: "License"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_License_LicenseModel_LicenseModelId",
                table: "License"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_License_LicenseType_LicenseTypeId",
                table: "License"
            );

            migrationBuilder.AlterColumn<decimal>(
                name: "SubscriptionFee",
                table: "License",
                nullable: false,
                oldClrType: typeof(decimal),
                oldNullable: true
            );

            migrationBuilder.AlterColumn<DateTime>(
                name: "SubscriptionExpirationDate",
                table: "License",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldNullable: true
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "LicenseTypeId",
                table: "License",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "LicenseModelId",
                table: "License",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "LicenseExpirationModelId",
                table: "License",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true
            );

            migrationBuilder.AlterColumn<bool>(
                name: "AutoRenew",
                table: "License",
                nullable: false,
                oldClrType: typeof(bool),
                oldNullable: true
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "BusinessEntityId",
                table: "Asset",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "AssetSubstatusId",
                table: "Asset",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Asset_AssetSubstatus_AssetSubstatusId",
                table: "Asset",
                column: "AssetSubstatusId",
                principalTable: "AssetSubstatus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Asset_BusinessEntity_BusinessEntityId",
                table: "Asset",
                column: "BusinessEntityId",
                principalTable: "BusinessEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_License_LicenseExpirationModel_LicenseExpirationModelId",
                table: "License",
                column: "LicenseExpirationModelId",
                principalTable: "LicenseExpirationModel",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_License_LicenseModel_LicenseModelId",
                table: "License",
                column: "LicenseModelId",
                principalTable: "LicenseModel",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_License_LicenseType_LicenseTypeId",
                table: "License",
                column: "LicenseTypeId",
                principalTable: "LicenseType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );
        }
    }
}
