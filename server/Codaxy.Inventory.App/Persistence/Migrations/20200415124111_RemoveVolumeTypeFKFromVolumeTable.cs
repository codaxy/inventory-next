using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    public partial class RemoveVolumeTypeFKFromVolumeTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Volume_VolumeType_VolumeTypeId",
                table: "Volume"
            );

            migrationBuilder.DropIndex(name: "IX_Volume_VolumeTypeId", table: "Volume");

            migrationBuilder.DropColumn(name: "VolumeTypeId", table: "Volume");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "VolumeTypeId",
                table: "Volume",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000")
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
    }
}
