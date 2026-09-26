using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    public partial class AddTypeColumnInElectronicDeviceTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ElectronicDeviceTypeId",
                table: "ElectronicDevice",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000")
            );

            migrationBuilder.CreateIndex(
                name: "IX_ElectronicDevice_ElectronicDeviceTypeId",
                table: "ElectronicDevice",
                column: "ElectronicDeviceTypeId"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_ElectronicDevice_ElectronicDeviceType_ElectronicDeviceTypeId",
                table: "ElectronicDevice",
                column: "ElectronicDeviceTypeId",
                principalTable: "ElectronicDeviceType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ElectronicDevice_ElectronicDeviceType_ElectronicDeviceTypeId",
                table: "ElectronicDevice"
            );

            migrationBuilder.DropIndex(
                name: "IX_ElectronicDevice_ElectronicDeviceTypeId",
                table: "ElectronicDevice"
            );

            migrationBuilder.DropColumn(name: "ElectronicDeviceTypeId", table: "ElectronicDevice");
        }
    }
}
