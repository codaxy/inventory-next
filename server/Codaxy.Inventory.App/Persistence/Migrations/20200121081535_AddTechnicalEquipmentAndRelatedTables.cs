using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    public partial class AddTechnicalEquipmentAndRelatedTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ComputerEquipment_Manufacturer_ManufacturerId",
                table: "ComputerEquipment"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_ComputerEquipment_Vendor_VendorId",
                table: "ComputerEquipment"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_MobileDevice_Manufacturer_ManufacturerId",
                table: "MobileDevice"
            );

            migrationBuilder.AlterColumn<DateTime>(
                name: "WarrantyExpirationDate",
                table: "MobileDevice",
                nullable: true,
                oldClrType: typeof(DateTime)
            );

            migrationBuilder.AlterColumn<DateTime>(
                name: "ManufacturingDate",
                table: "MobileDevice",
                nullable: true,
                oldClrType: typeof(DateTime)
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "ManufacturerId",
                table: "MobileDevice",
                nullable: true,
                oldClrType: typeof(Guid)
            );

            migrationBuilder.AlterColumn<DateTime>(
                name: "WarrantyExpirationDate",
                table: "ComputerEquipment",
                nullable: true,
                oldClrType: typeof(DateTime)
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "VendorId",
                table: "ComputerEquipment",
                nullable: true,
                oldClrType: typeof(Guid)
            );

            migrationBuilder.AlterColumn<DateTime>(
                name: "ManufacturingDate",
                table: "ComputerEquipment",
                nullable: true,
                oldClrType: typeof(DateTime)
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "ManufacturerId",
                table: "ComputerEquipment",
                nullable: true,
                oldClrType: typeof(Guid)
            );

            migrationBuilder.CreateTable(
                name: "TechnicalEquipmentCategory",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TechnicalEquipmentCategory", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "TechnicalEquipment",
                columns: table => new
                {
                    AssetId = table.Column<Guid>(nullable: false),
                    TechnicalEquipmentClassId = table.Column<Guid>(nullable: false),
                    ManufacturerId = table.Column<Guid>(nullable: false),
                    VendorId = table.Column<Guid>(nullable: false),
                    PurchaseValue = table.Column<decimal>(nullable: false),
                    PurchaseDate = table.Column<DateTime>(nullable: false),
                    Model = table.Column<string>(nullable: true),
                    SerialNumber = table.Column<string>(nullable: true),
                    ManufacturingDate = table.Column<DateTime>(nullable: false),
                    Warranty = table.Column<string>(nullable: true),
                    WarrantyExpirationDate = table.Column<DateTime>(nullable: false),
                    TechnicalEquipmentCategoryId = table.Column<Guid>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TechnicalEquipment", x => x.AssetId);
                    table.ForeignKey(
                        name: "FK_TechnicalEquipment_Asset_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Asset",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_TechnicalEquipment_Manufacturer_ManufacturerId",
                        column: x => x.ManufacturerId,
                        principalTable: "Manufacturer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_TechnicalEquipment_TechnicalEquipmentCategory_TechnicalEqui~",
                        column: x => x.TechnicalEquipmentCategoryId,
                        principalTable: "TechnicalEquipmentCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_TechnicalEquipment_Vendor_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_TechnicalEquipment_ManufacturerId",
                table: "TechnicalEquipment",
                column: "ManufacturerId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_TechnicalEquipment_TechnicalEquipmentCategoryId",
                table: "TechnicalEquipment",
                column: "TechnicalEquipmentCategoryId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_TechnicalEquipment_VendorId",
                table: "TechnicalEquipment",
                column: "VendorId"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_ComputerEquipment_Manufacturer_ManufacturerId",
                table: "ComputerEquipment",
                column: "ManufacturerId",
                principalTable: "Manufacturer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.AddForeignKey(
                name: "FK_ComputerEquipment_Vendor_VendorId",
                table: "ComputerEquipment",
                column: "VendorId",
                principalTable: "Vendor",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.AddForeignKey(
                name: "FK_MobileDevice_Manufacturer_ManufacturerId",
                table: "MobileDevice",
                column: "ManufacturerId",
                principalTable: "Manufacturer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ComputerEquipment_Manufacturer_ManufacturerId",
                table: "ComputerEquipment"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_ComputerEquipment_Vendor_VendorId",
                table: "ComputerEquipment"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_MobileDevice_Manufacturer_ManufacturerId",
                table: "MobileDevice"
            );

            migrationBuilder.DropTable(name: "TechnicalEquipment");

            migrationBuilder.DropTable(name: "TechnicalEquipmentCategory");

            migrationBuilder.AlterColumn<DateTime>(
                name: "WarrantyExpirationDate",
                table: "MobileDevice",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldNullable: true
            );

            migrationBuilder.AlterColumn<DateTime>(
                name: "ManufacturingDate",
                table: "MobileDevice",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldNullable: true
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "ManufacturerId",
                table: "MobileDevice",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true
            );

            migrationBuilder.AlterColumn<DateTime>(
                name: "WarrantyExpirationDate",
                table: "ComputerEquipment",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldNullable: true
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "VendorId",
                table: "ComputerEquipment",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true
            );

            migrationBuilder.AlterColumn<DateTime>(
                name: "ManufacturingDate",
                table: "ComputerEquipment",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldNullable: true
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "ManufacturerId",
                table: "ComputerEquipment",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true
            );

            migrationBuilder.AddForeignKey(
                name: "FK_ComputerEquipment_Manufacturer_ManufacturerId",
                table: "ComputerEquipment",
                column: "ManufacturerId",
                principalTable: "Manufacturer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_ComputerEquipment_Vendor_VendorId",
                table: "ComputerEquipment",
                column: "VendorId",
                principalTable: "Vendor",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_MobileDevice_Manufacturer_ManufacturerId",
                table: "MobileDevice",
                column: "ManufacturerId",
                principalTable: "Manufacturer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );
        }
    }
}
