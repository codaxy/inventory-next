using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    public partial class AddSoftwareOrServiceAndRelatedTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SoftwareOrServiceCategory",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoftwareOrServiceCategory", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "SoftwareOrService",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    SoftwareOrServiceCategoryId = table.Column<Guid>(nullable: false),
                    ManufacturerId = table.Column<Guid>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoftwareOrService", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SoftwareOrService_Manufacturer_ManufacturerId",
                        column: x => x.ManufacturerId,
                        principalTable: "Manufacturer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_SoftwareOrService_SoftwareOrServiceCategory_SoftwareOrServi~",
                        column: x => x.SoftwareOrServiceCategoryId,
                        principalTable: "SoftwareOrServiceCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareOrService_ManufacturerId",
                table: "SoftwareOrService",
                column: "ManufacturerId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareOrService_SoftwareOrServiceCategoryId",
                table: "SoftwareOrService",
                column: "SoftwareOrServiceCategoryId"
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "SoftwareOrService");

            migrationBuilder.DropTable(name: "SoftwareOrServiceCategory");
        }
    }
}
