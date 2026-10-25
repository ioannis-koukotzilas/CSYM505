using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CSYM505.ORM.Migrations
{
    /// <inheritdoc />
    public partial class AddRegionalTravelReceiptsShareView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                CREATE VIEW analytics_regional_travel_receipts_share AS
                SELECT
                    country_data.freq,
                    country_data.geo,
                    country_data.region,
                    country_data.country,
                    country_data.year,
                    country_data.unit,
                    CAST(country_data.value AS REAL) AS receipts_million_eur,
                    CAST(total_data.value AS REAL) AS regional_receipts_million_eur,
                    100.0 * CAST(country_data.value AS REAL)
                        / NULLIF(CAST(total_data.value AS REAL), 0)
                        AS receipts_share_percent,
                    country_data.flags,
                    country_data.raw_value,
                    total_data.flags AS regional_flags,
                    total_data.raw_value AS regional_raw_value
                FROM bog_regional_travel_receipts AS country_data
                LEFT JOIN bog_regional_travel_receipts AS total_data
                    ON total_data.freq = country_data.freq
                    AND total_data.geo = country_data.geo
                    AND total_data.year = country_data.year
                    AND total_data.unit = country_data.unit
                    AND total_data.country = 'TOTAL'
                WHERE country_data.country <> 'TOTAL';
                """
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DROP VIEW analytics_regional_travel_receipts_share;
                """
            );
        }
    }
}
