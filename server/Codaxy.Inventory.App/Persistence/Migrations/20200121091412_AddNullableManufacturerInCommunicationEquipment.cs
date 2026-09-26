using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    public partial class AddNullableManufacturerInCommunicationEquipment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CommunicationEquipment_Manufacturer_ManufacturerId",
                table: "CommunicationEquipment"
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "ManufacturerId",
                table: "CommunicationEquipment",
                nullable: true,
                oldClrType: typeof(Guid)
            );

            migrationBuilder.AddForeignKey(
                name: "FK_CommunicationEquipment_Manufacturer_ManufacturerId",
                table: "CommunicationEquipment",
                column: "ManufacturerId",
                principalTable: "Manufacturer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CommunicationEquipment_Manufacturer_ManufacturerId",
                table: "CommunicationEquipment"
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "ManufacturerId",
                table: "CommunicationEquipment",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true
            );

            migrationBuilder.AddForeignKey(
                name: "FK_CommunicationEquipment_Manufacturer_ManufacturerId",
                table: "CommunicationEquipment",
                column: "ManufacturerId",
                principalTable: "Manufacturer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );
        }
    }
}
