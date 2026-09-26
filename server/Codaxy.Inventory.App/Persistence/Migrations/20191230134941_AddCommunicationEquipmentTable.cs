using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    public partial class AddCommunicationEquipmentTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CommunicationEquipment",
                columns: table => new
                {
                    AssetId = table.Column<Guid>(nullable: false),
                    CommunicationEquipmentClassId = table.Column<Guid>(nullable: false),
                    CommunicationEquipmentCategoryId = table.Column<Guid>(nullable: false),
                    ManufacturerId = table.Column<Guid>(nullable: false),
                    VendorId = table.Column<Guid>(nullable: false),
                    PurchaseValue = table.Column<decimal>(nullable: false),
                    PurchaseDate = table.Column<DateTime>(nullable: false),
                    Model = table.Column<string>(nullable: true),
                    SerialNumber = table.Column<string>(nullable: true),
                    ManufacturingDate = table.Column<DateTime>(nullable: false),
                    Warranty = table.Column<string>(nullable: true),
                    WarrantyExpirationDate = table.Column<DateTime>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunicationEquipment", x => x.AssetId);
                    table.ForeignKey(
                        name: "FK_CommunicationEquipment_Asset_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Asset",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_CommunicationEquipment_CommunicationEquipmentCategory_Commu~",
                        column: x => x.CommunicationEquipmentCategoryId,
                        principalTable: "CommunicationEquipmentCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_CommunicationEquipment_CommunicationEquipmentClass_Communic~",
                        column: x => x.CommunicationEquipmentClassId,
                        principalTable: "CommunicationEquipmentClass",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_CommunicationEquipment_Manufacturer_ManufacturerId",
                        column: x => x.ManufacturerId,
                        principalTable: "Manufacturer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_CommunicationEquipment_Vendor_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationEquipment_CommunicationEquipmentCategoryId",
                table: "CommunicationEquipment",
                column: "CommunicationEquipmentCategoryId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationEquipment_CommunicationEquipmentClassId",
                table: "CommunicationEquipment",
                column: "CommunicationEquipmentClassId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationEquipment_ManufacturerId",
                table: "CommunicationEquipment",
                column: "ManufacturerId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationEquipment_VendorId",
                table: "CommunicationEquipment",
                column: "VendorId"
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "CommunicationEquipment");
        }
    }
}
