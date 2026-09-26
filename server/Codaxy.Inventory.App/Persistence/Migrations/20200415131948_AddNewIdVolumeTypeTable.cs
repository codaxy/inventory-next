using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    public partial class AddNewIdVolumeTypeTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(name: "PK_VolumeType", table: "VolumeType");

            migrationBuilder
                .AddColumn<int>(name: "NewId", table: "VolumeType", nullable: false)
                .Annotation(
                    "Npgsql:ValueGenerationStrategy",
                    NpgsqlValueGenerationStrategy.SerialColumn
                );

            migrationBuilder.AddPrimaryKey(
                name: "PK_VolumeType",
                table: "VolumeType",
                column: "NewId"
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(name: "PK_VolumeType", table: "VolumeType");

            migrationBuilder.DropColumn(name: "NewId", table: "VolumeType");

            migrationBuilder.AddPrimaryKey(
                name: "PK_VolumeType",
                table: "VolumeType",
                column: "Id"
            );
        }
    }
}
