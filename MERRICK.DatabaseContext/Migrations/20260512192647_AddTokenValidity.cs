using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MERRICK.DatabaseContext.Migrations
{
    /// <inheritdoc />
    public partial class AddTokenValidity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 24 Hours, Expressed In Ticks (10,000,000 Ticks Per Second).
            // Used As The Default Validity For Both New Rows Inserted Without An Explicit Value And For Existing Rows Backfilled By This Migration.
            const long defaultValidityTicks = 864_000_000_000L;

            migrationBuilder.AddColumn<long>(
                name: "Validity",
                schema: "auth",
                table: "Tokens",
                type: "bigint",
                nullable: false,
                defaultValue: defaultValidityTicks);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Validity",
                schema: "auth",
                table: "Tokens");
        }
    }
}
