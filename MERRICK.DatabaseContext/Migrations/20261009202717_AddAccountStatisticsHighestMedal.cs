using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MERRICK.DatabaseContext.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountStatisticsHighestMedal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HighestMedal",
                schema: "stat",
                table: "AccountStatistics",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HighestMedal",
                schema: "stat",
                table: "AccountStatistics");
        }
    }
}
