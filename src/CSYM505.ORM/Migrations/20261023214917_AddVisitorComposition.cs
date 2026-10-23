using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CSYM505.ORM.Migrations
{
    /// <inheritdoc />
    public partial class AddVisitorComposition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "analytics_visitor_composition",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    dataset = table.Column<string>(type: "TEXT", nullable: false),
                    freq = table.Column<string>(type: "TEXT", nullable: false),
                    nace_r2 = table.Column<string>(type: "TEXT", nullable: false),
                    month = table.Column<string>(type: "TEXT", nullable: false),
                    unit = table.Column<string>(type: "TEXT", nullable: false),
                    geo = table.Column<string>(type: "TEXT", nullable: false),
                    year = table.Column<int>(type: "INTEGER", nullable: false),
                    domestic_nights = table.Column<decimal>(type: "TEXT", nullable: true),
                    foreign_nights = table.Column<decimal>(type: "TEXT", nullable: true),
                    total_nights = table.Column<decimal>(type: "TEXT", nullable: true),
                    foreign_share_percent = table.Column<decimal>(type: "TEXT", nullable: true),
                    domestic_flags = table.Column<string>(type: "TEXT", nullable: true),
                    foreign_flags = table.Column<string>(type: "TEXT", nullable: true),
                    total_flags = table.Column<string>(type: "TEXT", nullable: true),
                    domestic_raw_value = table.Column<string>(type: "TEXT", nullable: true),
                    foreign_raw_value = table.Column<string>(type: "TEXT", nullable: true),
                    total_raw_value = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_analytics_visitor_composition", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_analytics_visitor_composition_geo_year",
                table: "analytics_visitor_composition",
                columns: new[] { "geo", "year" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "analytics_visitor_composition");
        }
    }
}
