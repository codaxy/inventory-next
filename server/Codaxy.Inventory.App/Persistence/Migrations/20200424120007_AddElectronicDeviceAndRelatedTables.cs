using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    public partial class AddElectronicDeviceAndRelatedTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ElectronicDevice",
                columns: table => new
                {
                    AssetId = table.Column<Guid>(nullable: false),
                    ManufacturerId = table.Column<Guid>(nullable: false),
                    ManufacturingDate = table.Column<DateTime>(nullable: false),
                    GuaranteeExpirationDate = table.Column<DateTime>(nullable: false),
                    GuaranteeNumber = table.Column<string>(nullable: true),
                    SerialNumber = table.Column<string>(nullable: true),
                    Model = table.Column<string>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElectronicDevice", x => x.AssetId);
                    table.ForeignKey(
                        name: "FK_ElectronicDevice_Asset_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Asset",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_ElectronicDevice_Manufacturer_ManufacturerId",
                        column: x => x.ManufacturerId,
                        principalTable: "Manufacturer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "MaintenanceType",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Text = table.Column<string>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceType", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "MaintenanceContract",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    AssetId = table.Column<Guid>(nullable: false),
                    VendorId = table.Column<Guid>(nullable: false),
                    MaintenanceTypeId = table.Column<Guid>(nullable: false),
                    ServiceDueDate = table.Column<DateTime>(nullable: false),
                    ExpirationDate = table.Column<DateTime>(nullable: false),
                    ContactName = table.Column<string>(nullable: true),
                    ContactNumber = table.Column<string>(nullable: true),
                    ContactEmail = table.Column<string>(nullable: true),
                    ContractNumber = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceContract", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaintenanceContract_Asset_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Asset",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_MaintenanceContract_MaintenanceType_MaintenanceTypeId",
                        column: x => x.MaintenanceTypeId,
                        principalTable: "MaintenanceType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_MaintenanceContract_Vendor_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_ElectronicDevice_ManufacturerId",
                table: "ElectronicDevice",
                column: "ManufacturerId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceContract_AssetId",
                table: "MaintenanceContract",
                column: "AssetId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceContract_MaintenanceTypeId",
                table: "MaintenanceContract",
                column: "MaintenanceTypeId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceContract_VendorId",
                table: "MaintenanceContract",
                column: "VendorId"
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ElectronicDevice");

            migrationBuilder.DropTable(name: "MaintenanceContract");

            migrationBuilder.DropTable(name: "MaintenanceType");
        }
    }
}
