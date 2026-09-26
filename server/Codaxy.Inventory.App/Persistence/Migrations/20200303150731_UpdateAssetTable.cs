using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    public partial class UpdateAssetTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Asset_BusinessUnit_BusinessUnitId",
                table: "Asset"
            );

            migrationBuilder.DropForeignKey(name: "FK_Asset_Company_CompanyId", table: "Asset");

            migrationBuilder.DropIndex(name: "IX_Asset_BusinessUnitId", table: "Asset");

            migrationBuilder.DropIndex(name: "IX_Asset_CompanyId", table: "Asset");

            migrationBuilder.DropColumn(name: "BusinessUnitId", table: "Asset");

            migrationBuilder.DropColumn(name: "CompanyId", table: "Asset");

            migrationBuilder.AddColumn<string>(
                name: "BusinessEntity",
                table: "Asset",
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "InvoiceNumber",
                table: "Asset",
                nullable: true
            );

            migrationBuilder.AddColumn<DateTime>(
                name: "PurchaseDate",
                table: "Asset",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified)
            );

            migrationBuilder.AddColumn<decimal>(
                name: "PurchaseValue",
                table: "Asset",
                nullable: false,
                defaultValue: 0m
            );

            migrationBuilder.AddColumn<Guid>(
                name: "VendorId",
                table: "Asset",
                nullable: false,
                defaultValue: new Guid("02b8d654-1460-4f13-b9a9-0a846860ea0f")
            );

            migrationBuilder.CreateIndex(
                name: "IX_Asset_VendorId",
                table: "Asset",
                column: "VendorId"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Asset_Vendor_VendorId",
                table: "Asset",
                column: "VendorId",
                principalTable: "Vendor",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_Asset_Vendor_VendorId", table: "Asset");

            migrationBuilder.DropIndex(name: "IX_Asset_VendorId", table: "Asset");

            migrationBuilder.DropColumn(name: "BusinessEntity", table: "Asset");

            migrationBuilder.DropColumn(name: "InvoiceNumber", table: "Asset");

            migrationBuilder.DropColumn(name: "PurchaseDate", table: "Asset");

            migrationBuilder.DropColumn(name: "PurchaseValue", table: "Asset");

            migrationBuilder.DropColumn(name: "VendorId", table: "Asset");

            migrationBuilder.AddColumn<Guid>(
                name: "BusinessUnitId",
                table: "Asset",
                nullable: true
            );

            migrationBuilder.AddColumn<Guid>(name: "CompanyId", table: "Asset", nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Asset_BusinessUnitId",
                table: "Asset",
                column: "BusinessUnitId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Asset_CompanyId",
                table: "Asset",
                column: "CompanyId"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Asset_BusinessUnit_BusinessUnitId",
                table: "Asset",
                column: "BusinessUnitId",
                principalTable: "BusinessUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Asset_Company_CompanyId",
                table: "Asset",
                column: "CompanyId",
                principalTable: "Company",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );
        }
    }
}
