using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    public partial class AddDataStorageAndRelatedTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DataStorageCategory",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataStorageCategory", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "DataStorage",
                columns: table => new
                {
                    AssetId = table.Column<Guid>(nullable: false),
                    DataStorageCategoryId = table.Column<Guid>(nullable: false),
                    ManufacturerId = table.Column<Guid>(nullable: false),
                    VendorId = table.Column<Guid>(nullable: false),
                    PurchaseValue = table.Column<decimal>(nullable: false),
                    PurchaseDate = table.Column<DateTime>(nullable: false),
                    Capacity = table.Column<string>(nullable: true),
                    Encryption = table.Column<bool>(nullable: false),
                    PersonalData = table.Column<bool>(nullable: false),
                    Model = table.Column<string>(nullable: true),
                    SerialNumber = table.Column<string>(nullable: true),
                    ManufacturingDate = table.Column<DateTime>(nullable: false),
                    Warranty = table.Column<string>(nullable: true),
                    WarrantyExpirationDate = table.Column<DateTime>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataStorage", x => x.AssetId);
                    table.ForeignKey(
                        name: "FK_DataStorage_Asset_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Asset",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_DataStorage_DataStorageCategory_DataStorageCategoryId",
                        column: x => x.DataStorageCategoryId,
                        principalTable: "DataStorageCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_DataStorage_Manufacturer_ManufacturerId",
                        column: x => x.ManufacturerId,
                        principalTable: "Manufacturer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_DataStorage_Vendor_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_DataStorage_DataStorageCategoryId",
                table: "DataStorage",
                column: "DataStorageCategoryId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_DataStorage_ManufacturerId",
                table: "DataStorage",
                column: "ManufacturerId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_DataStorage_VendorId",
                table: "DataStorage",
                column: "VendorId"
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "DataStorage");

            migrationBuilder.DropTable(name: "DataStorageCategory");
        }
    }
}
