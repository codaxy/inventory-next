using Microsoft.EntityFrameworkCore.Migrations;

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    public partial class AddVolumeTypeFKToVolumeTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VolumeTypeId",
                table: "Volume",
                nullable: false,
                defaultValue: 0
            );

            migrationBuilder.CreateIndex(
                name: "IX_Volume_VolumeTypeId",
                table: "Volume",
                column: "VolumeTypeId"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Volume_VolumeType_VolumeTypeId",
                table: "Volume",
                column: "VolumeTypeId",
                principalTable: "VolumeType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Volume_VolumeType_VolumeTypeId",
                table: "Volume"
            );

            migrationBuilder.DropIndex(name: "IX_Volume_VolumeTypeId", table: "Volume");

            migrationBuilder.DropColumn(name: "VolumeTypeId", table: "Volume");
        }
    }
}
