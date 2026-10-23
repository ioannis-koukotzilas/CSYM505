
#nullable disable

namespace CSYM505.ORM.Migrations
{
    using Microsoft.EntityFrameworkCore.Migrations;

    /// <inheritdoc />
    public partial class AddTourOccNin3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tour_occ_nin3",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    freq = table.Column<string>(type: "TEXT", nullable: false),
                    c_resid = table.Column<string>(type: "TEXT", nullable: false),
                    unit = table.Column<string>(type: "TEXT", nullable: false),
                    nace_r2 = table.Column<string>(type: "TEXT", nullable: false),
                    geo = table.Column<string>(type: "TEXT", nullable: false),
                    year = table.Column<int>(type: "INTEGER", nullable: false),
                    value = table.Column<decimal>(type: "TEXT", nullable: true),
                    flags = table.Column<string>(type: "TEXT", nullable: false),
                    raw_value = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tour_occ_nin3", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tour_occ_nin3_freq_c_resid_unit_nace_r2_geo_year",
                table: "tour_occ_nin3",
                columns: new[] { "freq", "c_resid", "unit", "nace_r2", "geo", "year" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tour_occ_nin3");
        }
    }
}
