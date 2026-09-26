using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    public partial class AddInformationAndRelatedTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Furniture_FurnitureCategory_FurnitureCategoryId",
                table: "Furniture"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_Furniture_FurnitureClass_FurnitureClassId",
                table: "Furniture"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_Furniture_Manufacturer_ManufacturerId",
                table: "Furniture"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_Furniture_Vendor_VendorId",
                table: "Furniture"
            );

            migrationBuilder.DropIndex(
                name: "IX_Furniture_FurnitureCategoryId",
                table: "Furniture"
            );

            migrationBuilder.DropIndex(name: "IX_Furniture_FurnitureClassId", table: "Furniture");

            migrationBuilder.DropIndex(name: "IX_Furniture_VendorId", table: "Furniture");

            migrationBuilder.DropColumn(name: "FurnitureCategoryId", table: "Furniture");

            migrationBuilder.DropColumn(name: "FurnitureClassId", table: "Furniture");

            migrationBuilder.DropColumn(name: "ManufacturingDate", table: "Furniture");

            migrationBuilder.DropColumn(name: "PurchaseDate", table: "Furniture");

            migrationBuilder.DropColumn(name: "PurchaseValue", table: "Furniture");

            migrationBuilder.DropColumn(name: "SerialNumber", table: "Furniture");

            migrationBuilder.DropColumn(name: "VendorId", table: "Furniture");

            migrationBuilder.DropColumn(name: "Warranty", table: "Furniture");

            migrationBuilder.DropColumn(name: "WarrantyExpirationDate", table: "Furniture");

            migrationBuilder.RenameColumn(
                name: "ManufacturerId",
                table: "Furniture",
                newName: "FurnitureTypeId"
            );

            migrationBuilder.RenameIndex(
                name: "IX_Furniture_ManufacturerId",
                table: "Furniture",
                newName: "IX_Furniture_FurnitureTypeId"
            );

            migrationBuilder.CreateTable(
                name: "Client",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Client", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "FurnitureType",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FurnitureType", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "InformationType",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InformationType", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "Project",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ProjectOwnerId = table.Column<Guid>(nullable: false),
                    ClientId = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Project", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Project_Client_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Client",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_Project_Person_ProjectOwnerId",
                        column: x => x.ProjectOwnerId,
                        principalTable: "Person",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "Information",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    InformationTypeId = table.Column<Guid>(nullable: false),
                    PersonId = table.Column<Guid>(nullable: false),
                    ConfidentialityId = table.Column<Guid>(nullable: true),
                    IntegrityId = table.Column<Guid>(nullable: true),
                    AvailabilityId = table.Column<Guid>(nullable: true),
                    ImportanceId = table.Column<Guid>(nullable: true),
                    ProjectId = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Note = table.Column<string>(nullable: true),
                    Author = table.Column<string>(nullable: true),
                    AccessRights = table.Column<string>(nullable: true),
                    PersonalInformation = table.Column<bool>(nullable: false),
                    ClientsPersonalInformation = table.Column<bool>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Information", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Information_Availability_AvailabilityId",
                        column: x => x.AvailabilityId,
                        principalTable: "Availability",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_Information_Confidentiality_ConfidentialityId",
                        column: x => x.ConfidentialityId,
                        principalTable: "Confidentiality",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_Information_Importance_ImportanceId",
                        column: x => x.ImportanceId,
                        principalTable: "Importance",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_Information_InformationType_InformationTypeId",
                        column: x => x.InformationTypeId,
                        principalTable: "InformationType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_Information_Integrity_IntegrityId",
                        column: x => x.IntegrityId,
                        principalTable: "Integrity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_Information_Person_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Person",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_Information_Project_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Project",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_Information_AvailabilityId",
                table: "Information",
                column: "AvailabilityId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Information_ConfidentialityId",
                table: "Information",
                column: "ConfidentialityId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Information_ImportanceId",
                table: "Information",
                column: "ImportanceId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Information_InformationTypeId",
                table: "Information",
                column: "InformationTypeId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Information_IntegrityId",
                table: "Information",
                column: "IntegrityId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Information_PersonId",
                table: "Information",
                column: "PersonId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Information_ProjectId",
                table: "Information",
                column: "ProjectId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Project_ClientId",
                table: "Project",
                column: "ClientId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Project_ProjectOwnerId",
                table: "Project",
                column: "ProjectOwnerId"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Furniture_FurnitureType_FurnitureTypeId",
                table: "Furniture",
                column: "FurnitureTypeId",
                principalTable: "FurnitureType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Furniture_FurnitureType_FurnitureTypeId",
                table: "Furniture"
            );

            migrationBuilder.DropTable(name: "FurnitureType");

            migrationBuilder.DropTable(name: "Information");

            migrationBuilder.DropTable(name: "InformationType");

            migrationBuilder.DropTable(name: "Project");

            migrationBuilder.DropTable(name: "Client");

            migrationBuilder.RenameColumn(
                name: "FurnitureTypeId",
                table: "Furniture",
                newName: "ManufacturerId"
            );

            migrationBuilder.RenameIndex(
                name: "IX_Furniture_FurnitureTypeId",
                table: "Furniture",
                newName: "IX_Furniture_ManufacturerId"
            );

            migrationBuilder.AddColumn<Guid>(
                name: "FurnitureCategoryId",
                table: "Furniture",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000")
            );

            migrationBuilder.AddColumn<Guid>(
                name: "FurnitureClassId",
                table: "Furniture",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000")
            );

            migrationBuilder.AddColumn<DateTime>(
                name: "ManufacturingDate",
                table: "Furniture",
                nullable: true
            );

            migrationBuilder.AddColumn<DateTime>(
                name: "PurchaseDate",
                table: "Furniture",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified)
            );

            migrationBuilder.AddColumn<decimal>(
                name: "PurchaseValue",
                table: "Furniture",
                nullable: false,
                defaultValue: 0m
            );

            migrationBuilder.AddColumn<string>(
                name: "SerialNumber",
                table: "Furniture",
                nullable: true
            );

            migrationBuilder.AddColumn<Guid>(
                name: "VendorId",
                table: "Furniture",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000")
            );

            migrationBuilder.AddColumn<string>(
                name: "Warranty",
                table: "Furniture",
                nullable: true
            );

            migrationBuilder.AddColumn<DateTime>(
                name: "WarrantyExpirationDate",
                table: "Furniture",
                nullable: true
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
                name: "IX_Furniture_VendorId",
                table: "Furniture",
                column: "VendorId"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Furniture_FurnitureCategory_FurnitureCategoryId",
                table: "Furniture",
                column: "FurnitureCategoryId",
                principalTable: "FurnitureCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Furniture_FurnitureClass_FurnitureClassId",
                table: "Furniture",
                column: "FurnitureClassId",
                principalTable: "FurnitureClass",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Furniture_Manufacturer_ManufacturerId",
                table: "Furniture",
                column: "ManufacturerId",
                principalTable: "Manufacturer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Furniture_Vendor_VendorId",
                table: "Furniture",
                column: "VendorId",
                principalTable: "Vendor",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );
        }
    }
}
