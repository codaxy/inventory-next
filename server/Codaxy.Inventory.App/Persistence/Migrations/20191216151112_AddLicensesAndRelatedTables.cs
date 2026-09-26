using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    public partial class AddLicensesAndRelatedTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LicenseCategory",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LicenseCategory", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "LicenseClass",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LicenseClass", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "LicenseExpirationModel",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LicenseExpirationModel", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "LicenseType",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LicenseType", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "Manufacturer",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    URL = table.Column<string>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Manufacturer", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "Vendor",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    LocationId = table.Column<Guid>(nullable: false),
                    RegistrationNumber = table.Column<string>(nullable: true),
                    VATNumber = table.Column<decimal>(nullable: false),
                    Web = table.Column<string>(nullable: true),
                    ContactPerson = table.Column<string>(nullable: true),
                    MobilePhone = table.Column<string>(nullable: true),
                    Phone = table.Column<string>(nullable: true),
                    Email = table.Column<string>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vendor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vendor_Location_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Location",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "Licenses",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
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
                    ExpirationDate = table.Column<DateTime>(nullable: false),
                    Available = table.Column<int>(nullable: false),
                    Consumed = table.Column<int>(nullable: false),
                    Status = table.Column<string>(nullable: true),
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

            migrationBuilder.CreateIndex(
                name: "IX_Vendor_LocationId",
                table: "Vendor",
                column: "LocationId"
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Licenses");

            migrationBuilder.DropTable(name: "LicenseCategory");

            migrationBuilder.DropTable(name: "LicenseClass");

            migrationBuilder.DropTable(name: "LicenseExpirationModel");

            migrationBuilder.DropTable(name: "LicenseType");

            migrationBuilder.DropTable(name: "Manufacturer");

            migrationBuilder.DropTable(name: "Vendor");
        }
    }
}
