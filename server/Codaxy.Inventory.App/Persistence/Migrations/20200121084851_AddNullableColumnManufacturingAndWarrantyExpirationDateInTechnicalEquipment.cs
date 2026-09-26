using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    public partial class AddNullableColumnManufacturingAndWarrantyExpirationDateInTechnicalEquipment
        : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "WarrantyExpirationDate",
                table: "TechnicalEquipment",
                nullable: true,
                oldClrType: typeof(DateTime)
            );

            migrationBuilder.AlterColumn<DateTime>(
                name: "ManufacturingDate",
                table: "TechnicalEquipment",
                nullable: true,
                oldClrType: typeof(DateTime)
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "WarrantyExpirationDate",
                table: "TechnicalEquipment",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldNullable: true
            );

            migrationBuilder.AlterColumn<DateTime>(
                name: "ManufacturingDate",
                table: "TechnicalEquipment",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldNullable: true
            );
        }
    }
}
