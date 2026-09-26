using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    public partial class AddAssetCategoryAndType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AssetCategory",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetCategory", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "AssetType",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    AssetCategoryId = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetType", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetType_AssetCategory_AssetCategoryId",
                        column: x => x.AssetCategoryId,
                        principalTable: "AssetCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

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

            migrationBuilder.CreateIndex(
                name: "IX_AssetType_AssetCategoryId",
                table: "AssetType",
                column: "AssetCategoryId"
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "AssetType");

            migrationBuilder.DropTable(name: "AssetCategory");
        }
    }
}
