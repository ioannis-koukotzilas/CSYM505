using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CSYM505.ORM.Migrations
{
    /// <inheritdoc />
    public partial class AddBankOfGreeceRegionalData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "bog_regional_expenditure_per_night",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    freq = table.Column<string>(type: "TEXT", nullable: false),
                    geo = table.Column<string>(type: "TEXT", nullable: false),
                    region = table.Column<string>(type: "TEXT", nullable: false),
                    country = table.Column<string>(type: "TEXT", nullable: false),
                    year = table.Column<int>(type: "INTEGER", nullable: false),
                    unit = table.Column<string>(type: "TEXT", nullable: false),
                    value = table.Column<decimal>(type: "TEXT", nullable: true),
                    flags = table.Column<string>(type: "TEXT", nullable: false),
                    raw_value = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bog_regional_expenditure_per_night", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "bog_regional_length_of_stay",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    freq = table.Column<string>(type: "TEXT", nullable: false),
                    geo = table.Column<string>(type: "TEXT", nullable: false),
                    region = table.Column<string>(type: "TEXT", nullable: false),
                    country = table.Column<string>(type: "TEXT", nullable: false),
                    year = table.Column<int>(type: "INTEGER", nullable: false),
                    unit = table.Column<string>(type: "TEXT", nullable: false),
                    value = table.Column<decimal>(type: "TEXT", nullable: true),
                    flags = table.Column<string>(type: "TEXT", nullable: false),
                    raw_value = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bog_regional_length_of_stay", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "bog_regional_travel_receipts",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    freq = table.Column<string>(type: "TEXT", nullable: false),
                    geo = table.Column<string>(type: "TEXT", nullable: false),
                    region = table.Column<string>(type: "TEXT", nullable: false),
                    country = table.Column<string>(type: "TEXT", nullable: false),
                    year = table.Column<int>(type: "INTEGER", nullable: false),
                    unit = table.Column<string>(type: "TEXT", nullable: false),
                    value = table.Column<decimal>(type: "TEXT", nullable: true),
                    flags = table.Column<string>(type: "TEXT", nullable: false),
                    raw_value = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bog_regional_travel_receipts", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_bog_regional_expenditure_per_night_freq_geo_country_year_unit",
                table: "bog_regional_expenditure_per_night",
                columns: new[] { "freq", "geo", "country", "year", "unit" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_bog_regional_length_of_stay_freq_geo_country_year_unit",
                table: "bog_regional_length_of_stay",
                columns: new[] { "freq", "geo", "country", "year", "unit" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_bog_regional_travel_receipts_freq_geo_country_year_unit",
                table: "bog_regional_travel_receipts",
                columns: new[] { "freq", "geo", "country", "year", "unit" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bog_regional_expenditure_per_night");

            migrationBuilder.DropTable(
                name: "bog_regional_length_of_stay");

            migrationBuilder.DropTable(
                name: "bog_regional_travel_receipts");
        }
    }
}
