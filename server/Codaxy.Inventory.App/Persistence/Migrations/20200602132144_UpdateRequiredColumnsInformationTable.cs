using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    public partial class UpdateRequiredColumnsInformationTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Information_Project_ProjectId",
                table: "Information"
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "ProjectId",
                table: "Information",
                nullable: true,
                oldClrType: typeof(Guid)
            );

            migrationBuilder.AlterColumn<bool>(
                name: "PersonalInformation",
                table: "Information",
                nullable: true,
                oldClrType: typeof(bool)
            );

            migrationBuilder.AlterColumn<bool>(
                name: "Incomplete",
                table: "Information",
                nullable: true,
                oldClrType: typeof(bool)
            );

            migrationBuilder.AlterColumn<bool>(
                name: "ClientsPersonalInformation",
                table: "Information",
                nullable: true,
                oldClrType: typeof(bool)
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Information_Project_ProjectId",
                table: "Information",
                column: "ProjectId",
                principalTable: "Project",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Information_Project_ProjectId",
                table: "Information"
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "ProjectId",
                table: "Information",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true
            );

            migrationBuilder.AlterColumn<bool>(
                name: "PersonalInformation",
                table: "Information",
                nullable: false,
                oldClrType: typeof(bool),
                oldNullable: true
            );

            migrationBuilder.AlterColumn<bool>(
                name: "Incomplete",
                table: "Information",
                nullable: false,
                oldClrType: typeof(bool),
                oldNullable: true
            );

            migrationBuilder.AlterColumn<bool>(
                name: "ClientsPersonalInformation",
                table: "Information",
                nullable: false,
                oldClrType: typeof(bool),
                oldNullable: true
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Information_Project_ProjectId",
                table: "Information",
                column: "ProjectId",
                principalTable: "Project",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );
        }
    }
}
