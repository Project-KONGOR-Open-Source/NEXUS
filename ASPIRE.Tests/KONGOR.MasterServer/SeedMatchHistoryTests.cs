using MERRICK.DatabaseContext.Entities.Statistics;

namespace ASPIRE.Tests.KONGOR.MasterServer;

public class SeedMatchHistoryTests
{
    // USAGE: dotnet test --filter SeedMatchHistory
    // This is a manual script wrapped as a test to populate the DB with fake history.

    [Test]
    public async Task SeedMatchHistory_ForGuest05()
    {
        // Setup Dependency Injection to get the DbContext
        ServiceCollection services = new ServiceCollection();
        
        // Use the connection string for your local SQL Server
        // Port 55678 is mapped to 1433 in the running container (ID 8799d12eb9d3)
        // Switch to 127.0.0.1 to avoid localhost resolution timeouts and increase Connection Timeout
        string connectionString = "Server=127.0.0.1,55678;Database=development;User Id=sa;Password=MerrickDevPassword2025;TrustServerCertificate=True;Connection Timeout=60;"; 
        
        services.AddDbContext<MerrickContext>(options =>
            options.UseSqlServer(connectionString));

        ServiceProvider provider = services.BuildServiceProvider();
        MerrickContext context = provider.GetRequiredService<MerrickContext>();

        string nickname = "GUEST-05";
        Account? account = await context.Accounts.FirstOrDefaultAsync(a => a.Name == nickname);
        
        if (account == null)
        {
            Console.WriteLine($"Account {nickname} not found. Skipping seed.");
            return;
        }

        Console.WriteLine($"Found Account: {account.Name} (ID: {account.ID})");

        Random random = new Random();
        int baseMatchId = 9000000 + random.Next(1, 100000); 

        // 1. Create Midwars Match
        int midwarsId = baseMatchId + 1;
        MatchStatistics midwarsMatch = new MatchStatistics
        {
            MatchID = midwarsId,
            ServerID = 1,
            HostAccountName = "System",
            Map = "midwars", 
            MapVersion = "1.0",
            TimePlayed = 1200,
            FileSize = 1000,
            FileName = $"M{midwarsId}.honreplay",
            ConnectionState = 0,
            Version = "4.10.0",
            AveragePSR = 1500,
            AveragePSRTeamOne = 1500,
            AveragePSRTeamTwo = 1500,
            GameMode = "midwars",
            ScoreTeam1 = 50,
            ScoreTeam2 = 40,
            TeamScoreGoal = 0,
            PlayerScoreGoal = 0,
            NumberOfRounds = 1,
            ReleaseStage = "Live",
            BannedHeroes = "",
            TimestampRecorded = DateTimeOffset.UtcNow.AddHours(-1),
            AwardMostAnnihilations = 0,
            AwardMostQuadKills = 0,
            AwardLargestKillStreak = 0,
            AwardMostSmackdowns = 0,
            AwardMostKills = 0,
            AwardMostAssists = 0,
            AwardLeastDeaths = 0,
            AwardMostBuildingDamage = 0,
            AwardMostWardsKilled = 0,
            AwardMostHeroDamageDealt = 0,
            AwardHighestCreepScore = 0
        };

        PlayerStatistics midwarsPlayer = new PlayerStatistics
        {
            MatchID = midwarsId,
            AccountID = account.ID,
            AccountName = account.Name,
            ClanID = null,
            ClanTag = null,
            Team = 1,
            LobbyPosition = 1,
            GroupNumber = 0,
            Benefit = 0,
            HeroProductID = 104, // Magmus
            Inventory = new List<string>(),
            Win = 1,
            Loss = 0,
            Disconnected = 0,
            Conceded = 0,
            Kicked = 0,
            PublicMatch = 0,
            PublicSkillRatingChange = 0,
            RankedMatch = 0,
            RankedSkillRatingChange = 0,
            SocialBonus = 0,
            UsedToken = 0,
            ConcedeVotes = 0,
            HeroKills = 10,
            HeroDamage = 15000,
            GoldFromHeroKills = 3000,
            HeroAssists = 15,
            HeroExperience = 20000,
            HeroDeaths = 2,
            Buybacks = 0,
            GoldLostToDeath = 500,
            SecondsDead = 60,
            TeamCreepKills = 50,
            TeamCreepDamage = 2000,
            TeamCreepGold = 1500,
            TeamCreepExperience = 2000,
            NeutralCreepKills = 10,
            NeutralCreepDamage = 500,
            NeutralCreepGold = 400,
            NeutralCreepExperience = 600,
            BuildingDamage = 1000,
            BuildingsRazed = 2,
            ExperienceFromBuildings = 1000,
            GoldFromBuildings = 1200,
            Denies = 5,
            ExperienceDenied = 200,
            Gold = 18000,
            GoldSpent = 17500,
            Experience = 25000,
            Actions = 5000,
            SecondsPlayed = 1200,
            HeroLevel = 25,
            ConsumablesPurchased = 2,
            WardsPlaced = 1,
            FirstBlood = 1,
            DoubleKill = 1,
            TripleKill = 0,
            QuadKill = 0,
            Annihilation = 0,
            KillStreak03 = 0,
            KillStreak04 = 0,
            KillStreak05 = 0,
            KillStreak06 = 0,
            KillStreak07 = 0,
            KillStreak08 = 0,
            KillStreak09 = 0,
            KillStreak10 = 0,
            KillStreak15 = 0,
            Smackdown = 0,
            Humiliation = 0,
            Nemesis = 0,
            Retribution = 0,
            Score = 0,
            GameplayStat0 = 0,
            GameplayStat1 = 0,
            GameplayStat2 = 0,
            GameplayStat3 = 0,
            GameplayStat4 = 0,
            GameplayStat5 = 0,
            GameplayStat6 = 0,
            GameplayStat7 = 0,
            GameplayStat8 = 0,
            GameplayStat9 = 0,
            TimeEarningExperience = 1100
        };

        // 2. Create Ranked Match
        int rankedId = baseMatchId + 2;
        MatchStatistics rankedMatch = new MatchStatistics
        {
            MatchID = rankedId,
            ServerID = 1,
            HostAccountName = "System",
            Map = "caldavar",
            MapVersion = "1.0",
            TimePlayed = 1800,
            FileSize = 1200,
            FileName = $"M{rankedId}.honreplay",
            ConnectionState = 0,
            Version = "4.10.0",
            AveragePSR = 1600,
            AveragePSRTeamOne = 1600,
            AveragePSRTeamTwo = 1600,
            GameMode = "picking",
            ScoreTeam1 = 20,
            ScoreTeam2 = 60,
            TeamScoreGoal = 0,
            PlayerScoreGoal = 0,
            NumberOfRounds = 1,
            ReleaseStage = "Live",
            BannedHeroes = "",
            TimestampRecorded = DateTimeOffset.UtcNow.AddHours(-2),
            AwardMostAnnihilations = 0,
            AwardMostQuadKills = 0,
            AwardLargestKillStreak = 0,
            AwardMostSmackdowns = 0,
            AwardMostKills = 0,
            AwardMostAssists = 0,
            AwardLeastDeaths = 0,
            AwardMostBuildingDamage = 0,
            AwardMostWardsKilled = 0,
            AwardMostHeroDamageDealt = 0,
            AwardHighestCreepScore = 0
        };

        PlayerStatistics rankedPlayer = new PlayerStatistics
        {
            MatchID = rankedId,
            AccountID = account.ID,
            AccountName = account.Name,
            ClanID = null,
            ClanTag = null,
            Team = 2,
            LobbyPosition = 6,
            GroupNumber = 0,
            Benefit = 0,
            HeroProductID = 227, // Midas
            Inventory = new List<string>(),
            Win = 1,
            Loss = 0,
            Disconnected = 0,
            Conceded = 0,
            Kicked = 0,
            PublicMatch = 0,
            PublicSkillRatingChange = 0,
            RankedMatch = 1,
            RankedSkillRatingChange = 5.0,
            SocialBonus = 0,
            UsedToken = 0,
            ConcedeVotes = 0,
            HeroKills = 8,
            HeroDamage = 12000,
            GoldFromHeroKills = 2500,
            HeroAssists = 8,
            HeroExperience = 18000,
            HeroDeaths = 4,
            Buybacks = 0,
            GoldLostToDeath = 800,
            SecondsDead = 120,
            TeamCreepKills = 150,
            TeamCreepDamage = 5000,
            TeamCreepGold = 6000,
            TeamCreepExperience = 5000,
            NeutralCreepKills = 20,
            NeutralCreepDamage = 1000,
            NeutralCreepGold = 800,
            NeutralCreepExperience = 900,
            BuildingDamage = 3000,
            BuildingsRazed = 4,
            ExperienceFromBuildings = 2000,
            GoldFromBuildings = 2400,
            Denies = 15,
            ExperienceDenied = 600,
            Gold = 22000,
            GoldSpent = 21000,
            Experience = 24000,
            Actions = 6000,
            SecondsPlayed = 1800,
            HeroLevel = 25,
            ConsumablesPurchased = 4,
            WardsPlaced = 0,
            FirstBlood = 0,
            DoubleKill = 0,
            TripleKill = 0,
            QuadKill = 0,
            Annihilation = 0,
            KillStreak03 = 0,
            KillStreak04 = 0,
            KillStreak05 = 0,
            KillStreak06 = 0,
            KillStreak07 = 0,
            KillStreak08 = 0,
            KillStreak09 = 0,
            KillStreak10 = 0,
            KillStreak15 = 0,
            Smackdown = 0,
            Humiliation = 0,
            Nemesis = 0,
            Retribution = 0,
            Score = 0,
            GameplayStat0 = 0,
            GameplayStat1 = 0,
            GameplayStat2 = 0,
            GameplayStat3 = 0,
            GameplayStat4 = 0,
            GameplayStat5 = 0,
            GameplayStat6 = 0,
            GameplayStat7 = 0,
            GameplayStat8 = 0,
            GameplayStat9 = 0,
            TimeEarningExperience = 1600
        };

        context.MatchStatistics.AddRange(midwarsMatch, rankedMatch);
        context.PlayerStatistics.AddRange(midwarsPlayer, rankedPlayer);
        await context.SaveChangesAsync();

        Console.WriteLine($"Seeded matches for {nickname}: Midwars ({midwarsId}), Ranked ({rankedId})");
    }
}
