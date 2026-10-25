using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CSYM505.ORM.Migrations
{
    /// <inheritdoc />
    public partial class AddAnnualRoomOccupancy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "analytics_annual_room_occupancy",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    dataset = table.Column<string>(type: "TEXT", nullable: false),
                    freq = table.Column<string>(type: "TEXT", nullable: false),
                    accomunit = table.Column<string>(type: "TEXT", nullable: false),
                    unit = table.Column<string>(type: "TEXT", nullable: false),
                    geo = table.Column<string>(type: "TEXT", nullable: false),
                    year = table.Column<int>(type: "INTEGER", nullable: false),
                    occupancy_percent = table.Column<decimal>(type: "TEXT", nullable: true),
                    previous_year_occupancy_percent = table.Column<decimal>(type: "TEXT", nullable: true),
                    annual_change_percentage_points = table.Column<decimal>(type: "TEXT", nullable: true),
                    flags = table.Column<string>(type: "TEXT", nullable: false),
                    previous_year_flags = table.Column<string>(type: "TEXT", nullable: true),
                    raw_value = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_analytics_annual_room_occupancy", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_analytics_annual_room_occupancy_geo_year",
                table: "analytics_annual_room_occupancy",
                columns: new[] { "geo", "year" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "analytics_annual_room_occupancy");
        }
    }
}
