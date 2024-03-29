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
                    Name = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Tag = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
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
                    TimestampJoinedClan = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AscensionLevel = table.Column<int>(type: "int", nullable: false),
                    TimestampCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TimestampLastActive = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AutoConnectChatChannels = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SelectedStoreItems = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IPAddressCollection = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MACAddressCollection = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SystemInformationCollection = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SystemInformationHashCollection = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BannedPeers = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FriendedPeers = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IgnoredPeers = table.Column<string>(type: "nvarchar(max)", nullable: true)
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

            migrationBuilder.InsertData(
                table: "Clans",
                columns: new[] { "ID", "Name", "Tag", "TimestampCreated" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000001"), "KONGOR", "K", new DateTime(2024, 3, 29, 23, 58, 45, 122, DateTimeKind.Utc).AddTicks(3439) },
                    { new Guid("00000000-0000-0000-0000-000000000002"), "Project KONGOR", "PK", new DateTime(2024, 3, 29, 23, 58, 45, 122, DateTimeKind.Utc).AddTicks(3443) },
                    { new Guid("00000000-0000-0000-0000-000000000003"), "Project KONGOR Open-Source", "PKOS", new DateTime(2024, 3, 29, 23, 58, 45, 122, DateTimeKind.Utc).AddTicks(3445) },
                    { new Guid("00000000-0000-0000-0000-000000000004"), "Project KONGOR Developers", "DEV", new DateTime(2024, 3, 29, 23, 58, 45, 122, DateTimeKind.Utc).AddTicks(3447) }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "ID", "Name" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-00002b88dfe2"), "USER" },
                    { new Guid("00000000-0000-0000-0000-0000f4a5e3d3"), "ADMINISTRATOR" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "ID", "EmailAddress", "GoldCoins", "OwnedStoreItems", "PBKDF2PasswordHash", "PlinkoTickets", "RoleID", "SRPPasswordHash", "SRPPasswordSalt", "SilverCoins", "TimestampCreated", "TimestampLastActive", "TotalExperience", "TotalLevel" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000001"), "project.kongor@proton.me", 5555555, "[\"ai.custom_icon:1\",\"av.Flamboyant\",\"c.cat_courier\",\"cc.frostburnlogo\",\"cr.Punk Creep\",\"cs.frostburnlogo\",\"m.Super-Taunt\",\"sc.paragon_circle_upgrade\",\"t.Dumpster_Taunt\",\"te.Punk TP\",\"w.8bit_ward\"]", "AQAAAAIAAYagAAAAEMUkpLAr01NjkKRPaXCyTa17nlOdPKJucn5QYur+wQBTDKCpgsAcREenK+pGJPBCRw==", 5555555, new Guid("00000000-0000-0000-0000-0000f4a5e3d3"), "fe6f16b0ecb80f6b2bc95d68420fd13afef0c895172a81819870660208ac221a", "861c37ec6d049d92cc1c67d195b414f26b572a56358272af3e9c06fcd9bfa053", 555555555, new DateTime(2024, 3, 29, 23, 58, 45, 122, DateTimeKind.Utc).AddTicks(3420), new DateTime(2024, 3, 29, 23, 58, 45, 122, DateTimeKind.Utc).AddTicks(3422), 22211666, 666 });

            migrationBuilder.InsertData(
                table: "Accounts",
                columns: new[] { "ID", "AscensionLevel", "AutoConnectChatChannels", "ClanID", "ClanTier", "IPAddressCollection", "IsMain", "MACAddressCollection", "Name", "SelectedStoreItems", "SystemInformationCollection", "SystemInformationHashCollection", "TimestampCreated", "TimestampJoinedClan", "TimestampLastActive", "Type", "UserID" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000001"), 666, "[\"KONGOR\",\"TERMINAL\"]", new Guid("00000000-0000-0000-0000-000000000004"), 3, "[]", true, "[]", "KONGOR", "[\"ai.custom_icon:1\",\"av.Flamboyant\",\"c.cat_courier\",\"cc.frostburnlogo\",\"cr.Punk Creep\",\"cs.frostburnlogo\",\"m.Super-Taunt\",\"sc.paragon_circle_upgrade\",\"t.Dumpster_Taunt\",\"te.Punk TP\",\"w.8bit_ward\"]", "[]", "[]", new DateTime(2024, 3, 29, 23, 58, 45, 124, DateTimeKind.Utc).AddTicks(6375), new DateTime(2024, 3, 29, 23, 58, 45, 124, DateTimeKind.Utc).AddTicks(6369), new DateTime(2024, 3, 29, 23, 58, 45, 124, DateTimeKind.Utc).AddTicks(6375), 5, new Guid("00000000-0000-0000-0000-000000000001") },
                    { new Guid("00000000-0000-0000-0000-000000000002"), 666, "[\"KONGOR\",\"TERMINAL\"]", new Guid("00000000-0000-0000-0000-000000000001"), 2, "[]", false, "[]", "GOPO", "[\"ai.custom_icon:1\",\"av.Flamboyant\",\"c.cat_courier\",\"cc.frostburnlogo\",\"cr.Punk Creep\",\"cs.frostburnlogo\",\"m.Super-Taunt\",\"sc.paragon_circle_upgrade\",\"t.Dumpster_Taunt\",\"te.Punk TP\",\"w.8bit_ward\"]", "[]", "[]", new DateTime(2024, 3, 29, 23, 58, 45, 124, DateTimeKind.Utc).AddTicks(6424), new DateTime(2024, 3, 29, 23, 58, 45, 124, DateTimeKind.Utc).AddTicks(6419), new DateTime(2024, 3, 29, 23, 58, 45, 124, DateTimeKind.Utc).AddTicks(6424), 5, new Guid("00000000-0000-0000-0000-000000000001") },
                    { new Guid("00000000-0000-0000-0000-000000000003"), 666, "[\"KONGOR\",\"TERMINAL\"]", new Guid("00000000-0000-0000-0000-000000000001"), 2, "[]", false, "[]", "Xen0byte", "[\"ai.custom_icon:1\",\"av.Flamboyant\",\"c.cat_courier\",\"cc.frostburnlogo\",\"cr.Punk Creep\",\"cs.frostburnlogo\",\"m.Super-Taunt\",\"sc.paragon_circle_upgrade\",\"t.Dumpster_Taunt\",\"te.Punk TP\",\"w.8bit_ward\"]", "[]", "[]", new DateTime(2024, 3, 29, 23, 58, 45, 124, DateTimeKind.Utc).AddTicks(6433), new DateTime(2024, 3, 29, 23, 58, 45, 124, DateTimeKind.Utc).AddTicks(6430), new DateTime(2024, 3, 29, 23, 58, 45, 124, DateTimeKind.Utc).AddTicks(6433), 5, new Guid("00000000-0000-0000-0000-000000000001") },
                    { new Guid("00000000-0000-0000-0000-000000000004"), 666, "[\"KONGOR\",\"TERMINAL\"]", new Guid("00000000-0000-0000-0000-000000000001"), 3, "[]", false, "[]", "ONGOR", "[\"ai.custom_icon:1\",\"av.Flamboyant\",\"c.cat_courier\",\"cc.frostburnlogo\",\"cr.Punk Creep\",\"cs.frostburnlogo\",\"m.Super-Taunt\",\"sc.paragon_circle_upgrade\",\"t.Dumpster_Taunt\",\"te.Punk TP\",\"w.8bit_ward\"]", "[]", "[]", new DateTime(2024, 3, 29, 23, 58, 45, 124, DateTimeKind.Utc).AddTicks(6439), new DateTime(2024, 3, 29, 23, 58, 45, 124, DateTimeKind.Utc).AddTicks(6436), new DateTime(2024, 3, 29, 23, 58, 45, 124, DateTimeKind.Utc).AddTicks(6439), 5, new Guid("00000000-0000-0000-0000-000000000001") }
                });

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
                name: "Accounts");

            migrationBuilder.DropTable(
                name: "Tokens");

            migrationBuilder.DropTable(
                name: "Clans");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
