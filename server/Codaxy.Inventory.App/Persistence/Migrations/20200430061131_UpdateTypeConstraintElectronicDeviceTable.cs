using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    public partial class UpdateTypeConstraintElectronicDeviceTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ElectronicDevice_ElectronicDeviceType_ElectronicDeviceTypeId",
                table: "ElectronicDevice"
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "ElectronicDeviceTypeId",
                table: "ElectronicDevice",
                nullable: true,
                oldClrType: typeof(Guid)
            );

            migrationBuilder.AddForeignKey(
                name: "FK_ElectronicDevice_ElectronicDeviceType_ElectronicDeviceTypeId",
                table: "ElectronicDevice",
                column: "ElectronicDeviceTypeId",
                principalTable: "ElectronicDeviceType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ElectronicDevice_ElectronicDeviceType_ElectronicDeviceTypeId",
                table: "ElectronicDevice"
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "ElectronicDeviceTypeId",
                table: "ElectronicDevice",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true
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
    }
}
