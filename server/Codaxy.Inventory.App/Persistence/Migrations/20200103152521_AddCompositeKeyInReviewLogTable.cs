using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    public partial class AddCompositeKeyInReviewLogTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReviewLog_Asset_AssetId1",
                table: "ReviewLog"
            );

            migrationBuilder.DropUniqueConstraint(name: "AK_ReviewLog_AssetId", table: "ReviewLog");

            migrationBuilder.DropPrimaryKey(name: "PK_ReviewLog", table: "ReviewLog");

            migrationBuilder.DropIndex(name: "IX_ReviewLog_AssetId1", table: "ReviewLog");

            migrationBuilder.DropColumn(name: "AssetId1", table: "ReviewLog");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReviewLog",
                table: "ReviewLog",
                columns: new[] { "Id", "AssetId" }
            );

            migrationBuilder.CreateIndex(
                name: "IX_ReviewLog_AssetId",
                table: "ReviewLog",
                column: "AssetId"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_ReviewLog_Asset_AssetId",
                table: "ReviewLog",
                column: "AssetId",
                principalTable: "Asset",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_ReviewLog_Asset_AssetId", table: "ReviewLog");

            migrationBuilder.DropPrimaryKey(name: "PK_ReviewLog", table: "ReviewLog");

            migrationBuilder.DropIndex(name: "IX_ReviewLog_AssetId", table: "ReviewLog");

            migrationBuilder.AddColumn<Guid>(name: "AssetId1", table: "ReviewLog", nullable: true);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_ReviewLog_AssetId",
                table: "ReviewLog",
                column: "AssetId"
            );

            migrationBuilder.AddPrimaryKey(name: "PK_ReviewLog", table: "ReviewLog", column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewLog_AssetId1",
                table: "ReviewLog",
                column: "AssetId1"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_ReviewLog_Asset_AssetId1",
                table: "ReviewLog",
                column: "AssetId1",
                principalTable: "Asset",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );
        }
    }
}
