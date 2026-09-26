using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    public partial class UpdateColumnNameInTechnicalEquipment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TechnicalEquipment_TechnicalEquipmentCategory_TechnicalEqui~",
                table: "TechnicalEquipment"
            );

            migrationBuilder.DropColumn(
                name: "TechnicalEquipmentClassId",
                table: "TechnicalEquipment"
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "TechnicalEquipmentCategoryId",
                table: "TechnicalEquipment",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true
            );

            migrationBuilder.AddForeignKey(
                name: "FK_TechnicalEquipment_TechnicalEquipmentCategory_TechnicalEqui~",
                table: "TechnicalEquipment",
                column: "TechnicalEquipmentCategoryId",
                principalTable: "TechnicalEquipmentCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TechnicalEquipment_TechnicalEquipmentCategory_TechnicalEqui~",
                table: "TechnicalEquipment"
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "TechnicalEquipmentCategoryId",
                table: "TechnicalEquipment",
                nullable: true,
                oldClrType: typeof(Guid)
            );

            migrationBuilder.AddColumn<Guid>(
                name: "TechnicalEquipmentClassId",
                table: "TechnicalEquipment",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000")
            );

            migrationBuilder.AddForeignKey(
                name: "FK_TechnicalEquipment_TechnicalEquipmentCategory_TechnicalEqui~",
                table: "TechnicalEquipment",
                column: "TechnicalEquipmentCategoryId",
                principalTable: "TechnicalEquipmentCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );
        }
    }
}
