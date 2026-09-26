using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    public partial class AddNavigationPropertyAndNullableStartAndEndDateReview : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Responsible", table: "Review");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartDate",
                table: "Review",
                nullable: true,
                oldClrType: typeof(DateTime)
            );

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndDate",
                table: "Review",
                nullable: true,
                oldClrType: typeof(DateTime)
            );

            migrationBuilder.AddColumn<Guid>(name: "PersonId", table: "Review", nullable: false);

            migrationBuilder.CreateIndex(
                name: "IX_Review_PersonId",
                table: "Review",
                column: "PersonId"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Review_Person_PersonId",
                table: "Review",
                column: "PersonId",
                principalTable: "Person",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_Review_Person_PersonId", table: "Review");

            migrationBuilder.DropIndex(name: "IX_Review_PersonId", table: "Review");

            migrationBuilder.DropColumn(name: "PersonId", table: "Review");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartDate",
                table: "Review",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldNullable: true
            );

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndDate",
                table: "Review",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldNullable: true
            );

            migrationBuilder.AddColumn<int>(
                name: "Responsible",
                table: "Review",
                nullable: false,
                defaultValue: 0
            );
        }
    }
}
