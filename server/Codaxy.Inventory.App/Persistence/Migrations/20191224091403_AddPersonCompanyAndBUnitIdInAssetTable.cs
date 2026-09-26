using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    public partial class AddPersonCompanyAndBUnitIdInAssetTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BusinessUnitId",
                table: "Asset",
                nullable: false
            );

            migrationBuilder.AddColumn<Guid>(name: "CompanyId", table: "Asset", nullable: false);

            migrationBuilder.AddColumn<Guid>(name: "PersonId", table: "Asset", nullable: false);

            migrationBuilder.CreateIndex(
                name: "IX_Asset_BusinessUnitId",
                table: "Asset",
                column: "BusinessUnitId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Asset_CompanyId",
                table: "Asset",
                column: "CompanyId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Asset_PersonId",
                table: "Asset",
                column: "PersonId"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Asset_BusinessUnit_BusinessUnitId",
                table: "Asset",
                column: "BusinessUnitId",
                principalTable: "BusinessUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Asset_Company_CompanyId",
                table: "Asset",
                column: "CompanyId",
                principalTable: "Company",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Asset_Person_PersonId",
                table: "Asset",
                column: "PersonId",
                principalTable: "Person",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Asset_BusinessUnit_BusinessUnitId",
                table: "Asset"
            );

            migrationBuilder.DropForeignKey(name: "FK_Asset_Company_CompanyId", table: "Asset");

            migrationBuilder.DropForeignKey(name: "FK_Asset_Person_PersonId", table: "Asset");

            migrationBuilder.DropIndex(name: "IX_Asset_BusinessUnitId", table: "Asset");

            migrationBuilder.DropIndex(name: "IX_Asset_CompanyId", table: "Asset");

            migrationBuilder.DropIndex(name: "IX_Asset_PersonId", table: "Asset");

            migrationBuilder.DropColumn(name: "BusinessUnitId", table: "Asset");

            migrationBuilder.DropColumn(name: "CompanyId", table: "Asset");

            migrationBuilder.DropColumn(name: "PersonId", table: "Asset");
        }
    }
}
