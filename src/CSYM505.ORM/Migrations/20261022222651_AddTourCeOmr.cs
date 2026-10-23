
#nullable disable

namespace CSYM505.ORM.Migrations
{
    using Microsoft.EntityFrameworkCore.Migrations;

    /// <inheritdoc />
    public partial class AddTourCeOmr : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tour_ce_omr",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    freq = table.Column<string>(type: "TEXT", nullable: false),
                    indic_to = table.Column<string>(type: "TEXT", nullable: false),
                    c_resid = table.Column<string>(type: "TEXT", nullable: false),
                    month = table.Column<string>(type: "TEXT", nullable: false),
                    unit = table.Column<string>(type: "TEXT", nullable: false),
                    geo = table.Column<string>(type: "TEXT", nullable: false),
                    year = table.Column<int>(type: "INTEGER", nullable: false),
                    value = table.Column<decimal>(type: "TEXT", nullable: true),
                    flags = table.Column<string>(type: "TEXT", nullable: false),
                    raw_value = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tour_ce_omr", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tour_ce_omr_freq_indic_to_c_resid_month_unit_geo_year",
                table: "tour_ce_omr",
                columns: new[] { "freq", "indic_to", "c_resid", "month", "unit", "geo", "year" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tour_ce_omr");
        }
    }
}
