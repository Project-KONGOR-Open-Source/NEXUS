using MERRICK.DatabaseContext.Entities.Statistics;

namespace ASPIRE.Tests.KONGOR.MasterServer;

public class SeedMatchHistoryTests
{
    // USAGE: dotnet test --filter SeedMatchHistory
    // This is a manual script wrapped as a test to populate the DB with fake history.

    [Test]
    public async Task SeedMatchHistory_Full5v5()
    {
        // Setup Dependency Injection to get the DbContext
        ServiceCollection services = new ServiceCollection();
        
        string connectionString = "Server=127.0.0.1,55678;Database=development;User Id=sa;Password=MerrickDevPassword2025;TrustServerCertificate=True;Connection Timeout=60;"; 
        
        services.AddDbContext<MerrickContext>(options =>
            options.UseSqlServer(connectionString, sqlOptions => 
                sqlOptions.MigrationsHistoryTable("MigrationsHistory", "meta")));

        ServiceProvider provider = services.BuildServiceProvider();
        MerrickContext context = provider.GetRequiredService<MerrickContext>();

        // FORCE REMEDIATION: Unconditionally drop table and clear migration history to ensure clean state
        try 
        {
            Console.WriteLine("[REMEDIATION] Forcing Drop of AccountStatistics table...");
            try { await context.Database.ExecuteSqlRawAsync("IF OBJECT_ID('data.AccountStatistics', 'U') IS NOT NULL DROP TABLE data.AccountStatistics"); }
            catch (Exception ex) { Console.WriteLine($"[REMEDIATION] Drop Table Failed (Non-Critical): {ex.Message}"); }
            
            Console.WriteLine("[REMEDIATION] Clearing Match History...");
            try { await context.Database.ExecuteSqlRawAsync("DELETE FROM data.PlayerStatistics"); }
            catch (Exception ex) { Console.WriteLine($"[REMEDIATION] Delete PlayerStats Failed: {ex.Message}"); }
            
            Console.WriteLine("[REMEDIATION] Clearing Migration History...");
            // Real configuration uses [meta].[MigrationsHistory]
            try { await context.Database.ExecuteSqlRawAsync("DELETE FROM [meta].[MigrationsHistory] WHERE [MigrationId] = '20260111031716_AddAccountStatisticsTable'"); }
            catch (Exception ex) { Console.WriteLine($"[REMEDIATION] Delete History Failed (Non-Critical): {ex.Message}"); }
            
            Console.WriteLine("[REMEDIATION] Re-applying latest migration...");
            await context.Database.MigrateAsync();
            Console.WriteLine("[REMEDIATION] Database is now up to date.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DEBUG/REMEDIATION] Failed: {ex.Message}");
            throw;
        } 

        Random random = new Random();
        int baseMatchId = 9000000 + random.Next(1, 100000);
        int midwarsId = baseMatchId + 1;
        int rankedId = baseMatchId + 2;

        List<Account> guestAccounts = new List<Account>();

        // 1. Fetch ALL Guest Accounts (01-10)
        for (int i = 1; i <= 10; i++)
        {
            string nickname = $"GUEST-{i:D2}";
            Account? account = await context.Accounts.FirstOrDefaultAsync(a => a.Name == nickname);
            
            if (account == null)
            {
                Console.WriteLine($"Account {nickname} not found. Skipping.");
                continue;
            }
            
            guestAccounts.Add(account);

            // 2. Seed AccountStatistics if missing
            if (!await context.AccountStatistics.AnyAsync(s => s.AccountID == account.ID))
            {
                context.AccountStatistics.Add(new AccountStatistics
                {
                    AccountID = account.ID,
                    MatchesPlayed = 2,
                    MatchesWon = 1,
                    MatchesLost = 1,
                    SkillRating = 1500.0 + (random.NextDouble() * 100 - 50)
                });
                Console.WriteLine($"Seeded AccountStatistics for {nickname}.");
            }
        }

        // 3. Create Matches
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

        context.MatchStatistics.AddRange(midwarsMatch, rankedMatch);

        // List of valid heroes confirmed to exist in HeroDefinitions.cs
        // 114: Armadon, 115: Behemoth, 116: Hammerstorm, 120: Predator, 121: Jeraziah
        // 122: Panda, 123: Rampage, 124: Tundra, 125: Gladiator, 153: Accursed
        List<uint> validHeroes = new List<uint> { 114, 115, 116, 120, 121, 122, 123, 124, 125, 153 };

        // 4. Create PlayerStatistics for 5v5
        foreach (Account account in guestAccounts)
        {
            // Parse ID (GUEST-01 -> 1)
            int guestNum = int.Parse(account.Name.Substring(6));
            int team = guestNum <= 5 ? 1 : 2; // 1-5 Team 1, 6-10 Team 2

            // Items (Randomly assigned from a small list)
            List<string> possibleItems = new List<string>
            {
                "Item_LoggersHatchet",
                "Item_ManaBattery",
                "Item_CrushingClaws",
                "Item_DuckBoots",
                "Item_MarkOfTheNovice",
                "Item_RunesOfTheBlight",
                "Item_HealthPotion"
            };
            List<string> inventory = possibleItems.OrderBy(x => random.Next()).Take(3).ToList();

            
            // Midwars Player
            context.PlayerStatistics.Add(new PlayerStatistics
            {
                MatchID = midwarsId,
                AccountID = account.ID,
                AccountName = account.Name,
                Team = team,
                LobbyPosition = guestNum - 1,
                GroupNumber = 0,
                ClanID = null,
                ClanTag = null,
                Benefit = 0,
                HeroProductID = validHeroes[(guestNum - 1) % validHeroes.Count], // Different hero for everyone
                Inventory = new List<string>(inventory),
                Win = team == 1 ? 1 : 0,
                Loss = team == 1 ? 0 : 1,
                HeroKills = random.Next(0, 15),
                HeroDeaths = random.Next(0, 10),
                HeroAssists = random.Next(0, 20),
                HeroLevel = 25,
                Gold = 10000,
                SecondsPlayed = 1200,
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
                HeroDamage = 10000,
                GoldFromHeroKills = 2000,
                HeroExperience = 15000,
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
                BuildingsRazed = 1,
                ExperienceFromBuildings = 1000,
                GoldFromBuildings = 1200,
                Denies = 5,
                ExperienceDenied = 200,
                GoldSpent = 9000,
                Experience = 20000,
                Actions = 5000,
                ConsumablesPurchased = 2,
                WardsPlaced = 1,
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
                TimeEarningExperience = 1100
            });

            // Ranked Player
            // Assign specific valid heroes to ensure icons display correctly
            // 112: Ra, 153: Accursed, 122: Panda, 116: Hammerstorm, 123: Rampage
            uint rankedHeroId = validHeroes[(guestNum - 1) % validHeroes.Count];

            context.PlayerStatistics.Add(new PlayerStatistics
            {
                MatchID = rankedId,
                AccountID = account.ID,
                AccountName = account.Name,
                Team = team,
                LobbyPosition = guestNum - 1,
                GroupNumber = 0,
                ClanID = null,
                ClanTag = null,
                Benefit = 0,
                HeroProductID = rankedHeroId,
                Inventory = new List<string>(inventory),
                Win = team == 2 ? 1 : 0,
                Loss = team == 2 ? 0 : 1,
                HeroKills = random.Next(0, 15),
                HeroDeaths = random.Next(0, 10),
                HeroAssists = random.Next(0, 20),
                HeroLevel = 25,
                Gold = 15000,
                SecondsPlayed = 1800,
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
                HeroDamage = 12000,
                GoldFromHeroKills = 2500,
                HeroExperience = 18000,
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
                GoldSpent = 14000,
                Experience = 24000,
                Actions = 6000,
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
            });
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"Seeded 5v5 matches for {guestAccounts.Count} accounts. Midwars ({midwarsId}), Ranked ({rankedId})");
    }
}
