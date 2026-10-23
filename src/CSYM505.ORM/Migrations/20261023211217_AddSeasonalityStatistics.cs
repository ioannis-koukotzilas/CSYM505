using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CSYM505.ORM.Migrations
{
    /// <inheritdoc />
    public partial class AddSeasonalityStatistics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "coefficient_of_variation_percent",
                table: "analytics_annual_seasonality",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "mean_monthly_nights",
                table: "analytics_annual_seasonality",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "population_standard_deviation",
                table: "analytics_annual_seasonality",
                type: "REAL",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "coefficient_of_variation_percent",
                table: "analytics_annual_seasonality");

            migrationBuilder.DropColumn(
                name: "mean_monthly_nights",
                table: "analytics_annual_seasonality");

            migrationBuilder.DropColumn(
                name: "population_standard_deviation",
                table: "analytics_annual_seasonality");
        }
    }
}
