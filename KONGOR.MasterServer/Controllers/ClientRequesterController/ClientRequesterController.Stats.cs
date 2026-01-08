namespace KONGOR.MasterServer.Controllers.ClientRequesterController;

public partial class ClientRequesterController
{
    private async Task<IActionResult> GetSimpleStats()
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
            CurrentSeason = 12, // TODO: Set Season
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

    private async Task<IActionResult> HandleMatchStats()
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

        List<PlayerStatistics> allPlayerStatistics = await MerrickContext.PlayerStatistics.Where(playerStatistics => playerStatistics.MatchID == matchStatistics.MatchID).ToListAsync();

        string? accountName = await DistributedCache.GetAccountNameForSessionCookie(cookie);

        if (accountName is null)
            return new NotFoundObjectResult("Session Not Found");

        Account? account = await MerrickContext.Accounts
            .Include(account => account.User)
            .Include(account => account.Clan)
            .SingleOrDefaultAsync(account => account.Name.Equals(accountName));

        if (account is null)
            return new NotFoundObjectResult("Account Not Found");

        MatchStartData? matchStartData = await DistributedCache.GetMatchStartData(matchStatistics.MatchID);

        if (matchStartData is null)
            return new NotFoundObjectResult("Match Start Data Not Found");

        MatchSummary matchSummary = new (matchStatistics, allPlayerStatistics, matchStartData);

        PlayerStatistics playerStatistics = allPlayerStatistics.Single(statistics => statistics.AccountID == account.ID);

        List<int> otherPlayerAccountIDs = [.. allPlayerStatistics.Select(statistics => statistics.AccountID).Distinct()];

        List<Account> otherPlayerAccounts = await MerrickContext.Accounts
            .Include(playerAccount => playerAccount.User)
            .Include(playerAccount => playerAccount.Clan)
            .Where(playerAccount => otherPlayerAccountIDs.Contains(playerAccount.ID))
            .ToListAsync();

        Dictionary<int, MatchPlayerStatistics> matchPlayerStatistics = [];

        foreach (PlayerStatistics statistics in allPlayerStatistics)
        {
            Account otherPlayerAccount = otherPlayerAccounts.Single(playerAccount => playerAccount.ID == statistics.AccountID);

            matchPlayerStatistics[statistics.AccountID] = new MatchPlayerStatistics(otherPlayerAccount, statistics)
            {

            };
        }


            //foreach (PlayerStatistics statistics in allPlayerStatistics)
            //{
            //    Account playerAccount = allPlayerAccounts.Single(playerAccount => playerAccount.ID == statistics.AccountID);
            //    matchPlayerStatistics[statistics.AccountID] = new MatchPlayerStatistics(playerAccount, statistics)
            //    {
            //        // TODO: Add HeroIdentifier Field To PlayerStatistics Entity And Capture During Stats Submission
            //        // For Now, Using Placeholder Value Until Hero Identifier Is Properly Captured
            //        HeroIdentifier = "Hero_Placeholder", // TODO: Implement Hero Identifier Mapping From HeroProductID
            //        Wins = "0", // TODO: Calculate From PlayerStatistics Records Where Win = 1
            //        Losses = "0", // TODO: Calculate From PlayerStatistics Records Where Loss = 1
            //        Concedes = "0", // TODO: Calculate From PlayerStatistics Records Where Conceded = 1
            //        ConcedeVotes = statistics.ConcedeVotes.ToString(),
            //        Buybacks = statistics.Buybacks.ToString(),
            //        Disconnections = "0", // TODO: Calculate From PlayerStatistics Records Where Disconnected = 1
            //        Kicked = "0", // TODO: Calculate From PlayerStatistics Records Where Kicked = 1
            //        PublicSkill = "1500.0", // TODO: Calculate Or Retrieve From Account Statistics
            //        PublicCount = "0", // TODO: Calculate From PlayerStatistics Records Where PublicMatch = 1
            //        AMMSoloRating = "1500.0", // TODO: Retrieve From Ranked Solo Statistics
            //        AMMSoloCount = "0", // TODO: Calculate From Ranked Solo Match Records
            //        AMMTeamRating = "1500.0", // TODO: Retrieve From Ranked Team Statistics
            //        AMMTeamCount = "0", // TODO: Calculate From Ranked Team Match Records
            //        AverageScore = "0.00", // TODO: Calculate Average Score From PlayerStatistics Records
            //        HeroKills = statistics.HeroKills.ToString(),
            //        HeroDamage = statistics.HeroDamage.ToString(),
            //        HeroExperience = statistics.HeroExperience.ToString(),
            //        HeroKillsGold = statistics.GoldFromHeroKills.ToString(),
            //        HeroAssists = statistics.HeroAssists.ToString(),
            //        Deaths = statistics.HeroDeaths.ToString(),
            //        GoldLostToDeath = statistics.GoldLostToDeath.ToString(),
            //        SecondsDead = statistics.SecondsDead.ToString(),
            //        TeamCreepKills = statistics.TeamCreepKills.ToString(),
            //        TeamCreepDamage = statistics.TeamCreepDamage.ToString(),
            //        TeamCreepExperience = statistics.TeamCreepExperience.ToString(),
            //        TeamCreepGold = statistics.TeamCreepGold.ToString(),
            //        NeutralCreepKills = statistics.NeutralCreepKills.ToString(),
            //        NeutralCreepDamage = statistics.NeutralCreepDamage.ToString(),
            //        NeutralCreepExperience = statistics.NeutralCreepExperience.ToString(),
            //        NeutralCreepGold = statistics.NeutralCreepGold.ToString(),
            //        BuildingDamage = statistics.BuildingDamage.ToString(),
            //        BuildingExperience = statistics.ExperienceFromBuildings.ToString(),
            //        BuildingsRazed = statistics.BuildingsRazed.ToString(),
            //        BuildingGold = statistics.GoldFromBuildings.ToString(),
            //        Denies = statistics.Denies.ToString(),
            //        ExperienceDenied = statistics.ExperienceDenied.ToString(),
            //        Gold = statistics.Gold.ToString(),
            //        GoldSpent = statistics.GoldSpent.ToString(),
            //        Experience = statistics.Experience.ToString(),
            //        Actions = statistics.Actions.ToString(),
            //        Seconds = statistics.SecondsPlayed.ToString(),
            //        Consumables = statistics.ConsumablesPurchased.ToString(),
            //        Wards = statistics.WardsPlaced.ToString(),
            //        TimeEarningExperience = statistics.TimeEarningExperience.ToString(),
            //        FirstBlood = statistics.FirstBlood.ToString(),
            //        DoubleKill = statistics.DoubleKill.ToString(),
            //        TripleKill = statistics.TripleKill.ToString(),
            //        QuadKill = statistics.QuadKill.ToString(),
            //        Annihilation = statistics.Annihilation.ToString(),
            //        KillStreak3 = statistics.KillStreak03.ToString(),
            //        KillStreak4 = statistics.KillStreak04.ToString(),
            //        KillStreak5 = statistics.KillStreak05.ToString(),
            //        KillStreak6 = statistics.KillStreak06.ToString(),
            //        KillStreak7 = statistics.KillStreak07.ToString(),
            //        KillStreak8 = statistics.KillStreak08.ToString(),
            //        KillStreak9 = statistics.KillStreak09.ToString(),
            //        KillStreak10 = statistics.KillStreak10.ToString(),
            //        KillStreak15 = statistics.KillStreak15.ToString(),
            //        Smackdown = statistics.Smackdown.ToString(),
            //        Humiliation = statistics.Humiliation.ToString(),
            //        Nemesis = statistics.Nemesis.ToString(),
            //        Retribution = statistics.Retribution.ToString(),
            //        UsedToken = statistics.UsedToken.ToString(),
            //        ClanTag = statistics.ClanTag ?? string.Empty,
            //        AlternativeAvatarName = statistics.AlternativeAvatarName ?? string.Empty,
            //        SeasonProgress = new SeasonProgress
            //        {
            //            AccountID = statistics.AccountID,
            //            MatchID = statistics.MatchID,
            //            IsCasual = "0", // TODO: Determine If Match Was Casual Or Competitive Ranked
            //            MMRBefore = "1500", // TODO: Retrieve From Player Ranked Statistics
            //            MMRAfter = "1500", // TODO: Calculate MMR Change
            //            MedalBefore = "0", // TODO: Retrieve Medal Rank From Player Ranked Statistics
            //            MedalAfter = "0", // TODO: Calculate Medal After Match
            //            Season = "1", // TODO: Get Current Season Identifier
            //            PlacementMatches = 0, // TODO: Retrieve Placement Match Count
            //            PlacementWins = "0" // TODO: Retrieve Placement Wins Count
            //        }
            //    };
            //}

            //// Build MatchPlayerInventories Dictionary For All Players
            //Dictionary<int, MatchPlayerInventory> matchPlayerInventories = [];
            //foreach (PlayerStatistics statistics in allPlayerStatistics)
            //{
            //    // Map Inventory List To Slots 1-6 (First 6 Items), Using NULL For Empty Slots
            //    List<string> inventory = statistics.Inventory ?? [];
            //    matchPlayerInventories[statistics.AccountID] = new MatchPlayerInventory
            //    {
            //        AccountID = statistics.AccountID,
            //        MatchID = statistics.MatchID,
            //        Slot1 = inventory.ElementAtOrDefault(0),
            //        Slot2 = inventory.ElementAtOrDefault(1),
            //        Slot3 = inventory.ElementAtOrDefault(2),
            //        Slot4 = inventory.ElementAtOrDefault(3),
            //        Slot5 = inventory.ElementAtOrDefault(4),
            //        Slot6 = inventory.ElementAtOrDefault(5)
            //    };
            //}

            //// Build MatchMastery With Placeholder Values
            //int matchMasteryExperience = 100; // TODO: Calculate Based On Match Duration And Result
            //int bonusExperience = 10; // TODO: Calculate Based On Max-Level Heroes Owned

            //MatchMastery matchMastery = new (
            //    heroIdentifier: "Hero_Gauntlet", // TODO: Get Actual Hero Identifier
            //    currentMasteryExperience: 0, // TODO: Retrieve From Mastery System
            //    matchMasteryExperience: matchMasteryExperience,
            //    bonusExperience: bonusExperience)
            //{
            //    HeroIdentifier = "Hero_Gauntlet",
            //    CurrentMasteryExperience = 0,
            //    MatchMasteryExperience = matchMasteryExperience,
            //    MasteryExperienceBonus = 0,
            //    MasteryExperienceBoost = 0,
            //    MasteryExperienceSuperBoost = 0,
            //    MasteryExperienceMaximumLevelHeroesCount = 0, // TODO: Count Heroes At Max Mastery Level
            //    MasteryExperienceHeroesBonus = bonusExperience,
            //    MasteryExperienceToBoost = (matchMasteryExperience + bonusExperience) * 2,
            //    MasteryExperienceEventBonus = 0,
            //    MasteryExperienceCanBoost = true,
            //    MasteryExperienceCanSuperBoost = true,
            //    MasteryExperienceBoostProductIdentifier = 3609,
            //    MasteryExperienceSuperBoostProductIdentifier = 4605,
            //    MasteryExperienceBoostProductCount = 0, // TODO: Count "ma.Mastery Boost" Items
            //    MasteryExperienceSuperBoostProductCount = 0 // TODO: Count "ma.Super Mastery Boost" Items
            //};

            MatchStatsResponse response = new ()
        {
            GoldCoins = account.User.GoldCoins.ToString(),
            SilverCoins = account.User.SilverCoins.ToString(),
            MatchSummary = [ matchSummary ],
            MatchPlayerStatistics = [ matchPlayerStatistics ],
            MatchPlayerInventories = [ matchPlayerInventories ],
            MatchMastery = matchMastery,
            OwnedStoreItems = account.User.OwnedStoreItems,
            OwnedStoreItemsData = SetOwnedStoreItemsData(account),
            SelectedStoreItems = account.SelectedStoreItems,
            CustomIconSlotID = SetCustomIconSlotID(account),
            CampaignReward = new CampaignReward() // Using Default Values From Model
        };

        string json = JsonSerializer.Serialize(response);
        string php = PhpSerialization.Serialize(response);

        return Ok(php);
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

        return items;
    }
}
