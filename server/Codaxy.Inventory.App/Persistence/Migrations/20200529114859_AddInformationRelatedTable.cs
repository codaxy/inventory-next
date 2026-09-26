using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    public partial class AddInformationRelatedTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Status",
                table: "Information",
                nullable: false,
                defaultValue: false
            );

            migrationBuilder.AddColumn<bool>(
                name: "Status",
                table: "Asset",
                nullable: false,
                defaultValue: false
            );

            migrationBuilder.CreateTable(
                name: "Cloud",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    VolumeId = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    ManagementURL = table.Column<string>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cloud", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cloud_Volume_VolumeId",
                        column: x => x.VolumeId,
                        principalTable: "Volume",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "InformationTag",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InformationTag", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "Software",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    VolumeId = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Software", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Software_Volume_VolumeId",
                        column: x => x.VolumeId,
                        principalTable: "Volume",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "VirtualMachine",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    IPAddress = table.Column<string>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VirtualMachine", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "InformationTagInformation",
                columns: table => new
                {
                    InformationTagId = table.Column<Guid>(nullable: false),
                    InformationId = table.Column<Guid>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_InformationTagInformation",
                        x => new { x.InformationId, x.InformationTagId }
                    );
                    table.ForeignKey(
                        name: "FK_InformationTagInformation_Information_InformationId",
                        column: x => x.InformationId,
                        principalTable: "Information",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_InformationTagInformation_InformationTag_InformationTagId",
                        column: x => x.InformationTagId,
                        principalTable: "InformationTag",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "InformationLocation",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    InformationId = table.Column<Guid>(nullable: false),
                    ElectronicDeviceId = table.Column<Guid>(nullable: true),
                    VirtualMachineId = table.Column<Guid>(nullable: true),
                    SoftwareId = table.Column<Guid>(nullable: true),
                    CloudId = table.Column<Guid>(nullable: true),
                    PhysicalLocationId = table.Column<Guid>(nullable: true),
                    URL = table.Column<string>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InformationLocation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InformationLocation_Cloud_CloudId",
                        column: x => x.CloudId,
                        principalTable: "Cloud",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_InformationLocation_ElectronicDevice_ElectronicDeviceId",
                        column: x => x.ElectronicDeviceId,
                        principalTable: "ElectronicDevice",
                        principalColumn: "AssetId",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_InformationLocation_Information_InformationId",
                        column: x => x.InformationId,
                        principalTable: "Information",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_InformationLocation_Location_PhysicalLocationId",
                        column: x => x.PhysicalLocationId,
                        principalTable: "Location",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_InformationLocation_Software_SoftwareId",
                        column: x => x.SoftwareId,
                        principalTable: "Software",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_InformationLocation_VirtualMachine_VirtualMachineId",
                        column: x => x.VirtualMachineId,
                        principalTable: "VirtualMachine",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_Cloud_VolumeId",
                table: "Cloud",
                column: "VolumeId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_InformationLocation_CloudId",
                table: "InformationLocation",
                column: "CloudId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_InformationLocation_ElectronicDeviceId",
                table: "InformationLocation",
                column: "ElectronicDeviceId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_InformationLocation_InformationId",
                table: "InformationLocation",
                column: "InformationId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_InformationLocation_PhysicalLocationId",
                table: "InformationLocation",
                column: "PhysicalLocationId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_InformationLocation_SoftwareId",
                table: "InformationLocation",
                column: "SoftwareId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_InformationLocation_VirtualMachineId",
                table: "InformationLocation",
                column: "VirtualMachineId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_InformationTagInformation_InformationTagId",
                table: "InformationTagInformation",
                column: "InformationTagId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Software_VolumeId",
                table: "Software",
                column: "VolumeId"
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "InformationLocation");

            migrationBuilder.DropTable(name: "InformationTagInformation");

            migrationBuilder.DropTable(name: "Cloud");

            migrationBuilder.DropTable(name: "Software");

            migrationBuilder.DropTable(name: "VirtualMachine");

            migrationBuilder.DropTable(name: "InformationTag");

            migrationBuilder.DropColumn(name: "Status", table: "Information");

            migrationBuilder.DropColumn(name: "Status", table: "Asset");
        }
    }
}
