using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CSYM505.ORM.Migrations
{
    /// <inheritdoc />
    public partial class AddAnnualPlatformDemand : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "analytics_annual_platform_demand",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    dataset = table.Column<string>(type: "TEXT", nullable: false),
                    freq = table.Column<string>(type: "TEXT", nullable: false),
                    indic_to = table.Column<string>(type: "TEXT", nullable: false),
                    c_resid = table.Column<string>(type: "TEXT", nullable: false),
                    month = table.Column<string>(type: "TEXT", nullable: true),
                    unit = table.Column<string>(type: "TEXT", nullable: false),
                    geo = table.Column<string>(type: "TEXT", nullable: false),
                    year = table.Column<int>(type: "INTEGER", nullable: false),
                    guest_nights = table.Column<decimal>(type: "TEXT", nullable: true),
                    previous_year_guest_nights = table.Column<decimal>(type: "TEXT", nullable: true),
                    annual_change_percent = table.Column<decimal>(type: "TEXT", nullable: true),
                    flags = table.Column<string>(type: "TEXT", nullable: false),
                    previous_year_flags = table.Column<string>(type: "TEXT", nullable: true),
                    raw_value = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_analytics_annual_platform_demand", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_analytics_annual_platform_demand_geo_year",
                table: "analytics_annual_platform_demand",
                columns: new[] { "geo", "year" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "analytics_annual_platform_demand");
        }
    }
}
