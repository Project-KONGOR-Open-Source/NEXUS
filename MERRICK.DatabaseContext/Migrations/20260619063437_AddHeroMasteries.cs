using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MERRICK.DatabaseContext.Migrations
{
    /// <inheritdoc />
    public partial class AddHeroMasteries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Masteries",
                schema: "stat",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountID = table.Column<int>(type: "int", nullable: false),
                    HeroExperiences = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Masteries", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Masteries_Accounts_AccountID",
                        column: x => x.AccountID,
                        principalSchema: "core",
                        principalTable: "Accounts",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MasteryRewards",
                schema: "stat",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountID = table.Column<int>(type: "int", nullable: false),
                    ClaimedLevels = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MasteryRewards", x => x.ID);
                    table.ForeignKey(
                        name: "FK_MasteryRewards_Accounts_AccountID",
                        column: x => x.AccountID,
                        principalSchema: "core",
                        principalTable: "Accounts",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Masteries_AccountID",
                schema: "stat",
                table: "Masteries",
                column: "AccountID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MasteryRewards_AccountID",
                schema: "stat",
                table: "MasteryRewards",
                column: "AccountID",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Masteries",
                schema: "stat");

            migrationBuilder.DropTable(
                name: "MasteryRewards",
                schema: "stat");
        }
    }
}
