using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MERRICK.DatabaseContext.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchParticipantMasteryProgression : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MasteryProgression",
                schema: "stat",
                table: "MatchParticipantStatistics",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MasteryProgression",
                schema: "stat",
                table: "MatchParticipantStatistics");
        }
    }
}
