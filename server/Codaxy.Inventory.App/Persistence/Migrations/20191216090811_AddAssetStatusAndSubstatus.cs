using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    public partial class AddAssetStatusAndSubstatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AssetStatus",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Status = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetStatus", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "AssetSubstatus",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    AssetStatusId = table.Column<Guid>(nullable: false),
                    Substatus = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetSubstatus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetSubstatus_AssetStatus_AssetStatusId",
                        column: x => x.AssetStatusId,
                        principalTable: "AssetStatus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_AssetSubstatus_AssetStatusId",
                table: "AssetSubstatus",
                column: "AssetStatusId"
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "AssetSubstatus");

            migrationBuilder.DropTable(name: "AssetStatus");
        }
    }
}
