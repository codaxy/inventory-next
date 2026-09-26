using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    public partial class DefineAssetLicenseRelationship : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Licenses");

            migrationBuilder.CreateTable(
                name: "License",
                columns: table => new
                {
                    AssetId = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    LicenseClassId = table.Column<Guid>(nullable: false),
                    LicenseCategoryId = table.Column<Guid>(nullable: false),
                    LicenseTypeId = table.Column<Guid>(nullable: false),
                    LicenseExpirationModelId = table.Column<Guid>(nullable: false),
                    ManufacturerId = table.Column<Guid>(nullable: false),
                    VendorId = table.Column<Guid>(nullable: false),
                    PurchaseValue = table.Column<decimal>(nullable: false),
                    PurchaseDate = table.Column<DateTime>(nullable: false),
                    ExpirationDate = table.Column<DateTime>(nullable: true),
                    Available = table.Column<int>(nullable: false),
                    Consumed = table.Column<int>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_License", x => x.AssetId);
                    table.ForeignKey(
                        name: "FK_License_Asset_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Asset",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_License_LicenseCategory_LicenseCategoryId",
                        column: x => x.LicenseCategoryId,
                        principalTable: "LicenseCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_License_LicenseClass_LicenseClassId",
                        column: x => x.LicenseClassId,
                        principalTable: "LicenseClass",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_License_LicenseExpirationModel_LicenseExpirationModelId",
                        column: x => x.LicenseExpirationModelId,
                        principalTable: "LicenseExpirationModel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_License_LicenseType_LicenseTypeId",
                        column: x => x.LicenseTypeId,
                        principalTable: "LicenseType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_License_Manufacturer_ManufacturerId",
                        column: x => x.ManufacturerId,
                        principalTable: "Manufacturer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_License_Vendor_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_License_LicenseCategoryId",
                table: "License",
                column: "LicenseCategoryId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_License_LicenseClassId",
                table: "License",
                column: "LicenseClassId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_License_LicenseExpirationModelId",
                table: "License",
                column: "LicenseExpirationModelId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_License_LicenseTypeId",
                table: "License",
                column: "LicenseTypeId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_License_ManufacturerId",
                table: "License",
                column: "ManufacturerId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_License_VendorId",
                table: "License",
                column: "VendorId"
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "License");

            migrationBuilder.CreateTable(
                name: "Licenses",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    AssetId = table.Column<Guid>(nullable: false),
                    Available = table.Column<int>(nullable: false),
                    Consumed = table.Column<int>(nullable: false),
                    ExpirationDate = table.Column<DateTime>(nullable: false),
                    LicenseCategoryId = table.Column<Guid>(nullable: false),
                    LicenseClassId = table.Column<Guid>(nullable: false),
                    LicenseExpirationModelId = table.Column<Guid>(nullable: false),
                    LicenseTypeId = table.Column<Guid>(nullable: false),
                    ManufacturerId = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    PurchaseDate = table.Column<DateTime>(nullable: false),
                    PurchaseValue = table.Column<decimal>(nullable: false),
                    Status = table.Column<string>(nullable: true),
                    VendorId = table.Column<Guid>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Licenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Licenses_LicenseCategory_LicenseCategoryId",
                        column: x => x.LicenseCategoryId,
                        principalTable: "LicenseCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_Licenses_LicenseClass_LicenseClassId",
                        column: x => x.LicenseClassId,
                        principalTable: "LicenseClass",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_Licenses_LicenseExpirationModel_LicenseExpirationModelId",
                        column: x => x.LicenseExpirationModelId,
                        principalTable: "LicenseExpirationModel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_Licenses_LicenseType_LicenseTypeId",
                        column: x => x.LicenseTypeId,
                        principalTable: "LicenseType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_Licenses_Manufacturer_ManufacturerId",
                        column: x => x.ManufacturerId,
                        principalTable: "Manufacturer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_Licenses_Vendor_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_Licenses_LicenseCategoryId",
                table: "Licenses",
                column: "LicenseCategoryId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Licenses_LicenseClassId",
                table: "Licenses",
                column: "LicenseClassId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Licenses_LicenseExpirationModelId",
                table: "Licenses",
                column: "LicenseExpirationModelId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Licenses_LicenseTypeId",
                table: "Licenses",
                column: "LicenseTypeId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Licenses_ManufacturerId",
                table: "Licenses",
                column: "ManufacturerId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Licenses_VendorId",
                table: "Licenses",
                column: "VendorId"
            );
        }
    }
}
