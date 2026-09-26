using Microsoft.EntityFrameworkCore.Migrations;

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    public partial class SetAssetLastModifiedDate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE public.\"Asset\" SET \"LastModified\" = \"PurchaseDate\"");
        }

        protected override void Down(MigrationBuilder migrationBuilder) { }
    }
}
