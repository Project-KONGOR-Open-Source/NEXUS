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

        List<int> otherPlayerAccountIDs = [.. allPlayerStatistics.Select(statistics => statistics.AccountID).Distinct()];

        List<Account> otherPlayerAccounts = await MerrickContext.Accounts
            .Include(playerAccount => playerAccount.User)
            .Include(playerAccount => playerAccount.Clan)
            .Where(playerAccount => otherPlayerAccountIDs.Contains(playerAccount.ID))
            .ToListAsync();

        List<Account> allPlayerAccounts = [account, .. otherPlayerAccounts];

        Dictionary<int, MatchPlayerStatistics> matchPlayerStatistics = [];
        Dictionary<int, MatchPlayerInventory> matchPlayerInventories = [];

        foreach (PlayerStatistics playerStatistics in allPlayerStatistics)
        {
            Account playerAccount = allPlayerAccounts.Single(playerAccount => playerAccount.ID == playerStatistics.AccountID);

            List<AccountStatistics> accountStatistics = await MerrickContext.AccountStatistics.Where(statistics => statistics.AccountID == playerStatistics.AccountID).ToListAsync();

            // TODO: Figure Out How To Select Which Statistics To Use (Public Match, Matchmaking, etc.)
            // INFO: Currently, This Code Logic Assumes A Public Match
            // INFO: Potential Logic + Switch/Case On Map Name: bool isPublic = form.player_stats.First().Value.First().Value.pub_count == 1;

            AccountStatistics currentMatchTypeStatistics = accountStatistics.Where(statistics => statistics.StatisticsType == AccountStatisticsType.Public).SingleOrDefault() ?? new()
            {
                AccountID = playerStatistics.AccountID,
                StatisticsType = AccountStatisticsType.Public,
                PlacementMatchesData = null
            };

            // TODO: Increment Current Match Type Statistics With Current Match Data

            AccountStatistics publicMatchStatistics = accountStatistics.Where(statistics => statistics.StatisticsType == AccountStatisticsType.Public).SingleOrDefault() ?? new ()
            {
                AccountID = playerStatistics.AccountID,
                StatisticsType = AccountStatisticsType.Public,
                PlacementMatchesData = null
            };

            // TODO: Increment Public Match Statistics With Current Match Data

            AccountStatistics matchmakingStatistics = accountStatistics.Where(statistics => statistics.StatisticsType == AccountStatisticsType.Public).SingleOrDefault() ?? new ()
            {
                AccountID = playerStatistics.AccountID,
                StatisticsType = AccountStatisticsType.Matchmaking,
                PlacementMatchesData = string.Empty
            };

            // TODO: Increment Matchmaking Statistics With Current Match Data

            matchPlayerStatistics[playerStatistics.AccountID] =
                new MatchPlayerStatistics(matchStartData, playerAccount, playerStatistics, currentMatchTypeStatistics, publicMatchStatistics, matchmakingStatistics)
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

        PlayerStatistics requestingPlayerStatistics = allPlayerStatistics.Single(statistics => statistics.AccountID == account.ID);

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
            MatchSummary = [ matchSummary ],
            MatchPlayerStatistics = [ matchPlayerStatistics ],
            MatchPlayerInventories = [ matchPlayerInventories ],
            MatchMastery = matchMastery,
            OwnedStoreItems = account.User.OwnedStoreItems,
            OwnedStoreItemsData = SetOwnedStoreItemsData(account),
            SelectedStoreItems = account.SelectedStoreItems,
            CustomIconSlotID = SetCustomIconSlotID(account)
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
