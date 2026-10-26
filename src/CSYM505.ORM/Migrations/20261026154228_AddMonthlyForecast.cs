using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CSYM505.ORM.Migrations
{
    /// <inheritdoc />
    public partial class AddMonthlyForecast : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "analytics_monthly_forecast",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    series = table.Column<string>(type: "TEXT", nullable: false),
                    dataset = table.Column<string>(type: "TEXT", nullable: false),
                    geo = table.Column<string>(type: "TEXT", nullable: false),
                    unit = table.Column<string>(type: "TEXT", nullable: false),
                    training_start_month = table.Column<DateTime>(type: "TEXT", nullable: false),
                    training_end_month = table.Column<DateTime>(type: "TEXT", nullable: false),
                    horizon_months = table.Column<int>(type: "INTEGER", nullable: false),
                    month = table.Column<DateTime>(type: "TEXT", nullable: false),
                    method = table.Column<string>(type: "TEXT", nullable: false),
                    predicted_value = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_analytics_monthly_forecast", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_analytics_monthly_forecast_series_geo_month",
                table: "analytics_monthly_forecast",
                columns: new[] { "series", "geo", "month" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "analytics_monthly_forecast");
        }
    }
}
