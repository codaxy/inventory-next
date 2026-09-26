using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    public partial class AddAssetAndRelatedTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AssetCategory",
                keyColumn: "Id",
                keyValue: new Guid("20e2cb55-bb14-4fb9-9455-12ef169d24e6")
            );

            migrationBuilder.DeleteData(
                table: "AssetCategory",
                keyColumn: "Id",
                keyValue: new Guid("5653b18d-2036-4f2a-a54d-1efa8db6ec85")
            );

            migrationBuilder.DeleteData(
                table: "AssetCategory",
                keyColumn: "Id",
                keyValue: new Guid("5fb2eccb-c940-48f9-aac6-0a204cdce42e")
            );

            migrationBuilder.DeleteData(
                table: "AssetCategory",
                keyColumn: "Id",
                keyValue: new Guid("6c993228-80e4-45eb-baf6-877310986aca")
            );

            migrationBuilder.DeleteData(
                table: "AssetCategory",
                keyColumn: "Id",
                keyValue: new Guid("a9871483-5d51-4134-9a38-425de55acdba")
            );

            migrationBuilder.DeleteData(
                table: "AssetCategory",
                keyColumn: "Id",
                keyValue: new Guid("b99d0b01-d36e-4b43-ad52-89fbf05b26f3")
            );

            migrationBuilder.DeleteData(
                table: "AssetCategory",
                keyColumn: "Id",
                keyValue: new Guid("e1bbabd5-5bc8-424b-bf14-687c912e1d5b")
            );

            migrationBuilder.CreateTable(
                name: "Availability",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Level = table.Column<string>(nullable: true),
                    Weight = table.Column<int>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Availability", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "Confidentiality",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Level = table.Column<string>(nullable: true),
                    Weight = table.Column<int>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Confidentiality", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "Importance",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Level = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Importance", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "Integrity",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Level = table.Column<string>(nullable: true),
                    Weight = table.Column<int>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Integrity", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "Location",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Country = table.Column<string>(nullable: true),
                    City = table.Column<string>(nullable: true),
                    PostalCode = table.Column<string>(nullable: true),
                    Address = table.Column<string>(nullable: true),
                    Floor = table.Column<string>(nullable: true),
                    Room = table.Column<string>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Location", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "Asset",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    AssetTypeId = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    AssetSubstatusId = table.Column<Guid>(nullable: false),
                    URL = table.Column<string>(nullable: true),
                    LocationId = table.Column<Guid>(nullable: false),
                    ConfidentialityId = table.Column<Guid>(nullable: false),
                    IntegrityId = table.Column<Guid>(nullable: false),
                    AvailabilityId = table.Column<Guid>(nullable: false),
                    ImportanceId = table.Column<Guid>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asset", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Asset_AssetSubstatus_AssetSubstatusId",
                        column: x => x.AssetSubstatusId,
                        principalTable: "AssetSubstatus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_Asset_AssetType_AssetTypeId",
                        column: x => x.AssetTypeId,
                        principalTable: "AssetType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_Asset_Availability_AvailabilityId",
                        column: x => x.AvailabilityId,
                        principalTable: "Availability",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_Asset_Confidentiality_ConfidentialityId",
                        column: x => x.ConfidentialityId,
                        principalTable: "Confidentiality",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_Asset_Importance_ImportanceId",
                        column: x => x.ImportanceId,
                        principalTable: "Importance",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_Asset_Integrity_IntegrityId",
                        column: x => x.IntegrityId,
                        principalTable: "Integrity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_Asset_Location_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Location",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_Asset_AssetSubstatusId",
                table: "Asset",
                column: "AssetSubstatusId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Asset_AssetTypeId",
                table: "Asset",
                column: "AssetTypeId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Asset_AvailabilityId",
                table: "Asset",
                column: "AvailabilityId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Asset_ConfidentialityId",
                table: "Asset",
                column: "ConfidentialityId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Asset_ImportanceId",
                table: "Asset",
                column: "ImportanceId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Asset_IntegrityId",
                table: "Asset",
                column: "IntegrityId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Asset_LocationId",
                table: "Asset",
                column: "LocationId"
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Asset");

            migrationBuilder.DropTable(name: "Availability");

            migrationBuilder.DropTable(name: "Confidentiality");

            migrationBuilder.DropTable(name: "Importance");

            migrationBuilder.DropTable(name: "Integrity");

            migrationBuilder.DropTable(name: "Location");

            migrationBuilder.InsertData(
                table: "AssetCategory",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("5fb2eccb-c940-48f9-aac6-0a204cdce42e"), "Entities" },
                    { new Guid("a9871483-5d51-4134-9a38-425de55acdba"), "Information" },
                    { new Guid("20e2cb55-bb14-4fb9-9455-12ef169d24e6"), "Software" },
                    { new Guid("e1bbabd5-5bc8-424b-bf14-687c912e1d5b"), "Equipment" },
                    { new Guid("5653b18d-2036-4f2a-a54d-1efa8db6ec85"), "Services" },
                    { new Guid("6c993228-80e4-45eb-baf6-877310986aca"), "Premises" },
                    { new Guid("b99d0b01-d36e-4b43-ad52-89fbf05b26f3"), "Transportation" },
                }
            );
        }
    }
}
