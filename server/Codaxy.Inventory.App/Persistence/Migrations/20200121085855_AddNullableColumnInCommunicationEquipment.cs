using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    public partial class AddNullableColumnInCommunicationEquipment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "WarrantyExpirationDate",
                table: "CommunicationEquipment",
                nullable: true,
                oldClrType: typeof(DateTime)
            );

            migrationBuilder.AlterColumn<DateTime>(
                name: "ManufacturingDate",
                table: "CommunicationEquipment",
                nullable: true,
                oldClrType: typeof(DateTime)
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "WarrantyExpirationDate",
                table: "CommunicationEquipment",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldNullable: true
            );

            migrationBuilder.AlterColumn<DateTime>(
                name: "ManufacturingDate",
                table: "CommunicationEquipment",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldNullable: true
            );
        }
    }
}
