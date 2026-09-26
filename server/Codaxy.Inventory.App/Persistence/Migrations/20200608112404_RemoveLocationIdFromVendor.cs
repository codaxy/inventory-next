using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    public partial class RemoveLocationIdFromVendor : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_Vendor_Location_LocationId", table: "Vendor");

            migrationBuilder.DropIndex(name: "IX_Vendor_LocationId", table: "Vendor");

            migrationBuilder.DropColumn(name: "LocationId", table: "Vendor");

            migrationBuilder.AddColumn<string>(name: "Location", table: "Vendor", nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Location", table: "Vendor");

            migrationBuilder.AddColumn<Guid>(
                name: "LocationId",
                table: "Vendor",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000")
            );

            migrationBuilder.CreateIndex(
                name: "IX_Vendor_LocationId",
                table: "Vendor",
                column: "LocationId"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Vendor_Location_LocationId",
                table: "Vendor",
                column: "LocationId",
                principalTable: "Location",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );
        }
    }
}
