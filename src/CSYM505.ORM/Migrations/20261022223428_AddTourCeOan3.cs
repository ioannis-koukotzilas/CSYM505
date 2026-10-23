
#nullable disable

namespace CSYM505.ORM.Migrations
{
    using Microsoft.EntityFrameworkCore.Migrations;

    /// <inheritdoc />
    public partial class AddTourCeOan3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tour_ce_oan3",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    freq = table.Column<string>(type: "TEXT", nullable: false),
                    indic_to = table.Column<string>(type: "TEXT", nullable: false),
                    c_resid = table.Column<string>(type: "TEXT", nullable: false),
                    unit = table.Column<string>(type: "TEXT", nullable: false),
                    geo = table.Column<string>(type: "TEXT", nullable: false),
                    year = table.Column<int>(type: "INTEGER", nullable: false),
                    value = table.Column<decimal>(type: "TEXT", nullable: true),
                    flags = table.Column<string>(type: "TEXT", nullable: false),
                    raw_value = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tour_ce_oan3", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tour_ce_oan3_freq_indic_to_c_resid_unit_geo_year",
                table: "tour_ce_oan3",
                columns: new[] { "freq", "indic_to", "c_resid", "unit", "geo", "year" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tour_ce_oan3");
        }
    }
}
