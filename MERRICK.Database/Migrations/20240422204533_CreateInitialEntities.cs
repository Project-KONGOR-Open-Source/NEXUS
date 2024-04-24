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
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
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
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
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
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Purpose = table.Column<int>(type: "int", nullable: false),
                    EmailAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TimestampCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TimestampConsumed = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Value = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmailAddress = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    RoleID = table.Column<int>(type: "int", nullable: false),
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
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    IsMain = table.Column<bool>(type: "bit", nullable: false),
                    ClanID = table.Column<int>(type: "int", nullable: true),
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
                    { 1, "KONGOR", "K", new DateTime(2024, 4, 22, 20, 45, 32, 815, DateTimeKind.Utc).AddTicks(6033) },
                    { 2, "Project KONGOR Developers", ".NET", new DateTime(2024, 4, 22, 20, 45, 32, 815, DateTimeKind.Utc).AddTicks(6049) },
                    { 3, "Project KONGOR", "PK", new DateTime(2024, 4, 22, 20, 45, 32, 815, DateTimeKind.Utc).AddTicks(6052) },
                    { 4, "Project KONGOR Open-Source", "PKOS", new DateTime(2024, 4, 22, 20, 45, 32, 815, DateTimeKind.Utc).AddTicks(6053) }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "ID", "Name" },
                values: new object[,]
                {
                    { 1, "ADMINISTRATOR" },
                    { 2, "USER" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "ID", "EmailAddress", "GoldCoins", "OwnedStoreItems", "PBKDF2PasswordHash", "PlinkoTickets", "RoleID", "SRPPasswordHash", "SRPPasswordSalt", "SilverCoins", "TimestampCreated", "TimestampLastActive", "TotalExperience", "TotalLevel" },
                values: new object[] { 1, "project.kongor@proton.me", 5555555, "[\"ai.custom_icon:1\",\"av.Flamboyant\",\"c.cat_courier\",\"cc.frostburnlogo\",\"cr.Punk Creep\",\"cs.frostburnlogo\",\"m.Super-Taunt\",\"sc.paragon_circle_upgrade\",\"t.Dumpster_Taunt\",\"te.Punk TP\",\"w.8bit_ward\"]", "AQAAAAIAAYagAAAAEMUkpLAr01NjkKRPaXCyTa17nlOdPKJucn5QYur+wQBTDKCpgsAcREenK+pGJPBCRw==", 5555555, 1, "fe6f16b0ecb80f6b2bc95d68420fd13afef0c895172a81819870660208ac221a", "861c37ec6d049d92cc1c67d195b414f26b572a56358272af3e9c06fcd9bfa053", 555555555, new DateTime(2024, 4, 22, 20, 45, 32, 815, DateTimeKind.Utc).AddTicks(5963), new DateTime(2024, 4, 22, 20, 45, 32, 815, DateTimeKind.Utc).AddTicks(5966), 22211666, 666 });

            migrationBuilder.InsertData(
                table: "Accounts",
                columns: new[] { "ID", "AscensionLevel", "AutoConnectChatChannels", "ClanID", "ClanTier", "IPAddressCollection", "IsMain", "MACAddressCollection", "Name", "SelectedStoreItems", "SystemInformationCollection", "SystemInformationHashCollection", "TimestampCreated", "TimestampJoinedClan", "TimestampLastActive", "Type", "UserID" },
                values: new object[,]
                {
                    { 1, 666, "[\"KONGOR\",\"TERMINAL\"]", 2, 3, "[]", true, "[]", "KONGOR", "[\"ai.custom_icon:1\",\"av.Flamboyant\",\"c.cat_courier\",\"cc.frostburnlogo\",\"cr.Punk Creep\",\"cs.frostburnlogo\",\"m.Super-Taunt\",\"sc.paragon_circle_upgrade\",\"t.Dumpster_Taunt\",\"te.Punk TP\",\"w.8bit_ward\"]", "[]", "[]", new DateTime(2024, 4, 22, 20, 45, 32, 815, DateTimeKind.Utc).AddTicks(6115), new DateTime(2024, 4, 22, 20, 45, 32, 815, DateTimeKind.Utc).AddTicks(6109), new DateTime(2024, 4, 22, 20, 45, 32, 815, DateTimeKind.Utc).AddTicks(6115), 5, 1 },
                    { 2, 666, "[\"KONGOR\",\"TERMINAL\"]", 1, 3, "[]", false, "[]", "ONGOR", "[\"ai.custom_icon:1\",\"av.Flamboyant\",\"c.cat_courier\",\"cc.frostburnlogo\",\"cr.Punk Creep\",\"cs.frostburnlogo\",\"m.Super-Taunt\",\"sc.paragon_circle_upgrade\",\"t.Dumpster_Taunt\",\"te.Punk TP\",\"w.8bit_ward\"]", "[]", "[]", new DateTime(2024, 4, 22, 20, 45, 32, 815, DateTimeKind.Utc).AddTicks(6123), new DateTime(2024, 4, 22, 20, 45, 32, 815, DateTimeKind.Utc).AddTicks(6121), new DateTime(2024, 4, 22, 20, 45, 32, 815, DateTimeKind.Utc).AddTicks(6124), 5, 1 },
                    { 3, 666, "[\"KONGOR\",\"TERMINAL\"]", 1, 2, "[]", false, "[]", "GOPO", "[\"ai.custom_icon:1\",\"av.Flamboyant\",\"c.cat_courier\",\"cc.frostburnlogo\",\"cr.Punk Creep\",\"cs.frostburnlogo\",\"m.Super-Taunt\",\"sc.paragon_circle_upgrade\",\"t.Dumpster_Taunt\",\"te.Punk TP\",\"w.8bit_ward\"]", "[]", "[]", new DateTime(2024, 4, 22, 20, 45, 32, 815, DateTimeKind.Utc).AddTicks(6129), new DateTime(2024, 4, 22, 20, 45, 32, 815, DateTimeKind.Utc).AddTicks(6128), new DateTime(2024, 4, 22, 20, 45, 32, 815, DateTimeKind.Utc).AddTicks(6130), 5, 1 },
                    { 4, 666, "[\"KONGOR\",\"TERMINAL\"]", 1, 2, "[]", false, "[]", "Xen0byte", "[\"ai.custom_icon:1\",\"av.Flamboyant\",\"c.cat_courier\",\"cc.frostburnlogo\",\"cr.Punk Creep\",\"cs.frostburnlogo\",\"m.Super-Taunt\",\"sc.paragon_circle_upgrade\",\"t.Dumpster_Taunt\",\"te.Punk TP\",\"w.8bit_ward\"]", "[]", "[]", new DateTime(2024, 4, 22, 20, 45, 32, 815, DateTimeKind.Utc).AddTicks(6132), new DateTime(2024, 4, 22, 20, 45, 32, 815, DateTimeKind.Utc).AddTicks(6131), new DateTime(2024, 4, 22, 20, 45, 32, 815, DateTimeKind.Utc).AddTicks(6133), 5, 1 },
                    { 5, 666, "[\"KONGOR\",\"TERMINAL\"]", 1, 2, "[]", false, "[]", "HOST", "[\"ai.custom_icon:1\",\"av.Flamboyant\",\"c.cat_courier\",\"cc.frostburnlogo\",\"cr.Punk Creep\",\"cs.frostburnlogo\",\"m.Super-Taunt\",\"sc.paragon_circle_upgrade\",\"t.Dumpster_Taunt\",\"te.Punk TP\",\"w.8bit_ward\"]", "[]", "[]", new DateTime(2024, 4, 22, 20, 45, 32, 815, DateTimeKind.Utc).AddTicks(6162), new DateTime(2024, 4, 22, 20, 45, 32, 815, DateTimeKind.Utc).AddTicks(6134), new DateTime(2024, 4, 22, 20, 45, 32, 815, DateTimeKind.Utc).AddTicks(6162), 2, 1 }
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
