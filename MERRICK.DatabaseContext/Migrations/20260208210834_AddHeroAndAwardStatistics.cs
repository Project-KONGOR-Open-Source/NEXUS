using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MERRICK.DatabaseContext.Migrations
{
    /// <inheritdoc />
    public partial class AddHeroAndAwardStatistics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AwardStatistics",
                schema: "stat",
                table: "AccountStatistics",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "{}");

            migrationBuilder.AddColumn<string>(
                name: "HeroStatistics",
                schema: "stat",
                table: "AccountStatistics",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "{}");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AwardStatistics",
                schema: "stat",
                table: "AccountStatistics");

            migrationBuilder.DropColumn(
                name: "HeroStatistics",
                schema: "stat",
                table: "AccountStatistics");
        }
    }
}
