using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    public partial class AddNullableColumnInLocation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_Location_State_StateId", table: "Location");

            migrationBuilder.AlterColumn<Guid>(
                name: "StateId",
                table: "Location",
                nullable: true,
                oldClrType: typeof(Guid)
            );

            migrationBuilder.AlterColumn<int>(
                name: "HouseNumber",
                table: "Location",
                nullable: true,
                oldClrType: typeof(int)
            );

            migrationBuilder.AlterColumn<int>(
                name: "Floor",
                table: "Location",
                nullable: true,
                oldClrType: typeof(int)
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Location_State_StateId",
                table: "Location",
                column: "StateId",
                principalTable: "State",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_Location_State_StateId", table: "Location");

            migrationBuilder.AlterColumn<Guid>(
                name: "StateId",
                table: "Location",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true
            );

            migrationBuilder.AlterColumn<int>(
                name: "HouseNumber",
                table: "Location",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true
            );

            migrationBuilder.AlterColumn<int>(
                name: "Floor",
                table: "Location",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Location_State_StateId",
                table: "Location",
                column: "StateId",
                principalTable: "State",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );
        }
    }
}
