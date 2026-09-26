using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    public partial class AddNullableFKInAsset : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Asset_Availability_AvailabilityId",
                table: "Asset"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_Asset_BusinessUnit_BusinessUnitId",
                table: "Asset"
            );

            migrationBuilder.DropForeignKey(name: "FK_Asset_Company_CompanyId", table: "Asset");

            migrationBuilder.DropForeignKey(
                name: "FK_Asset_Confidentiality_ConfidentialityId",
                table: "Asset"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_Asset_Importance_ImportanceId",
                table: "Asset"
            );

            migrationBuilder.DropForeignKey(name: "FK_Asset_Integrity_IntegrityId", table: "Asset");

            migrationBuilder.DropForeignKey(name: "FK_Asset_Location_LocationId", table: "Asset");

            migrationBuilder.DropForeignKey(name: "FK_Asset_Person_PersonId", table: "Asset");

            migrationBuilder.AlterColumn<Guid>(
                name: "PersonId",
                table: "Asset",
                nullable: true,
                oldClrType: typeof(Guid)
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "LocationId",
                table: "Asset",
                nullable: true,
                oldClrType: typeof(Guid)
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "IntegrityId",
                table: "Asset",
                nullable: true,
                oldClrType: typeof(Guid)
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "ImportanceId",
                table: "Asset",
                nullable: true,
                oldClrType: typeof(Guid)
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "ConfidentialityId",
                table: "Asset",
                nullable: true,
                oldClrType: typeof(Guid)
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "CompanyId",
                table: "Asset",
                nullable: true,
                oldClrType: typeof(Guid)
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "BusinessUnitId",
                table: "Asset",
                nullable: true,
                oldClrType: typeof(Guid)
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "AvailabilityId",
                table: "Asset",
                nullable: true,
                oldClrType: typeof(Guid)
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Asset_Availability_AvailabilityId",
                table: "Asset",
                column: "AvailabilityId",
                principalTable: "Availability",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Asset_BusinessUnit_BusinessUnitId",
                table: "Asset",
                column: "BusinessUnitId",
                principalTable: "BusinessUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Asset_Company_CompanyId",
                table: "Asset",
                column: "CompanyId",
                principalTable: "Company",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Asset_Confidentiality_ConfidentialityId",
                table: "Asset",
                column: "ConfidentialityId",
                principalTable: "Confidentiality",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Asset_Importance_ImportanceId",
                table: "Asset",
                column: "ImportanceId",
                principalTable: "Importance",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Asset_Integrity_IntegrityId",
                table: "Asset",
                column: "IntegrityId",
                principalTable: "Integrity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Asset_Location_LocationId",
                table: "Asset",
                column: "LocationId",
                principalTable: "Location",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Asset_Person_PersonId",
                table: "Asset",
                column: "PersonId",
                principalTable: "Person",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Asset_Availability_AvailabilityId",
                table: "Asset"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_Asset_BusinessUnit_BusinessUnitId",
                table: "Asset"
            );

            migrationBuilder.DropForeignKey(name: "FK_Asset_Company_CompanyId", table: "Asset");

            migrationBuilder.DropForeignKey(
                name: "FK_Asset_Confidentiality_ConfidentialityId",
                table: "Asset"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_Asset_Importance_ImportanceId",
                table: "Asset"
            );

            migrationBuilder.DropForeignKey(name: "FK_Asset_Integrity_IntegrityId", table: "Asset");

            migrationBuilder.DropForeignKey(name: "FK_Asset_Location_LocationId", table: "Asset");

            migrationBuilder.DropForeignKey(name: "FK_Asset_Person_PersonId", table: "Asset");

            migrationBuilder.AlterColumn<Guid>(
                name: "PersonId",
                table: "Asset",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "LocationId",
                table: "Asset",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "IntegrityId",
                table: "Asset",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "ImportanceId",
                table: "Asset",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "ConfidentialityId",
                table: "Asset",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "CompanyId",
                table: "Asset",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "BusinessUnitId",
                table: "Asset",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "AvailabilityId",
                table: "Asset",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Asset_Availability_AvailabilityId",
                table: "Asset",
                column: "AvailabilityId",
                principalTable: "Availability",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
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
                name: "FK_Asset_Confidentiality_ConfidentialityId",
                table: "Asset",
                column: "ConfidentialityId",
                principalTable: "Confidentiality",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Asset_Importance_ImportanceId",
                table: "Asset",
                column: "ImportanceId",
                principalTable: "Importance",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Asset_Integrity_IntegrityId",
                table: "Asset",
                column: "IntegrityId",
                principalTable: "Integrity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Asset_Location_LocationId",
                table: "Asset",
                column: "LocationId",
                principalTable: "Location",
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
    }
}
