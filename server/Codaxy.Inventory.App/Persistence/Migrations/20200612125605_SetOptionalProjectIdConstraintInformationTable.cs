using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    public partial class SetOptionalProjectIdConstraintInformationTable : Migration
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
