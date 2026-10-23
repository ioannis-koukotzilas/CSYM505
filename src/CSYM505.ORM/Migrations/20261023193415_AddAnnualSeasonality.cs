using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CSYM505.ORM.Migrations
{
    /// <inheritdoc />
    public partial class AddAnnualSeasonality : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "analytics_annual_seasonality",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    dataset = table.Column<string>(type: "TEXT", nullable: false),
                    freq = table.Column<string>(type: "TEXT", nullable: false),
                    c_resid = table.Column<string>(type: "TEXT", nullable: false),
                    nace_r2 = table.Column<string>(type: "TEXT", nullable: false),
                    unit = table.Column<string>(type: "TEXT", nullable: false),
                    geo = table.Column<string>(type: "TEXT", nullable: false),
                    year = table.Column<int>(type: "INTEGER", nullable: false),
                    summer_nights = table.Column<decimal>(type: "TEXT", nullable: true),
                    annual_nights = table.Column<decimal>(type: "TEXT", nullable: true),
                    summer_share_percent = table.Column<decimal>(type: "TEXT", nullable: true),
                    summer_flags = table.Column<string>(type: "TEXT", nullable: true),
                    annual_flags = table.Column<string>(type: "TEXT", nullable: true),
                    annual_raw_value = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_analytics_annual_seasonality", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_analytics_annual_seasonality_geo_year",
                table: "analytics_annual_seasonality",
                columns: new[] { "geo", "year" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "analytics_annual_seasonality");
        }
    }
}
