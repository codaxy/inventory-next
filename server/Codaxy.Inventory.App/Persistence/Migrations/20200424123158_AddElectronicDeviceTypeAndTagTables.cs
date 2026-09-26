using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    public partial class AddElectronicDeviceTypeAndTagTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ElectronicDeviceTag",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElectronicDeviceTag", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "ElectronicDeviceType",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElectronicDeviceType", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "ElectronicDeviceTypeElectronicDeviceTag",
                columns: table => new
                {
                    ElectronicDeviceTypeId = table.Column<Guid>(nullable: false),
                    ElectronicDeviceTagId = table.Column<Guid>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_ElectronicDeviceTypeElectronicDeviceTag",
                        x => new { x.ElectronicDeviceTypeId, x.ElectronicDeviceTagId }
                    );
                    table.ForeignKey(
                        name: "FK_ElectronicDeviceTypeElectronicDeviceTag_ElectronicDeviceTag~",
                        column: x => x.ElectronicDeviceTagId,
                        principalTable: "ElectronicDeviceTag",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_ElectronicDeviceTypeElectronicDeviceTag_ElectronicDeviceTyp~",
                        column: x => x.ElectronicDeviceTypeId,
                        principalTable: "ElectronicDeviceType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_ElectronicDeviceTypeElectronicDeviceTag_ElectronicDeviceTag~",
                table: "ElectronicDeviceTypeElectronicDeviceTag",
                column: "ElectronicDeviceTagId"
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ElectronicDeviceTypeElectronicDeviceTag");

            migrationBuilder.DropTable(name: "ElectronicDeviceTag");

            migrationBuilder.DropTable(name: "ElectronicDeviceType");
        }
    }
}
