using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    public partial class AddComputerEquipmentAndRelatedTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ComputerEquipmentCategory",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComputerEquipmentCategory", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "ComputerEquipment",
                columns: table => new
                {
                    AssetId = table.Column<Guid>(nullable: false),
                    ComputerEquipmentCategoryId = table.Column<Guid>(nullable: false),
                    ManufacturerId = table.Column<Guid>(nullable: false),
                    VendorId = table.Column<Guid>(nullable: false),
                    PurchaseValue = table.Column<decimal>(nullable: false),
                    PurchaseDate = table.Column<DateTime>(nullable: false),
                    Model = table.Column<string>(maxLength: 50, nullable: true),
                    SerialNumber = table.Column<string>(nullable: true),
                    ManufacturingDate = table.Column<DateTime>(nullable: false),
                    Warranty = table.Column<string>(nullable: true),
                    WarrantyExpirationDate = table.Column<DateTime>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComputerEquipment", x => x.AssetId);
                    table.ForeignKey(
                        name: "FK_ComputerEquipment_Asset_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Asset",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_ComputerEquipment_ComputerEquipmentCategory_ComputerEquipme~",
                        column: x => x.ComputerEquipmentCategoryId,
                        principalTable: "ComputerEquipmentCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_ComputerEquipment_Manufacturer_ManufacturerId",
                        column: x => x.ManufacturerId,
                        principalTable: "Manufacturer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_ComputerEquipment_Vendor_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_ComputerEquipment_ComputerEquipmentCategoryId",
                table: "ComputerEquipment",
                column: "ComputerEquipmentCategoryId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_ComputerEquipment_ManufacturerId",
                table: "ComputerEquipment",
                column: "ManufacturerId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_ComputerEquipment_VendorId",
                table: "ComputerEquipment",
                column: "VendorId"
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ComputerEquipment");

            migrationBuilder.DropTable(name: "ComputerEquipmentCategory");
        }
    }
}
