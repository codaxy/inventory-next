using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    public partial class AddVolumeActivationTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_License_LicenseCategory_LicenseCategoryId",
                table: "License"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_License_LicenseClass_LicenseClassId",
                table: "License"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_License_Manufacturer_ManufacturerId",
                table: "License"
            );

            migrationBuilder.DropForeignKey(name: "FK_License_Vendor_VendorId", table: "License");

            migrationBuilder.DropIndex(name: "IX_License_LicenseCategoryId", table: "License");

            migrationBuilder.DropIndex(name: "IX_License_LicenseClassId", table: "License");

            migrationBuilder.DropIndex(name: "IX_License_ManufacturerId", table: "License");

            migrationBuilder.DropIndex(name: "IX_License_VendorId", table: "License");

            migrationBuilder.DropColumn(name: "Available", table: "License");

            migrationBuilder.DropColumn(name: "Consumed", table: "License");

            migrationBuilder.DropColumn(name: "ExpirationDate", table: "License");

            migrationBuilder.DropColumn(name: "LicenseCategoryId", table: "License");

            migrationBuilder.DropColumn(name: "LicenseClassId", table: "License");

            migrationBuilder.DropColumn(name: "ManufacturerId", table: "License");

            migrationBuilder.DropColumn(name: "VendorId", table: "License");

            migrationBuilder.RenameColumn(
                name: "PurchaseValue",
                table: "License",
                newName: "SubscriptionFee"
            );

            migrationBuilder.RenameColumn(
                name: "PurchaseDate",
                table: "License",
                newName: "SubscriptionExpirationDate"
            );

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "License",
                newName: "RegistrationNumber"
            );

            migrationBuilder.AddColumn<bool>(
                name: "AutoRenew",
                table: "License",
                nullable: false,
                defaultValue: false
            );

            migrationBuilder.AddColumn<string>(
                name: "ManagementConsoleUrl",
                table: "License",
                nullable: true
            );

            migrationBuilder.CreateTable(
                name: "LicenseModel",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LicenseModel", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "VolumeType",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VolumeType", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "Volume",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    SoftwareOrServiceId = table.Column<Guid>(nullable: false),
                    VolumeTypeId = table.Column<Guid>(nullable: false),
                    LicenseId = table.Column<Guid>(nullable: false),
                    Quantity = table.Column<int>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Volume", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Volume_License_LicenseId",
                        column: x => x.LicenseId,
                        principalTable: "License",
                        principalColumn: "AssetId",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_Volume_SoftwareOrService_SoftwareOrServiceId",
                        column: x => x.SoftwareOrServiceId,
                        principalTable: "SoftwareOrService",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_Volume_VolumeType_VolumeTypeId",
                        column: x => x.VolumeTypeId,
                        principalTable: "VolumeType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "Activation",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    PersonId = table.Column<Guid>(nullable: false),
                    AssetId = table.Column<Guid>(nullable: false),
                    VolumeId = table.Column<Guid>(nullable: false),
                    Quantity = table.Column<int>(nullable: false),
                    ActivationDate = table.Column<DateTime>(nullable: false),
                    DeactivationDate = table.Column<DateTime>(nullable: true),
                    KeyIdentifier = table.Column<Guid>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Activation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Activation_Asset_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Asset",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_Activation_Person_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Person",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_Activation_Volume_VolumeId",
                        column: x => x.VolumeId,
                        principalTable: "Volume",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_Activation_AssetId",
                table: "Activation",
                column: "AssetId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Activation_PersonId",
                table: "Activation",
                column: "PersonId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Activation_VolumeId",
                table: "Activation",
                column: "VolumeId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Volume_LicenseId",
                table: "Volume",
                column: "LicenseId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Volume_SoftwareOrServiceId",
                table: "Volume",
                column: "SoftwareOrServiceId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Volume_VolumeTypeId",
                table: "Volume",
                column: "VolumeTypeId"
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Activation");

            migrationBuilder.DropTable(name: "LicenseModel");

            migrationBuilder.DropTable(name: "Volume");

            migrationBuilder.DropTable(name: "VolumeType");

            migrationBuilder.DropColumn(name: "AutoRenew", table: "License");

            migrationBuilder.DropColumn(name: "ManagementConsoleUrl", table: "License");

            migrationBuilder.RenameColumn(
                name: "SubscriptionFee",
                table: "License",
                newName: "PurchaseValue"
            );

            migrationBuilder.RenameColumn(
                name: "SubscriptionExpirationDate",
                table: "License",
                newName: "PurchaseDate"
            );

            migrationBuilder.RenameColumn(
                name: "RegistrationNumber",
                table: "License",
                newName: "Name"
            );

            migrationBuilder.AddColumn<int>(name: "Available", table: "License", nullable: true);

            migrationBuilder.AddColumn<int>(name: "Consumed", table: "License", nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpirationDate",
                table: "License",
                nullable: true
            );

            migrationBuilder.AddColumn<Guid>(
                name: "LicenseCategoryId",
                table: "License",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000")
            );

            migrationBuilder.AddColumn<Guid>(
                name: "LicenseClassId",
                table: "License",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000")
            );

            migrationBuilder.AddColumn<Guid>(
                name: "ManufacturerId",
                table: "License",
                nullable: true
            );

            migrationBuilder.AddColumn<Guid>(name: "VendorId", table: "License", nullable: true);

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
                name: "IX_License_ManufacturerId",
                table: "License",
                column: "ManufacturerId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_License_VendorId",
                table: "License",
                column: "VendorId"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_License_LicenseCategory_LicenseCategoryId",
                table: "License",
                column: "LicenseCategoryId",
                principalTable: "LicenseCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_License_LicenseClass_LicenseClassId",
                table: "License",
                column: "LicenseClassId",
                principalTable: "LicenseClass",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_License_Manufacturer_ManufacturerId",
                table: "License",
                column: "ManufacturerId",
                principalTable: "Manufacturer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.AddForeignKey(
                name: "FK_License_Vendor_VendorId",
                table: "License",
                column: "VendorId",
                principalTable: "Vendor",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );
        }
    }
}
