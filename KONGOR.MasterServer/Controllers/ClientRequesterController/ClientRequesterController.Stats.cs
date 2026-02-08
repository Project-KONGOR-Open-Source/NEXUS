namespace KONGOR.MasterServer.Controllers.ClientRequesterController;

public partial class ClientRequesterController
{
    private async Task<IActionResult> GetPlayerAwardSummary()
    {
        string? accountName = Request.Form["nickname"];

        if (accountName is null)
            return BadRequest(@"Missing Value For Form Parameter ""nickname""");

        Account? account = await MerrickContext.Accounts
            .SingleOrDefaultAsync(account => account.Name.Equals(accountName));

        if (account is null)
            return NotFound($@"Account With Name ""{accountName}"" Was Not Found");

        // TODO: Implement Actual Award Statistics From Database

        GetPlayerAwardSummaryResponse response = new ()
        {
            AccountID = account.ID.ToString(),

            MVPAwards = Random.Shared.Next(0, int.MaxValue).ToString(),
            AnnihilationAwards = Random.Shared.Next(0, int.MaxValue).ToString(),
            QuadKillAwards = Random.Shared.Next(0, int.MaxValue).ToString(),
            LongestKillStreakAwards = Random.Shared.Next(0, int.MaxValue).ToString(),
            SmackdownAwards = Random.Shared.Next(0, int.MaxValue).ToString(),
            MostKillsAwards = Random.Shared.Next(0, int.MaxValue).ToString(),
            MostAssistsAwards = Random.Shared.Next(0, int.MaxValue).ToString(),
            LeastDeathsAwards = Random.Shared.Next(0, int.MaxValue).ToString(),
            MostBuildingDamageAwards = Random.Shared.Next(0, int.MaxValue).ToString(),
            MostWardsDestroyedAwards = Random.Shared.Next(0, int.MaxValue).ToString(),
            MostHeroDamageDealtAwards = Random.Shared.Next(0, int.MaxValue).ToString(),
            HighestCreepScoreAwards = Random.Shared.Next(0, int.MaxValue).ToString()
        };

        // TODO: Most Wards Destroyed Awards Seems To Be Missing From The Client UI, Find Out Why

        return Ok(PhpSerialization.Serialize(response));
    }

    private async Task<IActionResult> GetSeasons()
    {
        string? accountName = Request.Form["nickname"];

        if (accountName is null)
            return BadRequest(@"Missing Value For Form Parameter ""nickname""");

        int[] seasons = [ 666 ];

        GetSeasonsResponse response = new ()
        {
            AllSeasons = string.Join("|", seasons.Select(season => $"{season},0|{season},1"))
        };

        return Ok(PhpSerialization.Serialize(response));
    }

    private async Task<IActionResult> GetSimpleStatistics()
    {
        string? accountName = Request.Form["nickname"];

        if (accountName is null)
            return BadRequest(@"Missing Value For Form Parameter ""nickname""");

        Account? account = await MerrickContext.Accounts
            .Include(account => account.User)
            .Include(account => account.Clan)
            .SingleOrDefaultAsync(account => account.Name.Equals(accountName));

        if (account is null)
            return NotFound($@"Account With Name ""{accountName}"" Was Not Found");

        ShowSimpleStatsResponse response = new ()
        {
            NameWithClanTag = account.NameWithClanTag,
            ID = account.ID.ToString(),
            Level = account.User.TotalLevel,
            LevelExperience = account.User.TotalExperience,
            NumberOfAvatarsOwned = account.User.OwnedStoreItems.Count(item => item.StartsWith("aa.")),
            TotalMatchesPlayed = 5555, // TODO: Implement Matches Played
            CurrentSeason = 666,
            SimpleSeasonStats = new SimpleSeasonStats() // TODO: Implement Stats
            {
                RankedMatchesWon = 1001 /* ranked */ + 1001 /* ranked casual */,
                RankedMatchesLost = 1002 /* ranked */ + 1002 /* ranked casual */,
                WinStreak = Math.Max(1003 /* ranked */, 1003 /* ranked casual */),
                InPlacementPhase = 0, // TODO: Implement Placement Matches
                LevelsGainedThisSeason = account.User.TotalLevel
            },
            SimpleCasualSeasonStats = new SimpleSeasonStats() // TODO: Implement Stats
            {
                RankedMatchesWon = 1001 /* ranked */ + 1001 /* ranked casual */,
                RankedMatchesLost = 1002 /* ranked */ + 1002 /* ranked casual */,
                WinStreak = Math.Max(1003 /* ranked */, 1003 /* ranked casual */),
                InPlacementPhase = 0, // TODO: Implement Placement Matches
                LevelsGainedThisSeason = account.User.TotalLevel
            },
            MVPAwardsCount = 1004,
            Top4AwardNames = [ "awd_masst", "awd_mhdd", "awd_mbdmg", "awd_lgks" ], // TODO: Implement Awards
            Top4AwardCounts = [ 1005, 1006, 1007, 1008 ], // TODO: Implement Awards
            CustomIconSlotID = SetCustomIconSlotID(account),
            OwnedStoreItems = account.User.OwnedStoreItems,
            SelectedStoreItems = account.SelectedStoreItems,
            OwnedStoreItemsData = SetOwnedStoreItemsData(account)
        };

        return Ok(PhpSerialization.Serialize(response));
    }

    private async Task<IActionResult> GetStatistics()
    {
        string? accountName = Request.Form["nickname"];

        if (accountName is null)
            return BadRequest(@"Missing Value For Form Parameter ""nickname""");

        Account? account = await MerrickContext.Accounts
            .Include(account => account.User)
            .Include(account => account.Clan)
            .SingleOrDefaultAsync(account => account.Name.Equals(accountName));

        if (account is null)
            return NotFound($@"Account With Name ""{accountName}"" Was Not Found");

        string? table = Request.Form["table"];

        if (table is null)
            return BadRequest(@"Missing Value For Form Parameter ""table""");

        List<AccountStatistics> allAccountStatistics = await MerrickContext.AccountStatistics
            .Where(statistics => statistics.AccountID == account.ID).ToListAsync();

        Dictionary<AccountStatisticsType, AccountStatistics> statisticsByType = allAccountStatistics.ToDictionary(statistics => statistics.Type);

        AggregateStatistics aggregates = AggregateStatistics.FromStatistics(statisticsByType);

        if (table is "player")
        {
            AccountStatistics statistics = statisticsByType[AccountStatisticsType.Public];

            PlayerStatisticsResponse response = new (account, statistics, aggregates);

            return Ok(PhpSerialization.Serialize(response));
        }

        if (table is "ranked")
        {
            AccountStatistics statistics = statisticsByType[AccountStatisticsType.Matchmaking];

            RankedStatisticsResponse response = new (account, statistics, aggregates);

            return Ok(PhpSerialization.Serialize(response));
        }

        if (table is "casual")
        {
            AccountStatistics statistics = statisticsByType[AccountStatisticsType.MatchmakingCasual];

            CasualStatisticsResponse response = new (account, statistics, aggregates);

            return Ok(PhpSerialization.Serialize(response));
        }

        if (table is "campaign")
        {
            AccountStatistics statistics = statisticsByType[AccountStatisticsType.Matchmaking];

            CampaignStatisticsResponse response = new (account, statistics, aggregates);

            return Ok(PhpSerialization.Serialize(response));
        }

        if (table is "campaign_casual")
        {
            AccountStatistics statistics = statisticsByType[AccountStatisticsType.MatchmakingCasual];

            CampaignCasualStatisticsResponse response = new (account, statistics, aggregates);

            return Ok(PhpSerialization.Serialize(response));
        }

        if (table is "mastery")
        {
            ShowMasteryStatisticsResponse response = new (account);

            // TODO: Populate MasteryInfo From Mastery System Once Re-Implemented
            // TODO: Populate MasteryRewards From Mastery System Once Re-Implemented (Only For Own Account)

            return Ok(PhpSerialization.Serialize(response));
        }

        throw new ArgumentOutOfRangeException(nameof(table), table, $@"Unsupported Value For Form Parameter ""table"": ""{table}""");
    }

    private async Task<IActionResult> GetHeroStatistics()
    {
        // TODO: Implement Actual Hero Statistics From Database

        GetHeroStatisticsResponse response = new ()
        {
            # region Test Data
            AllHeroStatistics = new AllHeroStatistics
            {
                Ranked =
                [
                    new ()
                    {
                        HeroIdentifier = "Hero_Armadon",
                        TimesUsed = "69",
                        Wins = "37",
                        Losses = "32",
                        HeroKills = "338",
                        Deaths = "437",
                        HeroAssists = "735",
                        TeamCreepKills = "4855",
                        Denies = "423",
                        Experience = "936843",
                        Gold = "571726",
                        Actions = "238865",
                        TimeEarningExperience = "147897"
                    },
                    new ()
                    {
                        HeroIdentifier = "Hero_Behemoth",
                        TimesUsed = "21",
                        Wins = "13",
                        Losses = "8",
                        HeroKills = "89",
                        Deaths = "142",
                        HeroAssists = "304",
                        TeamCreepKills = "1552",
                        Denies = "93",
                        Experience = "316797",
                        Gold = "200982",
                        Actions = "68970",
                        TimeEarningExperience = "48968"
                    },
                    new ()
                    {
                        HeroIdentifier = "Hero_Valkyrie",
                        TimesUsed = "85",
                        Wins = "42",
                        Losses = "43",
                        HeroKills = "414",
                        Deaths = "462",
                        HeroAssists = "807",
                        TeamCreepKills = "7373",
                        Denies = "812",
                        Experience = "1314576",
                        Gold = "830936",
                        Actions = "293553",
                        TimeEarningExperience = "197288"
                    }
                ],
                Casual =
                [
                    new ()
                    {
                        HeroIdentifier = "Hero_Defiler",
                        TimesUsed = "4",
                        Wins = "2",
                        Losses = "2",
                        HeroKills = "19",
                        Deaths = "40",
                        HeroAssists = "74",
                        TeamCreepKills = "276",
                        Denies = "18",
                        Experience = "71282",
                        Gold = "60110",
                        Actions = "11786",
                        TimeEarningExperience = "8333"
                    },
                    new ()
                    {
                        HeroIdentifier = "Hero_Hellbringer",
                        TimesUsed = "6",
                        Wins = "4",
                        Losses = "2",
                        HeroKills = "42",
                        Deaths = "46",
                        HeroAssists = "59",
                        TeamCreepKills = "422",
                        Denies = "34",
                        Experience = "85391",
                        Gold = "89842",
                        Actions = "13797",
                        TimeEarningExperience = "10682"
                    }
                ],
                Campaign =
                [
                    new ()
                    {
                        HeroIdentifier = "Hero_Ebulus",
                        TimesUsed = "25",
                        Wins = "13",
                        Losses = "12",
                        HeroKills = "140",
                        Deaths = "202",
                        HeroAssists = "266",
                        TeamCreepKills = "1696",
                        Denies = "121",
                        Experience = "323778",
                        Gold = "224830",
                        Actions = "66027",
                        TimeEarningExperience = "54746"
                    },
                    new ()
                    {
                        HeroIdentifier = "Hero_Shaman",
                        TimesUsed = "32",
                        Wins = "17",
                        Losses = "15",
                        HeroKills = "67",
                        Deaths = "194",
                        HeroAssists = "413",
                        TeamCreepKills = "1648",
                        Denies = "98",
                        Experience = "426615",
                        Gold = "282328",
                        Actions = "81134",
                        TimeEarningExperience = "71783"
                    },
                    new ()
                    {
                        HeroIdentifier = "Hero_Hellbringer",
                        TimesUsed = "31",
                        Wins = "18",
                        Losses = "13",
                        HeroKills = "146",
                        Deaths = "227",
                        HeroAssists = "391",
                        TeamCreepKills = "1519",
                        Denies = "75",
                        Experience = "383348",
                        Gold = "278681",
                        Actions = "73110",
                        TimeEarningExperience = "65994"
                    }
                ],
                CampaignCasual =
                [
                    new ()
                    {
                        HeroIdentifier = "Hero_Empath",
                        TimesUsed = "5",
                        Wins = "3",
                        Losses = "2",
                        HeroKills = "20",
                        Deaths = "35",
                        HeroAssists = "45",
                        TeamCreepKills = "182",
                        Denies = "15",
                        Experience = "50000",
                        Gold = "35000",
                        Actions = "8000",
                        TimeEarningExperience = "7500"
                    },
                    new ()
                    {
                        HeroIdentifier = "Hero_Lodestone",
                        TimesUsed = "3",
                        Wins = "2",
                        Losses = "1",
                        HeroKills = "15",
                        Deaths = "20",
                        HeroAssists = "30",
                        TeamCreepKills = "250",
                        Denies = "20",
                        Experience = "45000",
                        Gold = "30000",
                        Actions = "6500",
                        TimeEarningExperience = "6000"
                    }
                ]
            }
            # endregion
        };

        return Ok(PhpSerialization.Serialize(response));
    }

    private async Task<IActionResult> GetMatchStatistics()
    {
        string? cookie = Request.Form["cookie"];

        if (cookie is null)
            return BadRequest(@"Missing Value For Form Parameter ""cookie""");

        string? matchID = Request.Form["match_id"];

        if (matchID is null)
            return BadRequest(@"Missing Value For Form Parameter ""match_id""");

        MatchStatistics? matchStatistics = await MerrickContext.MatchStatistics.SingleOrDefaultAsync(matchStatistics => matchStatistics.MatchID == int.Parse(matchID));

        if (matchStatistics is null)
            return new NotFoundObjectResult("Match Stats Not Found");

        List<MatchParticipantStatistics> allPlayerStatistics = await MerrickContext.MatchParticipantStatistics.Where(playerStatistics => playerStatistics.MatchID == matchStatistics.MatchID).ToListAsync();

        string? accountName = await DistributedCache.GetAccountNameForSessionCookie(cookie);

        if (accountName is null)
            return new NotFoundObjectResult("Session Not Found");

        Account? account = await MerrickContext.Accounts
            .Include(account => account.User)
            .Include(account => account.Clan)
            .SingleOrDefaultAsync(account => account.Name.Equals(accountName));

        if (account is null)
            return new NotFoundObjectResult("Account Not Found");

        MatchInformation? matchInformation = await DistributedCache.GetMatchInformation(matchStatistics.MatchID);

        if (matchInformation is null)
            return new NotFoundObjectResult("Match Information Not Found");

        MatchSummary matchSummary = new (matchStatistics, allPlayerStatistics, matchInformation);

        List<int> otherPlayerAccountIDs = [.. allPlayerStatistics.Select(statistics => statistics.AccountID).Where(id => id != account.ID)];

        List<Account> otherPlayerAccounts = await MerrickContext.Accounts
            .Include(playerAccount => playerAccount.User)
            .Include(playerAccount => playerAccount.Clan)
            .Where(playerAccount => otherPlayerAccountIDs.Contains(playerAccount.ID))
            .ToListAsync();

        List<Account> allPlayerAccounts = [account, .. otherPlayerAccounts];

        Dictionary<int, OneOf<MatchPlayerStatisticsWithMatchPerformanceData, MatchPlayerStatistics>> matchPlayerStatistics = [];
        Dictionary<int, MatchPlayerInventory> matchPlayerInventories = [];

        foreach (MatchParticipantStatistics playerStatistics in allPlayerStatistics)
        {
            Account playerAccount = allPlayerAccounts.Single(playerAccount => playerAccount.ID == playerStatistics.AccountID);

            List<AccountStatistics> accountStatistics = await MerrickContext.AccountStatistics.Where(statistics => statistics.AccountID == playerStatistics.AccountID).ToListAsync();

            // TODO: Figure Out How To Select Which Statistics To Use (Public Match, Matchmaking, etc.)
            // INFO: Currently, This Code Logic Assumes A Public Match
            // INFO: Potential Logic + Switch/Case On Map Name: bool isPublic = form.player_stats.First().Value.First().Value.pub_count == 1;

            AccountStatistics currentMatchTypeStatistics = accountStatistics.Single(statistics => statistics.Type == AccountStatisticsType.Public);

            // TODO: Increment Current Match Type Statistics With Current Match Data

            AccountStatistics publicMatchStatistics = accountStatistics.Single(statistics => statistics.Type == AccountStatisticsType.Public);

            // TODO: Increment Public Match Statistics With Current Match Data

            AccountStatistics matchmakingStatistics = accountStatistics.Single(statistics => statistics.Type == AccountStatisticsType.Matchmaking);

            // TODO: Increment Matchmaking Statistics With Current Match Data

            // Use PrimaryMatchPlayerStatistics With Additional Information For The Primary (Requesting) Player And MatchPlayerStatistics With The Standard Amount Of Information For Secondary Players
            matchPlayerStatistics[playerStatistics.AccountID] = playerStatistics.AccountID == account.ID
                ? new MatchPlayerStatisticsWithMatchPerformanceData(matchInformation, playerAccount, playerStatistics, currentMatchTypeStatistics, publicMatchStatistics, matchmakingStatistics)
                    { HeroIdentifier = playerStatistics.HeroIdentifier }
                : new MatchPlayerStatistics(matchInformation, playerAccount, playerStatistics, currentMatchTypeStatistics, publicMatchStatistics, matchmakingStatistics)
                    { HeroIdentifier = playerStatistics.HeroIdentifier };

            List<string> inventory = playerStatistics.Inventory ?? [];

            matchPlayerInventories[playerStatistics.AccountID] = new MatchPlayerInventory
            {
                AccountID = playerStatistics.AccountID,
                MatchID = playerStatistics.MatchID,

                Slot1 = inventory.ElementAtOrDefault(0),
                Slot2 = inventory.ElementAtOrDefault(1),
                Slot3 = inventory.ElementAtOrDefault(2),
                Slot4 = inventory.ElementAtOrDefault(3),
                Slot5 = inventory.ElementAtOrDefault(4),
                Slot6 = inventory.ElementAtOrDefault(5)
            };
        }

        MatchParticipantStatistics requestingPlayerStatistics = allPlayerStatistics.Single(statistics => statistics.AccountID == account.ID);

        MatchMastery matchMastery = new
        (
            heroIdentifier: requestingPlayerStatistics.HeroIdentifier,
            currentMasteryExperience: 0, // TODO: Retrieve From Mastery System Once Re-Implemented
            matchMasteryExperience: 100, // TODO: Calculate Based On Match Duration And Result (Use Calculation That I Implemented In Legacy PK)
            bonusExperience: 10 // TODO: Calculate Based On Max-Level Heroes Owned
        )
        {
            MasteryExperienceMaximumLevelHeroesCount = 0, // TODO: Count Heroes At Max Mastery Level (+ Enable MatchMastery Constructor Once Masteries Are Re-Implemented)
            MasteryExperienceBoostProductCount = 0, // TODO: Count "ma.Mastery Boost" Items (+ Enable MatchMastery Constructor Once Masteries Are Re-Implemented)
            MasteryExperienceSuperBoostProductCount = 0 // TODO: Count "ma.Super Mastery Boost" Items (+ Enable MatchMastery Constructor Once Masteries Are Re-Implemented)
        };

        MatchStatsResponse response = new ()
        {
            GoldCoins = account.User.GoldCoins.ToString(),
            SilverCoins = account.User.SilverCoins.ToString(),
            MatchSummary = new Dictionary<int, MatchSummary> { { matchStatistics.MatchID, matchSummary } },
            MatchPlayerStatistics = new Dictionary<int, Dictionary<int, OneOf<MatchPlayerStatisticsWithMatchPerformanceData, MatchPlayerStatistics>>> { { matchStatistics.MatchID, matchPlayerStatistics } },
            MatchPlayerInventories = new Dictionary<int, Dictionary<int, MatchPlayerInventory>> { { matchStatistics.MatchID, matchPlayerInventories } },
            MatchMastery = matchMastery,
            OwnedStoreItems = account.User.OwnedStoreItems,
            OwnedStoreItemsData = SetOwnedStoreItemsData(account),
            SelectedStoreItems = account.SelectedStoreItems,
            CustomIconSlotID = SetCustomIconSlotID(account)
        };

        return Ok(PhpSerialization.Serialize(response));
    }

    private static string SetCustomIconSlotID(Account account)
        => account.SelectedStoreItems.Any(item => item.StartsWith("ai.custom_icon"))
            ? account.SelectedStoreItems.Single(item => item.StartsWith("ai.custom_icon")).Replace("ai.custom_icon:", string.Empty) : "0";

    private static Dictionary<string, OneOf<StoreItemData, StoreItemDiscountCoupon>> SetOwnedStoreItemsData(Account account)
    {
        Dictionary<string, OneOf<StoreItemData, StoreItemDiscountCoupon>> items = account.User.OwnedStoreItems
            .Where(item => item.StartsWith("ma.").Equals(false) && item.StartsWith("cp.").Equals(false))
            .ToDictionary<string, string, OneOf<StoreItemData, StoreItemDiscountCoupon>>(upgrade => upgrade, upgrade => new StoreItemData());

        // TODO: Add Mastery Boosts And Coupons

        /*
            Dictionary<string, object> myUpgradesInfo = accountDetails.UnlockedUpgradeCodes
                .Where(upgrade => upgrade.StartsWith("ma.").Equals(false) && upgrade.StartsWith("cp.").Equals(false))
                .ToDictionary<string, string, object>(upgrade => upgrade, upgrade => new MyUpgradesInfoEntry());

            foreach (string boost in GameConsumables.GetOwnedMasteryBoostProducts(accountDetails.UnlockedUpgradeCodes))
                myUpgradesInfo.Add(boost, new MyUpgradesInfoEntry());

            foreach (KeyValuePair<string, Coupon> coupon in GameConsumables.GetOwnedCoupons(accountDetails.UnlockedUpgradeCodes))
                myUpgradesInfo.Add(coupon.Key, coupon.Value);

            return myUpgradesInfo;
         */

        return items;
    }
}
