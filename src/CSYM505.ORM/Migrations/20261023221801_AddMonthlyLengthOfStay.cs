using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CSYM505.ORM.Migrations
{
    /// <inheritdoc />
    public partial class AddMonthlyLengthOfStay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "analytics_monthly_length_of_stay",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    nights_dataset = table.Column<string>(type: "TEXT", nullable: false),
                    arrivals_dataset = table.Column<string>(type: "TEXT", nullable: false),
                    freq = table.Column<string>(type: "TEXT", nullable: false),
                    c_resid = table.Column<string>(type: "TEXT", nullable: false),
                    nace_r2 = table.Column<string>(type: "TEXT", nullable: false),
                    unit = table.Column<string>(type: "TEXT", nullable: false),
                    geo = table.Column<string>(type: "TEXT", nullable: false),
                    time_period = table.Column<string>(type: "TEXT", nullable: false),
                    nights = table.Column<decimal>(type: "TEXT", nullable: true),
                    arrivals = table.Column<decimal>(type: "TEXT", nullable: true),
                    average_stay_nights = table.Column<decimal>(type: "TEXT", nullable: true),
                    nights_flags = table.Column<string>(type: "TEXT", nullable: true),
                    arrivals_flags = table.Column<string>(type: "TEXT", nullable: true),
                    nights_raw_value = table.Column<string>(type: "TEXT", nullable: true),
                    arrivals_raw_value = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_analytics_monthly_length_of_stay", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_analytics_monthly_length_of_stay_geo_time_period",
                table: "analytics_monthly_length_of_stay",
                columns: new[] { "geo", "time_period" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "analytics_monthly_length_of_stay");
        }
    }
}
