using MERRICK.DatabaseContext.Entities.Statistics;
using KONGOR.MasterServer.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.FileProviders; // For IFileProvider

namespace ASPIRE.Tests.KONGOR.MasterServer.Tests;

public class SeedMatchHistoryTests
{
    // =================================================================================================
    //  CONFIGURATION SECTION - UPDATE THESE VALUES TO CONTROL SEEDING
    // =================================================================================================
    
    // SET TO TRUE to wipe ALL existing match history (PlayerStatistics AND MatchStatistics) before seeding.
    // WARNING: This is destructive!
    private const bool CLEAR_HISTORY = false;

    // The number of randomized matches to generate for the 'SeedMatchHistory_RandomizedPublic' test.
    // value 100 = ~10 matches per guest account if using 10 guests.
    // scale matches if needed
    private const int MATCH_COUNT = 100;

    private static readonly string[] ItemPool = 
    [
        // Boots
        "Item_Steamboots", "Item_EnhancedMarchers", "Item_PostHaste", "Item_PlatedGreaves", "Item_Striders",
        // Weapons
        "Item_Dawnbringer", "Item_Sasuke", "Item_NullfireBlade", "Item_Thunderclaw", "Item_HarkonsBlade", "Item_SavageMace", "Item_Wingbow",
        // Defense/Utility
        "Item_BehemothsHeart", "Item_DaemonicBreastplate", "Item_FrostfieldPlate", "Item_Immunity", "Item_BarrierIdol", "Item_VoidTalisman",
        // Caster
        "Item_Sheepstick", "Item_Stormspirit", "Item_RestorationStone", "Item_GrimoireOfPower", "Item_SpellShards", "Item_NomesWisdom",
        // Support/Misc
        "Item_Astrolabe", "Item_Tablet", "Item_PortalKey", "Item_Bottle", "Item_BloodChalice", "Item_PowerSupply", "Item_LoggersHatchet"
    ];

    // =================================================================================================

    // USAGE: dotnet test --filter SeedMatchHistory
    // This is a manual script wrapped as a test to populate the DB with fake history.

    [Test]
    public async Task SeedMatchHistory_Full5v5()
    {
        // Setup Dependency Injection to get the DbContext
        ServiceCollection services = new();

        string connectionString = GetConnectionString();

        services.AddDbContext<MerrickContext>(options =>
            options.UseSqlServer(connectionString, sqlOptions =>
                sqlOptions.MigrationsHistoryTable("MigrationsHistory", "meta")));

        // Register HeroDefinitionService and dependencies
        services.AddLogging();
        services.AddSingleton<IHostEnvironment>(new TestHostEnvironment 
        { 
            ContentRootPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../KONGOR.MasterServer")) 
        });
        services.AddSingleton<IHeroDefinitionService, HeroDefinitionService>();

        ServiceProvider provider = services.BuildServiceProvider();
        MerrickContext context = provider.GetRequiredService<MerrickContext>();

        // FORCE REMEDIATION: Unconditionally drop table and clear migration history to ensure clean state
        try
        {
            Console.WriteLine("[REMEDIATION] Forcing Drop of AccountStatistics table...");
            try
            {
                await context.Database.ExecuteSqlRawAsync(
                    "IF OBJECT_ID('data.AccountStatistics', 'U') IS NOT NULL DROP TABLE data.AccountStatistics");
            }
            catch (Exception ex) { Console.WriteLine($"[REMEDIATION] Drop Table Failed (Non-Critical): {ex.Message}"); }

            Console.WriteLine("[REMEDIATION] Clearing Match History...");
            try { await context.Database.ExecuteSqlRawAsync("DELETE FROM stat.PlayerStatistics"); }
            catch (Exception ex) { Console.WriteLine($"[REMEDIATION] Delete PlayerStats Failed: {ex.Message}"); }

            Console.WriteLine("[REMEDIATION] Clearing Migration History...");
            // Real configuration uses [meta].[MigrationsHistory]
            try
            {
                await context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM [meta].[MigrationsHistory] WHERE [MigrationId] = '20260111031716_AddAccountStatisticsTable'");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[REMEDIATION] Delete History Failed (Non-Critical): {ex.Message}");
            }

            Console.WriteLine("[REMEDIATION] Re-applying latest migration...");
            await context.Database.MigrateAsync();
            Console.WriteLine("[REMEDIATION] Database is now up to date.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DEBUG/REMEDIATION] Failed: {ex.Message}");
            throw;
        }

        Random random = new();
        int baseMatchId = 9000000 + random.Next(1, 100000);
        int midwarsId = baseMatchId + 1;
        int rankedId = baseMatchId + 2;

        List<Account> guestAccounts = new();

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
                    SkillRating = 1500.0 + ((random.NextDouble() * 100) - 50)
                });
                Console.WriteLine($"Seeded AccountStatistics for {nickname}.");
            }
        }

        // 3. Create Matches
        MatchStatistics midwarsMatch = new()
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

        MatchStatistics rankedMatch = new()
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
        List<uint> validHeroes =
        [
            114,
            115,
            116,
            120,
            121,
            185,
            122,
            123,
            124,
            125,
            153
        ];

        // 4. Create PlayerStatistics for 5v5
        foreach (Account account in guestAccounts)
        {
            // Parse ID (GUEST-01 -> 1)
            int guestNum = int.Parse(account.Name.Substring(6));
            int team = guestNum <= 5 ? 1 : 2; // 1-5 Team 1, 6-10 Team 2

            // Items (Randomly assigned from a small list)
            List<string> possibleItems =
            [
                "Item_LoggersHatchet",
                "Item_ManaBattery",
                "Item_CrushingClaws",
                "Item_DuckBoots",
                "Item_MarkOfTheNovice",
                "Item_RunesOfTheBlight",
                "Item_HealthPotion"
            ];
            Random rnd = new Random();
            List<string> inventory = possibleItems.OrderBy(_ => rnd.Next()).Take(3).ToList();


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
                Inventory = [..inventory],
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
                Inventory = [..inventory],
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
        Console.WriteLine(
            $"Seeded 5v5 matches for {guestAccounts.Count} accounts. Midwars ({midwarsId}), Ranked ({rankedId})");
    }

    [Test]
    public async Task SeedMatchHistory_RandomizedPublic()
    {
        // USAGE: dotnet test --filter SeedMatchHistory_RandomizedPublic
        // Generates Public Matches with random heroes from Upgrades.JSON and mixed game modes.
        // SEE CONSTANTS AT TOP OF FILE FOR CONFIGURATION

        ServiceCollection services = new();
        string connectionString = GetConnectionString();

        services.AddDbContext<MerrickContext>(options =>
            options.UseSqlServer(connectionString, sqlOptions =>
                sqlOptions.MigrationsHistoryTable("MigrationsHistory", "meta")));

        // Register HeroDefinitionService and dependencies
        services.AddLogging();
        services.AddSingleton<IHostEnvironment>(new TestHostEnvironment 
        { 
            ContentRootPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../KONGOR.MasterServer")) 
        });
        services.AddSingleton<IHeroDefinitionService, HeroDefinitionService>();

        ServiceProvider provider = services.BuildServiceProvider();
        MerrickContext context = provider.GetRequiredService<MerrickContext>();


        if (CLEAR_HISTORY)
        {
            Console.WriteLine("[SEED] Uncomment to clear match history data...");
            //            Console.WriteLine("[SEED] Clearing Match History (PlayerStatistics)...");
            //            await context.Database.ExecuteSqlRawAsync("DELETE FROM stat.PlayerStatistics");
            //            await context.Database.ExecuteSqlRawAsync("DELETE FROM stat.MatchStatistics");
        }
        
        Random random = new();
        int baseMatchId = 9800000 + random.Next(1, 100000);

        List<Account> guestAccounts = new();
        for (int i = 1; i <= 10; i++)
        {
            string nickname = $"GUEST-{i:D2}";
            Account? account = await context.Accounts.AsNoTracking().FirstOrDefaultAsync(a => a.Name == nickname);
            if (account != null) guestAccounts.Add(account);
        }

        if (guestAccounts.Count == 0)
        {
            Console.WriteLine("No GUEST accounts found via DB. Please run generic seed first.");
            return;
        }

        // --- Dynamic Hero Loading via Service ---
        Console.WriteLine("[SEED] Initializing HeroDefinitionService to load valid heroes...");
        IHeroDefinitionService heroService = provider.GetRequiredService<IHeroDefinitionService>();
        
        // Get all heroes and filter out 0 (Legionnaire default) if desired, 
        // though 0 is technicaly valid. We filter > 0 to ensure variety?
        // Actually 0 is Legionnaire. We want him too.
        List<uint> validHeroes = heroService.GetAllHeroIds().ToList();
        
        Console.WriteLine($"[SEED] Found {validHeroes.Count} valid heroes via HeroDefinitionService.");
        
        if (validHeroes.Count == 0 || validHeroes is [0])
        {
             Console.WriteLine("[SEED] WARNING: Service returned no heroes or only default. Adding fallback list.");
             validHeroes.AddRange([111, 112, 10, 11, 12, 40, 44, 103, 250, 2501, 2502]); // Fallback
        }

        // --- Game Mode Scenarios ---
        var scenarios = new[]
        {
            new { Mode = "picking", Map = "caldavar", Type = "Ranked" },
            new { Mode = "picking", Map = "caldavar", Type = "Casual" }, // Renamed from Normal
            new { Mode = "midwars", Map = "midwars", Type = "Public" },
            new { Mode = "sd", Map = "caldavar", Type = "Casual" }, // Single Draft -> Casual
            new { Mode = "ar", Map = "caldavar", Type = "Casual" }  // All Random -> Casual
        };

        Console.WriteLine($"[SEED] Generating {MATCH_COUNT} Matches (Ranked/Public/Midwars) starting at ID {baseMatchId}...");

        for (int m = 0; m < MATCH_COUNT; m++)
        {
            int currentMatchId = baseMatchId + m;
            var scenario = scenarios[random.Next(scenarios.Length)];

            // --- Mock Product Catalog ---
            // Maps Product IDs to Strings. Ranges chosen to look realistic but arbitrary.
            Dictionary<string, uint> productCatalog = new()
            {
                { "AccountIcon_Default", 2500 }, { "AccountIcon_Legion", 2501 }, { "AccountIcon_Hellbourne", 2502 }, { "AccountIcon_GoldShield", 2503 },
                { "Courier_Default", 3000 }, { "Courier_Legion", 3001 }, { "Courier_Hellbourne", 3002 }, { "Courier_Panda", 3003 },
                { "Ward_Default", 4000 }, { "Ward_Eye", 4001 },
                { "Taunt_Default", 5000 }, { "Taunt_Baby", 5001 },
                { "Announcer_Default", 6000 }, { "Announcer_Flamboyant", 6001 }, { "Announcer_Badass", 6002 },
                { "ChatColor_Red", 7000 }, { "ChatColor_Orange", 7001 }, { "ChatColor_Yellow", 7002 }, { "ChatColor_Green", 7003 }, { "ChatColor_Blue", 7004 }, { "ChatColor_Purple", 7005 }
            };

            // Helper buffers for random selection
            string[] accountIcons = productCatalog.Keys.Where(k => k.StartsWith("AccountIcon")).ToArray();
            string[] couriers = productCatalog.Keys.Where(k => k.StartsWith("Courier")).ToArray();
            string[] wards = productCatalog.Keys.Where(k => k.StartsWith("Ward")).ToArray();
            string[] taunts = productCatalog.Keys.Where(k => k.StartsWith("Taunt")).ToArray();
            string[] announcers = productCatalog.Keys.Where(k => k.StartsWith("Announcer")).ToArray();
            string[] colors = productCatalog.Keys.Where(k => k.StartsWith("ChatColor")).ToArray();

            // Create Match
            MatchStatistics match = new()
            {
                MatchID = currentMatchId,
                ServerID = 1,
                HostAccountName = "System",
                Map = scenario.Map,
                MapVersion = "1.0",
                TimePlayed = 1500 + random.Next(-300, 300),
                FileSize = 1000,
                FileName = $"M{currentMatchId}.honreplay",
                ConnectionState = 0,
                Version = "4.10.1",
                AveragePSR = 1500,
                AveragePSRTeamOne = 1500,
                AveragePSRTeamTwo = 1500,
                GameMode = scenario.Mode,
                ScoreTeam1 = random.Next(10, 60),
                ScoreTeam2 = random.Next(10, 60),
                TeamScoreGoal = 0,
                PlayerScoreGoal = 0,
                NumberOfRounds = 1,
                ReleaseStage = "Live",
                BannedHeroes = "",
                TimestampRecorded = DateTimeOffset.UtcNow.AddMinutes(-(m * 30)),
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
            context.MatchStatistics.Add(match);

            // Create Players
            // Shuffle validHeroes for this match
            List<uint> shuffledHeroes = validHeroes.OrderBy(_ => random.Next()).Take(guestAccounts.Count).ToList();

            for (int p = 0; p < guestAccounts.Count; p++)
            {
                Account account = guestAccounts[p];
                int team = p < 5 ? 1 : 2;
                uint heroId = shuffledHeroes.Count > p ? shuffledHeroes[p] : 0; // Handled index out of range if heroes < accounts
                bool win = (team == 1 && match.ScoreTeam1 > match.ScoreTeam2) || (team == 2 && match.ScoreTeam2 > match.ScoreTeam1);

                // Determine Lobby Position/Type specific flags
                byte publicMatch = (byte)(scenario.Type == "Public" || scenario.Type == "Casual" ? 1 : 0);
                byte rankedMatch = (byte)(scenario.Type == "Ranked" ? 1 : 0);
                
                // Skill Change logic
                double skillChange = rankedMatch == 1 ? (win ? 5.0 : -5.0) : 0;
                
                // Random Selections
                string accIcon = accountIcons[random.Next(accountIcons.Length)];
                string courier = couriers[random.Next(couriers.Length)];
                string ward = wards[random.Next(wards.Length)];
                string taunt = taunts[random.Next(taunts.Length)];
                string announcer = announcers[random.Next(announcers.Length)];
                string color = colors[random.Next(colors.Length)];
                
                // --- ITEM RANDOMIZATION ---
                List<string> finalItems = SelectRandomItems(random);

                context.PlayerStatistics.Add(new PlayerStatistics
                {
                    MatchID = currentMatchId,
                    AccountID = account.ID,
                    AccountName = account.Name,
                    Team = team,
                    LobbyPosition = p,
                    GroupNumber = random.Next(0,
                        2), // Occasional grouping
                    ClanID = null,
                    ClanTag = null,
                    Benefit = random.Next(0,
                        5),
                    HeroProductID = heroId,

                    // Cosmetics
                    AlternativeAvatarName = "",
                    AlternativeAvatarProductID = 0,
                    WardProductName = ward,
                    WardProductID = productCatalog[ward],
                    TauntProductName = taunt,
                    TauntProductID = productCatalog[taunt],
                    AnnouncerProductName = announcer,
                    AnnouncerProductID = productCatalog[announcer],
                    CourierProductName = courier,
                    CourierProductID = productCatalog[courier],
                    AccountIconProductName = accIcon,
                    AccountIconProductID = productCatalog[accIcon],
                    ChatColourProductName = color,
                    ChatColourProductID = productCatalog[color],

                    // --- RANDOMIZED ITEMS ---
                    Inventory = finalItems,

                    // Generate history for these items + some starting consumables
                    ItemHistory = GenerateRandomItemHistory(finalItems,
                        random),
                    Win = win ? 1 : 0,
                    Loss = win ? 0 : 1,
                    
                    HeroKills = random.Next(0, 20),
                    HeroDeaths = random.Next(0, 15),
                    HeroAssists = random.Next(0, 25),
                    HeroLevel = random.Next(10, 25),
                    Gold = random.Next(5000, 15000),
                    SecondsPlayed = match.TimePlayed,
                    
                    // Logic Fields
                    Disconnected = random.Next(0, 20) == 0 ? 1 : 0, // Rare disconnect
                    Conceded = random.Next(0, 10) == 0 ? 1 : 0,
                    Kicked = 0,
                    PublicMatch = publicMatch,
                    PublicSkillRatingChange = 0, 
                    RankedMatch = rankedMatch,
                    RankedSkillRatingChange = skillChange,
                    SocialBonus = random.Next(0, 10),
                    UsedToken = random.Next(0, 5) == 0 ? 1 : 0,
                    ConcedeVotes = random.Next(0, 2),
                    
                    // Enriched Gameplay Stats (Missing Required Fields)
                    HeroDamage = random.Next(5000, 25000),
                    GoldFromHeroKills = random.Next(1000, 5000),
                    HeroExperience = random.Next(10000, 25000),
                    Buybacks = random.Next(0, 3),
                    GoldLostToDeath = random.Next(0, 1000),
                    SecondsDead = random.Next(0, 300),
                    TeamCreepKills = random.Next(100, 500),
                    TeamCreepDamage = random.Next(10000, 30000),
                    TeamCreepGold = random.Next(5000, 15000),
                    TeamCreepExperience = random.Next(5000, 15000),
                    NeutralCreepKills = random.Next(0, 100),
                    NeutralCreepDamage = random.Next(0, 10000),
                    NeutralCreepGold = random.Next(0, 3000),
                    NeutralCreepExperience = random.Next(0, 3000),
                    BuildingDamage = random.Next(0, 5000),
                    BuildingsRazed = random.Next(0, 5),
                    ExperienceFromBuildings = random.Next(0, 2000),
                    GoldFromBuildings = random.Next(0, 2000),
                    Denies = random.Next(0, 30),
                    ExperienceDenied = random.Next(0, 1000),
                    GoldSpent = random.Next(4000, 14000),
                    Experience = random.Next(10000, 30000),
                    Actions = random.Next(1000, 5000),
                    ConsumablesPurchased = random.Next(0, 10),
                    WardsPlaced = random.Next(0, 10),
                    
                    // Kill Events
                    FirstBlood = random.Next(0, 2),
                    DoubleKill = random.Next(0, 5),
                    TripleKill = random.Next(0, 3),
                    QuadKill = random.Next(0, 2),
                    Annihilation = random.Next(0, 2) == 0 ? 0 : 1, // Rare
                    KillStreak03 = random.Next(0, 5),
                    KillStreak04 = random.Next(0, 3),
                    KillStreak05 = random.Next(0, 2),
                    KillStreak06 = random.Next(0, 2),
                    KillStreak07 = random.Next(0, 2),
                    KillStreak08 = random.Next(0, 2),
                    KillStreak09 = random.Next(0, 1),
                    KillStreak10 = random.Next(0, 1),
                    KillStreak15 = random.Next(0, 20) == 0 ? 1 : 0, // Very Rare
                    
                    Smackdown = random.Next(0, 3),
                    Humiliation = random.Next(0, 2),
                    Nemesis = random.Next(0, 2),
                    Retribution = random.Next(0, 2),
                    
                    Score = random.Next(100, 500),
                    GameplayStat0 = random.NextDouble() * 100.0,
                    GameplayStat1 = random.NextDouble() * 50.0,
                    GameplayStat2 = random.NextDouble() * 10.0,
                    GameplayStat3 = random.NextDouble() * 5.0,
                    GameplayStat4 = random.NextDouble() * 1000.0,
                    GameplayStat5 = random.NextDouble() * 500.0,
                    GameplayStat6 = random.NextDouble() * 20.0,
                    GameplayStat7 = 0,
                    GameplayStat8 = 0,
                    GameplayStat9 = 0,
                    TimeEarningExperience = 0
                });
            }
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"[SEED] Successfully seeded {MATCH_COUNT} mixed-mode matches with dynamic heroes and items.");
    }
    
    private List<string> SelectRandomItems(Random random)
    {
        // Pick 3 to 6 unique items
        int count = random.Next(3, 7);
        return ItemPool.OrderBy(_ => random.Next()).Take(count).ToList();
    }

    private List<ItemEvent> GenerateRandomItemHistory(List<string> endItems, Random random)
    {
        List<ItemEvent> events =
        [
            new ItemEvent { ItemName = "Item_RunesOfTheBlight", GameTimeSeconds = -60, EventType = 0 },
            new ItemEvent { ItemName = "Item_HealthPotion", GameTimeSeconds = -60, EventType = 0 }

            // 2. Add end-game items at random times
        ];

        // 1. Starting Items (Consumables that disappear)

        // 2. Add end-game items at random times
        foreach (string item in endItems)
        {
            // Random purchase time between 0 and 20 mins (1200s)
            events.Add(new ItemEvent 
            { 
                ItemName = item, 
                GameTimeSeconds = random.Next(0, 1200), 
                EventType = 0 
            });
        }

        return events.OrderBy(e => e.GameTimeSeconds).ToList();
    }



    private string GetConnectionString()
    {
        // Priority 1: Full Connection String Override (Env)
        string? fullConn = Environment.GetEnvironmentVariable("MERRICK_CONNECTION_STRING") 
                           ?? Environment.GetEnvironmentVariable("ConnectionStrings__MerrickContext");
        
        // Also check TestContext if possible, but handle potential type mismatch by calling .ToString() or avoiding it if it causes issues.
        // For safety/unblocking, we stick to Environment variables which Docker/Aspire provides reliably.
        
        if (!string.IsNullOrWhiteSpace(fullConn)) return fullConn;

        // Priority 2: Build from parts (Host, Port, Password) to allow simple overrides (e.g. Docker port)
        // Defaults: 127.0.0.1, 55678, MerrickDevPassword2025
        string host = Environment.GetEnvironmentVariable("MERRICK_DB_HOST") ?? "127.0.0.1";
        string port = Environment.GetEnvironmentVariable("MERRICK_DB_PORT") ?? "55678"; // Default dev port
        string password = Environment.GetEnvironmentVariable("MERRICK_DB_PASSWORD") ?? "MerrickDevPassword2025"; 

        return $"Server={host},{port};Database=development;User Id=sa;Password={password};TrustServerCertificate=True;Connection Timeout=60;";
    }
}

public class TestHostEnvironment : IHostEnvironment
{
    public string EnvironmentName { get; set; } = "Development";
    public string ApplicationName { get; set; } = "TestApp";
    public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
    public IFileProvider ContentRootFileProvider { get; set; } = null!;
}