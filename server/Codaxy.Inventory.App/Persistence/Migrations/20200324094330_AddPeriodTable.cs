using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    public partial class AddPeriodTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(name: "PeriodId", table: "License", nullable: true);

            migrationBuilder.CreateTable(
                name: "Period",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Value = table.Column<string>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Period", x => x.Id);
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_License_PeriodId",
                table: "License",
                column: "PeriodId"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_License_Period_PeriodId",
                table: "License",
                column: "PeriodId",
                principalTable: "Period",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_License_Period_PeriodId", table: "License");

            migrationBuilder.DropTable(name: "Period");

            migrationBuilder.DropIndex(name: "IX_License_PeriodId", table: "License");

            migrationBuilder.DropColumn(name: "PeriodId", table: "License");
        }
    }
}
