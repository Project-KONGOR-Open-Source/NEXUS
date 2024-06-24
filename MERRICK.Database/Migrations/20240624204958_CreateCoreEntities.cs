using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MERRICK.Database.Migrations
{
    /// <inheritdoc />
    public partial class CreateCoreEntities : Migration
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
                name: "MatchStatistics",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServerID = table.Column<long>(type: "bigint", nullable: false),
                    HostAccountName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MatchID = table.Column<int>(type: "int", nullable: false),
                    Map = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MapVersion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TimePlayed = table.Column<int>(type: "int", nullable: false),
                    FileSize = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConnectionState = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AveragePSR = table.Column<int>(type: "int", nullable: false),
                    AveragePSRTeamOne = table.Column<int>(type: "int", nullable: false),
                    AveragePSRTeamTwo = table.Column<int>(type: "int", nullable: false),
                    GameMode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ScoreTeam1 = table.Column<int>(type: "int", nullable: false),
                    ScoreTeam2 = table.Column<int>(type: "int", nullable: false),
                    TeamScoreGoal = table.Column<int>(type: "int", nullable: false),
                    PlayerScoreGoal = table.Column<int>(type: "int", nullable: false),
                    NumberOfRounds = table.Column<int>(type: "int", nullable: false),
                    ReleaseStage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BannedHeroes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AwardMostAnnihilations = table.Column<int>(type: "int", nullable: false),
                    AwardMostQuadKills = table.Column<int>(type: "int", nullable: false),
                    AwardLargestKillStreak = table.Column<int>(type: "int", nullable: false),
                    AwardMostSmackdowns = table.Column<int>(type: "int", nullable: false),
                    AwardMostKills = table.Column<int>(type: "int", nullable: false),
                    AwardMostAssists = table.Column<int>(type: "int", nullable: false),
                    AwardLeastDeaths = table.Column<int>(type: "int", nullable: false),
                    AwardMostBuildingDamage = table.Column<int>(type: "int", nullable: false),
                    AwardMostWardsKilled = table.Column<int>(type: "int", nullable: false),
                    AwardMostHeroDamageDealt = table.Column<int>(type: "int", nullable: false),
                    AwardHighestCreepScore = table.Column<int>(type: "int", nullable: false),
                    SubmissionDebug = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchStatistics", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "PlayerStatistics",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MatchID = table.Column<int>(type: "int", nullable: false),
                    AccountID = table.Column<int>(type: "int", nullable: false),
                    AccountName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClanTag = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Team = table.Column<int>(type: "int", nullable: false),
                    LobbyPosition = table.Column<int>(type: "int", nullable: false),
                    GroupNumber = table.Column<int>(type: "int", nullable: false),
                    Benefit = table.Column<int>(type: "int", nullable: false),
                    HeroID = table.Column<long>(type: "bigint", nullable: false),
                    Win = table.Column<int>(type: "int", nullable: false),
                    Loss = table.Column<int>(type: "int", nullable: false),
                    Disconnected = table.Column<int>(type: "int", nullable: false),
                    Conceded = table.Column<int>(type: "int", nullable: false),
                    Kicked = table.Column<int>(type: "int", nullable: false),
                    PublicMatch = table.Column<int>(type: "int", nullable: false),
                    PublicSkillRatingChange = table.Column<double>(type: "float", nullable: false),
                    SoloRankedMatch = table.Column<int>(type: "int", nullable: false),
                    SoloRankedSkillRatingChange = table.Column<double>(type: "float", nullable: false),
                    TeamRankedMatch = table.Column<int>(type: "int", nullable: false),
                    TeamRankedSkillRatingChange = table.Column<double>(type: "float", nullable: false),
                    SocialBonus = table.Column<int>(type: "int", nullable: false),
                    UsedToken = table.Column<int>(type: "int", nullable: false),
                    ConcedeVotes = table.Column<int>(type: "int", nullable: false),
                    HeroKills = table.Column<int>(type: "int", nullable: false),
                    HeroDamage = table.Column<int>(type: "int", nullable: false),
                    GoldFromHeroKills = table.Column<int>(type: "int", nullable: false),
                    HeroAssists = table.Column<int>(type: "int", nullable: false),
                    HeroExperience = table.Column<int>(type: "int", nullable: false),
                    HeroDeaths = table.Column<int>(type: "int", nullable: false),
                    Buybacks = table.Column<int>(type: "int", nullable: false),
                    GoldLostToDeath = table.Column<int>(type: "int", nullable: false),
                    SecondsDead = table.Column<int>(type: "int", nullable: false),
                    TeamCreepKills = table.Column<int>(type: "int", nullable: false),
                    TeamCreepDamage = table.Column<int>(type: "int", nullable: false),
                    TeamCreepGold = table.Column<int>(type: "int", nullable: false),
                    TeamCreepExperience = table.Column<int>(type: "int", nullable: false),
                    NeutralCreepKills = table.Column<int>(type: "int", nullable: false),
                    NeutralCreepDamage = table.Column<int>(type: "int", nullable: false),
                    NeutralCreepGold = table.Column<int>(type: "int", nullable: false),
                    NeutralCreepExperience = table.Column<int>(type: "int", nullable: false),
                    BuildingDamage = table.Column<int>(type: "int", nullable: false),
                    BuildingsRazed = table.Column<int>(type: "int", nullable: false),
                    ExperienceFromBuildings = table.Column<int>(type: "int", nullable: false),
                    GoldFromBuildings = table.Column<int>(type: "int", nullable: false),
                    Denies = table.Column<int>(type: "int", nullable: false),
                    ExperienceDenied = table.Column<int>(type: "int", nullable: false),
                    Gold = table.Column<int>(type: "int", nullable: false),
                    GoldSpent = table.Column<int>(type: "int", nullable: false),
                    Experience = table.Column<int>(type: "int", nullable: false),
                    Actions = table.Column<int>(type: "int", nullable: false),
                    SecondsPlayed = table.Column<int>(type: "int", nullable: false),
                    HeroLevel = table.Column<int>(type: "int", nullable: false),
                    ConsumablesUsed = table.Column<int>(type: "int", nullable: false),
                    WardsPlaced = table.Column<int>(type: "int", nullable: false),
                    Bloodlust = table.Column<int>(type: "int", nullable: false),
                    DoubleKill = table.Column<int>(type: "int", nullable: false),
                    TripleKill = table.Column<int>(type: "int", nullable: false),
                    QuadKill = table.Column<int>(type: "int", nullable: false),
                    Annihilation = table.Column<int>(type: "int", nullable: false),
                    KillStreak3 = table.Column<int>(type: "int", nullable: false),
                    KillStreak4 = table.Column<int>(type: "int", nullable: false),
                    KillStreak5 = table.Column<int>(type: "int", nullable: false),
                    KillStreak6 = table.Column<int>(type: "int", nullable: false),
                    KillStreak7 = table.Column<int>(type: "int", nullable: false),
                    KillStreak8 = table.Column<int>(type: "int", nullable: false),
                    KillStreak9 = table.Column<int>(type: "int", nullable: false),
                    KillStreak10 = table.Column<int>(type: "int", nullable: false),
                    KillStreak15 = table.Column<int>(type: "int", nullable: false),
                    Smackdown = table.Column<int>(type: "int", nullable: false),
                    Humiliation = table.Column<int>(type: "int", nullable: false),
                    Nemesis = table.Column<int>(type: "int", nullable: false),
                    Retribution = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false),
                    GameplayStat0 = table.Column<double>(type: "float", nullable: false),
                    GameplayStat1 = table.Column<double>(type: "float", nullable: false),
                    GameplayStat2 = table.Column<double>(type: "float", nullable: false),
                    GameplayStat3 = table.Column<double>(type: "float", nullable: false),
                    GameplayStat4 = table.Column<double>(type: "float", nullable: false),
                    GameplayStat5 = table.Column<double>(type: "float", nullable: false),
                    GameplayStat6 = table.Column<double>(type: "float", nullable: false),
                    GameplayStat7 = table.Column<double>(type: "float", nullable: false),
                    GameplayStat8 = table.Column<double>(type: "float", nullable: false),
                    GameplayStat9 = table.Column<double>(type: "float", nullable: false),
                    TimeEarningExperience = table.Column<int>(type: "int", nullable: false),
                    Inventory = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerStatistics", x => x.ID);
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

            migrationBuilder.CreateTable(
                name: "HeroGuides",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    HeroName = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    HeroIdentifier = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    Intro = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(1500)", maxLength: 1500, nullable: false),
                    StartingItems = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    EarlyGameItems = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CoreItems = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    LuxuryItems = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    AbilityQueue = table.Column<string>(type: "nvarchar(750)", maxLength: 750, nullable: false),
                    AuthorID = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<float>(type: "real", nullable: false),
                    UpVotes = table.Column<int>(type: "int", nullable: false),
                    DownVotes = table.Column<int>(type: "int", nullable: false),
                    Public = table.Column<bool>(type: "bit", nullable: false),
                    Featured = table.Column<bool>(type: "bit", nullable: false),
                    TimestampCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TimestampLastUpdated = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HeroGuides", x => x.ID);
                    table.ForeignKey(
                        name: "FK_HeroGuides_Accounts_AuthorID",
                        column: x => x.AuthorID,
                        principalTable: "Accounts",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "ID", "Name" },
                values: new object[,]
                {
                    { 1, "ADMINISTRATOR" },
                    { 2, "USER" }
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
                name: "IX_HeroGuides_AuthorID",
                table: "HeroGuides",
                column: "AuthorID");

            migrationBuilder.CreateIndex(
                name: "IX_MatchStatistics_MatchID",
                table: "MatchStatistics",
                column: "MatchID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlayerStatistics_MatchID_AccountID",
                table: "PlayerStatistics",
                columns: new[] { "MatchID", "AccountID" },
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
                name: "HeroGuides");

            migrationBuilder.DropTable(
                name: "MatchStatistics");

            migrationBuilder.DropTable(
                name: "PlayerStatistics");

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
