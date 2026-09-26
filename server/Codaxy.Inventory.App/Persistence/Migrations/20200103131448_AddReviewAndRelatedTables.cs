using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    public partial class AddReviewAndRelatedTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Review",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Goal = table.Column<string>(maxLength: 100, nullable: true),
                    Description = table.Column<string>(maxLength: 300, nullable: true),
                    StartDate = table.Column<DateTime>(nullable: false),
                    EndDate = table.Column<DateTime>(nullable: false),
                    Responsible = table.Column<int>(nullable: false),
                    Committee = table.Column<string>(nullable: true),
                    FindingURL = table.Column<string>(maxLength: 100, nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Review", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "ReviewStatus",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Status = table.Column<string>(maxLength: 30, nullable: true),
                    Description = table.Column<string>(maxLength: 300, nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewStatus", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "ReviewLog",
                columns: table => new
                {
                    AssetId = table.Column<Guid>(nullable: false),
                    Id = table.Column<Guid>(nullable: false),
                    ReviewStatusId = table.Column<Guid>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    AssetId1 = table.Column<Guid>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewLog", x => x.AssetId);
                    table.ForeignKey(
                        name: "FK_ReviewLog_Asset_AssetId1",
                        column: x => x.AssetId1,
                        principalTable: "Asset",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_ReviewLog_ReviewStatus_ReviewStatusId",
                        column: x => x.ReviewStatusId,
                        principalTable: "ReviewStatus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_ReviewLog_AssetId1",
                table: "ReviewLog",
                column: "AssetId1"
            );

            migrationBuilder.CreateIndex(
                name: "IX_ReviewLog_ReviewStatusId",
                table: "ReviewLog",
                column: "ReviewStatusId"
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Review");

            migrationBuilder.DropTable(name: "ReviewLog");

            migrationBuilder.DropTable(name: "ReviewStatus");
        }
    }
}
