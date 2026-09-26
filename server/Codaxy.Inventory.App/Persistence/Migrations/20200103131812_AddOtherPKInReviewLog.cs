using Microsoft.EntityFrameworkCore.Migrations;

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    public partial class AddOtherPKInReviewLog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(name: "PK_ReviewLog", table: "ReviewLog");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_ReviewLog_AssetId",
                table: "ReviewLog",
                column: "AssetId"
            );

            migrationBuilder.AddPrimaryKey(name: "PK_ReviewLog", table: "ReviewLog", column: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(name: "AK_ReviewLog_AssetId", table: "ReviewLog");

            migrationBuilder.DropPrimaryKey(name: "PK_ReviewLog", table: "ReviewLog");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReviewLog",
                table: "ReviewLog",
                column: "AssetId"
            );
        }
    }
}
