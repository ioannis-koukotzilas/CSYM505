
#nullable disable

namespace CSYM505.ORM.Migrations
{
    using Microsoft.EntityFrameworkCore.Migrations;

    /// <inheritdoc />
    public partial class AddTourOccNin2m : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tour_occ_nin2m",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    freq = table.Column<string>(type: "TEXT", nullable: false),
                    c_resid = table.Column<string>(type: "TEXT", nullable: false),
                    nace_r2 = table.Column<string>(type: "TEXT", nullable: false),
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
                    table.PrimaryKey("PK_tour_occ_nin2m", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tour_occ_nin2m_freq_c_resid_nace_r2_month_unit_geo_year",
                table: "tour_occ_nin2m",
                columns: new[] { "freq", "c_resid", "nace_r2", "month", "unit", "geo", "year" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tour_occ_nin2m");
        }
    }
}
