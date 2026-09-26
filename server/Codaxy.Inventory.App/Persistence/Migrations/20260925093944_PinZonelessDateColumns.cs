using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    /// <summary>
    /// The calendar-date columns were created by migrations that declare no store type, so the type
    /// was resolved when they were applied — from <c>Npgsql.EnableLegacyTimestampBehavior</c>, which
    /// the app set and <c>dotnet ef</c> did not. Every database built through the app therefore has
    /// <c>timestamp without time zone</c> and one built by replaying the history has
    /// <c>timestamp with time zone</c>. With the switch gone the model states the type, and this
    /// makes the existing history agree with it.
    /// </summary>
    public partial class PinZonelessDateColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Guarded, so a database that already holds the right type is not rewritten.
            migrationBuilder.Sql(
                """
                DO $$
                DECLARE
                    target record;
                BEGIN
                    FOR target IN
                        SELECT table_name, column_name
                        FROM information_schema.columns
                        WHERE table_schema = 'public'
                          AND data_type = 'timestamp with time zone'
                          AND (table_name, column_name) IN (
                              ('asset', 'purchase_date'),
                              ('activation', 'activation_date'),
                              ('activation', 'deactivation_date'),
                              ('electronic_device', 'guarantee_expiration_date'),
                              ('electronic_device', 'manufacturing_date'),
                              ('license', 'subscription_expiration_date'),
                              ('maintenance_contract', 'expiration_date'),
                              ('maintenance_contract', 'service_due_date')
                          )
                    LOOP
                        EXECUTE format(
                            'ALTER TABLE %I ALTER COLUMN %I TYPE timestamp without time zone',
                            target.table_name, target.column_name
                        );
                    END LOOP;
                END $$;
                """
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Nothing to undo: the type the columns held before depended on which tool applied the
            // migration that created them, and every real database already held this one.
        }
    }
}
