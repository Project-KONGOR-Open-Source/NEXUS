using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MERRICK.Database.Migrations
{
    /// <inheritdoc />
    public partial class CreateInitialEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clans",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Tag = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    TimestampCreated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clans", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Tokens",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Purpose = table.Column<int>(type: "int", nullable: false),
                    EmailAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TimestampCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TimestampConsumed = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Data = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tokens", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmailAddress = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    RoleID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SRPPasswordSalt = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    SRPPasswordHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    PBKDF2PasswordHash = table.Column<string>(type: "nvarchar(84)", maxLength: 84, nullable: false),
                    TimestampCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TimestampLastActive = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GoldCoins = table.Column<int>(type: "int", nullable: false),
                    SilverCoins = table.Column<int>(type: "int", nullable: false),
                    PlinkoTickets = table.Column<int>(type: "int", nullable: false),
                    TotalLevel = table.Column<int>(type: "int", nullable: false),
                    TotalExperience = table.Column<int>(type: "int", nullable: false),
                    OwnedStoreItems = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Users_Roles_RoleID",
                        column: x => x.RoleID,
                        principalTable: "Roles",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    UserID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    IsMain = table.Column<bool>(type: "bit", nullable: false),
                    ClanID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ClanTier = table.Column<int>(type: "int", nullable: false),
                    AscensionLevel = table.Column<int>(type: "int", nullable: false),
                    TimestampCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TimestampLastActive = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AutoConnectChatChannels = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SelectedStoreItems = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IPAddressCollection = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MACAddressCollection = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SystemInformationCollection = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SystemInformationHashCollection = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Accounts_Clans_ClanID",
                        column: x => x.ClanID,
                        principalTable: "Clans",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_Accounts_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AccountBannedAccounts",
                columns: table => new
                {
                    AccountID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BannedAccountID = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountBannedAccounts", x => x.AccountID);
                    table.ForeignKey(
                        name: "FK_AccountBannedAccounts_Accounts_AccountID",
                        column: x => x.AccountID,
                        principalTable: "Accounts",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccountBannedAccounts_Accounts_BannedAccountID",
                        column: x => x.BannedAccountID,
                        principalTable: "Accounts",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "AccountFriendAccounts",
                columns: table => new
                {
                    AccountID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FriendAccountID = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountFriendAccounts", x => x.AccountID);
                    table.ForeignKey(
                        name: "FK_AccountFriendAccounts_Accounts_AccountID",
                        column: x => x.AccountID,
                        principalTable: "Accounts",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccountFriendAccounts_Accounts_FriendAccountID",
                        column: x => x.FriendAccountID,
                        principalTable: "Accounts",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "AccountIgnoredAccounts",
                columns: table => new
                {
                    AccountID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IgnoredAccountID = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountIgnoredAccounts", x => x.AccountID);
                    table.ForeignKey(
                        name: "FK_AccountIgnoredAccounts_Accounts_AccountID",
                        column: x => x.AccountID,
                        principalTable: "Accounts",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccountIgnoredAccounts_Accounts_IgnoredAccountID",
                        column: x => x.IgnoredAccountID,
                        principalTable: "Accounts",
                        principalColumn: "ID");
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "ID", "Name" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-00002b88dfe2"), "USER" },
                    { new Guid("00000000-0000-0000-0000-0000f4a5e3d3"), "ADMINISTRATOR" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountBannedAccounts_AccountID_BannedAccountID",
                table: "AccountBannedAccounts",
                columns: new[] { "AccountID", "BannedAccountID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccountBannedAccounts_BannedAccountID",
                table: "AccountBannedAccounts",
                column: "BannedAccountID");

            migrationBuilder.CreateIndex(
                name: "IX_AccountFriendAccounts_AccountID_FriendAccountID",
                table: "AccountFriendAccounts",
                columns: new[] { "AccountID", "FriendAccountID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccountFriendAccounts_FriendAccountID",
                table: "AccountFriendAccounts",
                column: "FriendAccountID");

            migrationBuilder.CreateIndex(
                name: "IX_AccountIgnoredAccounts_AccountID_IgnoredAccountID",
                table: "AccountIgnoredAccounts",
                columns: new[] { "AccountID", "IgnoredAccountID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccountIgnoredAccounts_IgnoredAccountID",
                table: "AccountIgnoredAccounts",
                column: "IgnoredAccountID");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_ClanID",
                table: "Accounts",
                column: "ClanID");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Name",
                table: "Accounts",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_UserID",
                table: "Accounts",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Clans_Name_Tag",
                table: "Clans",
                columns: new[] { "Name", "Tag" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Name",
                table: "Roles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_EmailAddress",
                table: "Users",
                column: "EmailAddress",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleID",
                table: "Users",
                column: "RoleID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountBannedAccounts");

            migrationBuilder.DropTable(
                name: "AccountFriendAccounts");

            migrationBuilder.DropTable(
                name: "AccountIgnoredAccounts");

            migrationBuilder.DropTable(
                name: "Tokens");

            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropTable(
                name: "Clans");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
