using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class TypeIconParts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Computer components are parts, not a desktop: the icon the first guess gave them. Only
            // where the guess put them, so an icon someone picked since stays.
            migrationBuilder.Sql(
                """
                update electronic_device_type set icon = 'parts'
                where icon = 'desktop' and name ~* 'component|komponent|\yparts?\y|dijelov';
                """
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("update electronic_device_type set icon = 'desktop' where icon = 'parts';");
        }
    }
}
