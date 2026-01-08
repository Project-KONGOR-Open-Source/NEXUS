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
            .FirstOrDefaultAsync(account => account.Name.Equals(accountName));

        if (account is null)
            return NotFound($@"Account With Name ""{accountName}"" Was Not Found");

        ShowSimpleStatsResponse response = await CreateShowSimpleStatsResponse(account);

        return Ok(PhpSerialization.Serialize(response));
    }

    private async Task<IActionResult> HandleInitStats()
    {
        string? cookie = Request.Form["cookie"];
        // 2026-01-07: REMOVED Dash Stripping. Main Controller handles fuzzy validation.
        // if (cookie is not null) cookie = cookie.Replace("-", string.Empty);

        if (cookie is null)
            return BadRequest(@"Missing Value For Form Parameter ""cookie""");

        string? accountName = HttpContext.Items["SessionAccountName"] as string 
                             ?? await DistributedCache.GetAccountNameForSessionCookie(cookie);

        if (accountName is null)
            return Unauthorized("Session Not Found");

        Account? account = await MerrickContext.Accounts
            .Include(account => account.User)
            .Include(account => account.Clan)
            .FirstOrDefaultAsync(account => account.Name.Equals(accountName));

        if (account is null)
            return NotFound("Account Not Found");

        if (account is null)
            return NotFound("Account Not Found");

        // 2026-01-06: FIX - Refactored to return explicit dictionary matching legacy protocol.
        // We exclude 'my_upgrades' and 'my_upgrades_info' from get_initStats as they trigger client-side errors.
        // The legacy client expects only basic stats and selected upgrades here.
        ShowSimpleStatsResponse fullResponse = await CreateShowSimpleStatsResponse(account);

        // 2026-01-06: FIX - Refactored to return manual dictionary.
        // Restored standard keys (slot_id, tokens) as strict removal might cause client instability.
        Dictionary<string, object> response = new()
        {
            { "nickname", fullResponse.NameWithClanTag },
            { "account_id", fullResponse.ID },
            { "level", fullResponse.Level },
            { "level_exp", fullResponse.LevelExperience },
            { "avatar_num", fullResponse.NumberOfAvatarsOwned },
            { "hero_num", fullResponse.NumberOfHeroesOwned },
            { "total_played", fullResponse.TotalMatchesPlayed },
            { "season_id", fullResponse.CurrentSeason },
            { "season_level", fullResponse.SeasonLevel },
            { "creep_level", fullResponse.CreepLevel },
            { "season_normal", fullResponse.SimpleSeasonStats },
            { "season_casual", fullResponse.SimpleCasualSeasonStats },
            { "mvp_num", fullResponse.MVPAwardsCount },
            { "award_top4_name", fullResponse.Top4AwardNames },
            { "award_top4_num", fullResponse.Top4AwardCounts },
            { "slot_id", fullResponse.CustomIconSlotID },
            { "selected_upgrades", fullResponse.SelectedStoreItems },
            { "dice_tokens", fullResponse.DiceTokens },
            { "game_tokens", fullResponse.GameTokens },
            { "timestamp", fullResponse.ServerTimestamp },
            { "vested_threshold", fullResponse.VestedThreshold },
            { "0", fullResponse.Zero }
        };

        string serializedResponse = PhpSerialization.Serialize(response);
        Logger.LogInformation($"[InitStats] Response: {serializedResponse}");
        return Ok(serializedResponse);
    }

    private async Task<ShowSimpleStatsResponse> CreateShowSimpleStatsResponse(Account account)
    {
        return new ShowSimpleStatsResponse()
        {
            NameWithClanTag = account.NameWithClanTag,
            ID = account.ID.ToString(),
            Level = account.User.TotalLevel,
            LevelExperience = account.User.TotalExperience,
            NumberOfAvatarsOwned = account.User.OwnedStoreItems.Count(item => item.StartsWith("aa.")),
            TotalMatchesPlayed = await MerrickContext.PlayerStatistics.CountAsync(stats => stats.AccountID == account.ID), // TODO: Implement Matches Played
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
            Top4AwardNames = ["awd_masst", "awd_mhdd", "awd_mbdmg", "awd_lgks"], // TODO: Implement Awards
            Top4AwardCounts = [1005, 1006, 1007, 1008], // TODO: Implement Awards
            CustomIconSlotID = SetCustomIconSlotID(account),
            OwnedStoreItems = account.User.OwnedStoreItems.Distinct().ToList(),
            SelectedStoreItems = account.SelectedStoreItems.Distinct().ToList(),
            OwnedStoreItemsData = SetOwnedStoreItemsData(account)
        };
    }

    private async Task<IActionResult> HandleMatchStats()
    {
        string? cookie = Request.Form["cookie"];
        // 2026-01-07: REMOVED Dash Stripping. Main Controller handles fuzzy validation.
        // if (cookie is not null) cookie = cookie.Replace("-", string.Empty);

        string? matchIDString = Request.Form["match_id"];

        Logger.LogInformation("Received Match Stats Request: MatchID={MatchID}, Cookie={Cookie}", matchIDString, cookie);

        if (cookie is null)
            return BadRequest(@"Missing Value For Form Parameter ""cookie""");

        if (matchIDString is null)
        {
            Logger.LogError("Match Stats Request Failed: Missing Match ID");
            return BadRequest(@"Missing Value For Form Parameter ""match_id""");
        }

        if (!int.TryParse(matchIDString, out int matchID))
        {
            return BadRequest("Invalid Match ID");
        }

        MatchStatistics? matchStatistics = await MerrickContext.MatchStatistics.SingleOrDefaultAsync(matchStatistics => matchStatistics.MatchID == matchID);

        if (matchStatistics is null)
        {
            Logger.LogWarning("Match Stats Request Failed: Match Statistics Not Found For ID {MatchID}. Returning Soft Failure.", matchID);
            
            // Construct a "Safe" dummy response to prevent client crash when stats are missing
            MatchStatsResponse safeResponse = new()
            {
                GoldCoins = "0",
                SilverCoins = "0",
                MatchSummary = [],
                MatchPlayerStatistics = [],
                MatchPlayerInventories = [],
                MatchMastery = new MatchMastery
                {
                    HeroIdentifier = "Hero_Legionnaire",
                    CurrentMasteryExperience = 0,
                    MatchMasteryExperience = 0,
                    MasteryExperienceBonus = 0,
                    MasteryExperienceBoost = 0,
                    MasteryExperienceSuperBoost = 0,
                    MasteryExperienceMaximumLevelHeroesCount = 0,
                    MasteryExperienceHeroesBonus = 0,
                    MasteryExperienceToBoost = 0,
                    MasteryExperienceEventBonus = 0,
                    MasteryExperienceCanBoost = false,
                    MasteryExperienceCanSuperBoost = false,
                    MasteryExperienceBoostProductIdentifier = 3609,
                    MasteryExperienceSuperBoostProductIdentifier = 4605,
                    MasteryExperienceBoostProductCount = 0,
                    MasteryExperienceSuperBoostProductCount = 0
                },
                // CRITICAL FIX: Initialize these as empty Dictionaries to satisfy the client's parser
                OwnedStoreItems = [],
                SelectedStoreItems = [],
                OwnedStoreItemsData = [],
                CustomIconSlotID = "0",
                CampaignReward = new CampaignReward()
            };

            return Ok(PhpSerialization.Serialize(safeResponse));
        }

        List<PlayerStatistics> allPlayerStatistics = await MerrickContext.PlayerStatistics.Where(playerStatistics => playerStatistics.MatchID == matchStatistics.MatchID).ToListAsync();

        string? accountName = HttpContext.Items["SessionAccountName"] as string
                              ?? await DistributedCache.GetAccountNameForSessionCookie(cookie);

        // Allow anonymous access; fetch account only if session exists
        Account? account = null;
        if (accountName is not null)
        {
            account = await MerrickContext.Accounts
                .Include(account => account.User)
                .Include(account => account.Clan)
                .SingleOrDefaultAsync(account => account.Name.Equals(accountName));
        }

        // Robustness: If MatchStartData is missing (expired cache), reconstruct from MatchStatistics
        MatchStartData? matchStartData = await DistributedCache.GetMatchStartData(matchStatistics.MatchID);
        
        matchStartData ??= new MatchStartData
        {
            MatchID = matchStatistics.MatchID,
            ServerID = matchStatistics.ServerID,
            MatchName = matchStatistics.FileName, // FIX: Use FileName as fallback for Name
            HostAccountName = matchStatistics.HostAccountName,
            Map = matchStatistics.Map,
            MatchMode = matchStatistics.GameMode,
            Version = matchStatistics.Version,
            IsCasual = matchStatistics.GameMode == "casual", // Approximation
            MatchType = (byte)(matchStatistics.GameMode.Contains("rank") ? 1 : 0), // FIX: Infer from GameMode
            Options = MatchOptions.None, // Data loss, but allows viewing stats
            ServerName = "Unknown"
        };
        
        // Define matchSummary
        MatchSummary matchSummary = new MatchSummary(matchStatistics, allPlayerStatistics, matchStartData);

        // Populate stats for ALL players
        Dictionary<int, MatchPlayerStatistics> matchPlayerStatistics = [];
        Dictionary<int, MatchPlayerInventory> matchPlayerInventories = [];

        // Pre-create a dummy Role for the dummy User
        MERRICK.DatabaseContext.Entities.Utility.Role dummyRole = new() { ID = 1, Name = "User" }; 

        foreach (PlayerStatistics stats in allPlayerStatistics)
        {
            // Create a lightweight Account object from stored stats to avoid N+1 queries
            Account playerAccount = new()
            {
                ID = stats.AccountID,
                Name = stats.AccountName,
                IsMain = true, // FIX: Required field
                User = new User // FIX: Required field
                {
                    ID = stats.AccountID,
                    EmailAddress = "dummy@kongor.net", // Required
                    Role = dummyRole, // Required
                    SRPPasswordHash = "", // Required
                    SRPPasswordSalt = "" // Required
                },
                Clan = stats.ClanID.HasValue ? new Clan 
                { 
                    ID = stats.ClanID.Value, 
                    Tag = stats.ClanTag ?? "",
                    Name = stats.ClanTag ?? "" // FIX: Required field
                } : null 
            };
        
            matchPlayerStatistics[stats.AccountID] = new MatchPlayerStatistics(playerAccount, stats);
            
            matchPlayerInventories[stats.AccountID] = new MatchPlayerInventory 
            { 
                 AccountID = stats.AccountID, 
                 MatchID = matchStatistics.ID,
                 Slot1 = "", Slot2 = "", Slot3 = "", Slot4 = "", Slot5 = "", Slot6 = "" // TODO: Populate from Inventory History/Events if available
            };
        }

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
            ? account.SelectedStoreItems.FirstOrDefault(item => item.StartsWith("ai.custom_icon"))?.Replace("ai.custom_icon:", string.Empty) ?? "0" : "0";

    private static Dictionary<string, OneOf<global::KONGOR.MasterServer.Models.RequestResponse.SRP.StoreItemData, global::KONGOR.MasterServer.Models.RequestResponse.Store.StoreItemDiscountCoupon>> SetOwnedStoreItemsData(Account account)
    {
        // 2026-01-06: FIX - Do NOT populate metadata for standard owned items (avatars, etc.).
        // The legacy client expects 'my_upgrades_info' to contain specific data for Rentables/Coupons only.
        // Sending generic StoreItemData with empty strings for all items causes "Error when refreshing upgrades" and client logout.
        Dictionary<string, OneOf<global::KONGOR.MasterServer.Models.RequestResponse.SRP.StoreItemData, global::KONGOR.MasterServer.Models.RequestResponse.Store.StoreItemDiscountCoupon>> items = new();

        // TODO: Add Mastery Boosts And Coupons (cp. items) when implemented.
        // Legacy reference: GameConsumables.GetOwnedCoupons checks for "cp." prefix.

        return items;
    }
}
