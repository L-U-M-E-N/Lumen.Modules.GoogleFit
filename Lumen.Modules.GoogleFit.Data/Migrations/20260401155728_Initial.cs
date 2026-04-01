using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lumen.Modules.GoogleFit.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "googlefit");

            migrationBuilder.CreateTable(
                name: "DailySteps",
                schema: "googlefit",
                columns: table => new
                {
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Steps = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailySteps", x => x.Date);
                });

            migrationBuilder.CreateTable(
                name: "HourlySteps",
                schema: "googlefit",
                columns: table => new
                {
                    HourStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Steps = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HourlySteps", x => x.HourStart);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailySteps",
                schema: "googlefit");

            migrationBuilder.DropTable(
                name: "HourlySteps",
                schema: "googlefit");
        }
    }
}
