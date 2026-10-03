using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MERRICK.DatabaseContext.Migrations
{
    /// <inheritdoc />
    public partial class AddConfigurationBackup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConfigurationBackups",
                schema: "misc",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountID = table.Column<int>(type: "int", nullable: false),
                    UseCloud = table.Column<bool>(type: "bit", nullable: false),
                    AutomaticUpload = table.Column<bool>(type: "bit", nullable: false),
                    FileModificationTime = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfigurationArchive = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigurationBackups", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ConfigurationBackups_Accounts_AccountID",
                        column: x => x.AccountID,
                        principalSchema: "core",
                        principalTable: "Accounts",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConfigurationBackups_AccountID",
                schema: "misc",
                table: "ConfigurationBackups",
                column: "AccountID",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfigurationBackups",
                schema: "misc");
        }
    }
}
