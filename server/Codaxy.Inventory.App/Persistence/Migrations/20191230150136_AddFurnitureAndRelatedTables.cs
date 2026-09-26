using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    public partial class AddFurnitureAndRelatedTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FurnitureCategory",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FurnitureCategory", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "FurnitureClass",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FurnitureClass", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "Furniture",
                columns: table => new
                {
                    AssetId = table.Column<Guid>(nullable: false),
                    FurnitureClassId = table.Column<Guid>(nullable: false),
                    FurnitureCategoryId = table.Column<Guid>(nullable: false),
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
                    table.PrimaryKey("PK_Furniture", x => x.AssetId);
                    table.ForeignKey(
                        name: "FK_Furniture_Asset_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Asset",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_Furniture_FurnitureCategory_FurnitureCategoryId",
                        column: x => x.FurnitureCategoryId,
                        principalTable: "FurnitureCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_Furniture_FurnitureClass_FurnitureClassId",
                        column: x => x.FurnitureClassId,
                        principalTable: "FurnitureClass",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_Furniture_Manufacturer_ManufacturerId",
                        column: x => x.ManufacturerId,
                        principalTable: "Manufacturer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_Furniture_Vendor_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_Furniture_FurnitureCategoryId",
                table: "Furniture",
                column: "FurnitureCategoryId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Furniture_FurnitureClassId",
                table: "Furniture",
                column: "FurnitureClassId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Furniture_ManufacturerId",
                table: "Furniture",
                column: "ManufacturerId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Furniture_VendorId",
                table: "Furniture",
                column: "VendorId"
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Furniture");

            migrationBuilder.DropTable(name: "FurnitureCategory");

            migrationBuilder.DropTable(name: "FurnitureClass");
        }
    }
}
