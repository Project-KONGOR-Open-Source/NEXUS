using Role = MERRICK.DatabaseContext.Entities.Utility.Role;

namespace KONGOR.MasterServer.Controllers.ClientRequesterController;

public partial class ClientRequesterController
{
    private async Task<IActionResult> GetSimpleStats()
    {
        string? accountName = Request.Form["nickname"];

        if (accountName is null)
        {
            return BadRequest(@"Missing Value For Form Parameter ""nickname""");
        }

        Account? account = await MerrickContext.Accounts
            .Include(account => account.User)
            .Include(account => account.Clan)
            .FirstOrDefaultAsync(account => account.Name.Equals(accountName));

        if (account is null)
        {
            return NotFound($@"Account With Name ""{accountName}"" Was Not Found");
        }

        ShowSimpleStatsResponse response = await CreateShowSimpleStatsResponse(account);

        return Ok(PhpSerialization.Serialize(response));
    }

    private async Task<IActionResult> HandleInitStats()
    {
        string? cookie = Request.Form["cookie"];
        // 2026-01-07: REMOVED Dash Stripping. Main Controller handles fuzzy validation.
        // if (cookie is not null) cookie = cookie.Replace("-", string.Empty);

        if (cookie is null)
        {
            return BadRequest(@"Missing Value For Form Parameter ""cookie""");
        }

        string? accountName = HttpContext.Items["SessionAccountName"] as string
                              ?? await DistributedCache.GetAccountNameForSessionCookie(cookie);

        if (accountName is null)
        {
            return Unauthorized("Session Not Found");
        }

        Account? account = await MerrickContext.Accounts
            .Include(account => account.User)
            .Include(account => account.Clan)
            .FirstOrDefaultAsync(account => account.Name.Equals(accountName));

        if (account is null)
        {
            return NotFound("Account Not Found");
        }


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
            { "points", account.User.GoldCoins.ToString() },
            { "mmpoints", account.User.SilverCoins },
            { "slot_id", fullResponse.CustomIconSlotID },
            { "selected_upgrades", fullResponse.SelectedStoreItems },
            { "dice_tokens", fullResponse.DiceTokens },
            { "game_tokens", fullResponse.GameTokens },
            { "timestamp", fullResponse.ServerTimestamp },
            { "vested_threshold", fullResponse.VestedThreshold },
            {
                "quest_system",
                new Dictionary<string, object>
                {
                    {
                        "error", new Dictionary<string, int> { { "quest_status", 0 }, { "leaderboard_status", 0 } }
                    }
                }
            },
            {
                "season_system",
                new Dictionary<string, object>
                {
                    { "drop_diamonds", 0 }, { "cur_diamonds", 0 }, { "box_price", new Dictionary<int, int>() }
                }
            },
            {
                "con_reward",
                new Dictionary<string, object>
                {
                    { "old_lvl", 5 },
                    { "curr_lvl", 6 },
                    { "next_lvl", 0 },
                    { "require_rank", 0 },
                    { "need_more_play", 0 },
                    { "percentage_before", "0.92" },
                    { "percentage", "1.00" }
                }
            },
            { "0", fullResponse.Zero }
        };

        string serializedResponse = PhpSerialization.Serialize(response);
        Logger.LogInformation($"[InitStats] Response: {serializedResponse}");
        return Ok(serializedResponse);
    }

    private async Task<ShowSimpleStatsResponse> CreateShowSimpleStatsResponse(Account account)
    {
        return new ShowSimpleStatsResponse
        {
            NameWithClanTag = account.NameWithClanTag,
            ID = account.ID.ToString(),
            Level = account.User.TotalLevel,
            LevelExperience = account.User.TotalExperience,
            NumberOfAvatarsOwned = account.User.OwnedStoreItems.Count(item => item.StartsWith("aa.")),
            TotalMatchesPlayed =
                await MerrickContext.PlayerStatistics.CountAsync(stats =>
                    stats.AccountID == account.ID), // TODO: Implement Matches Played
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

        Logger.LogInformation("Received Match Stats Request: MatchID={MatchID}, Cookie={Cookie}", matchIDString,
            cookie);

        if (cookie is null)
        {
            return BadRequest(@"Missing Value For Form Parameter ""cookie""");
        }

        if (matchIDString is null)
        {
            Logger.LogError("Match Stats Request Failed: Missing Match ID");
            return BadRequest(@"Missing Value For Form Parameter ""match_id""");
        }

        if (!int.TryParse(matchIDString, out int matchID))
        {
            return BadRequest("Invalid Match ID");
        }

        MatchStatistics? matchStatistics =
            await MerrickContext.MatchStatistics.SingleOrDefaultAsync(matchStatistics =>
                matchStatistics.MatchID == matchID);

        if (matchStatistics is null)
        {
            Logger.LogWarning(
                "Match Stats Request Failed: Match Statistics Not Found For ID {MatchID}. Returning Soft Failure.",
                matchID);

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

        List<PlayerStatistics> allPlayerStatistics = await MerrickContext.PlayerStatistics
            .Where(playerStatistics => playerStatistics.MatchID == matchStatistics.MatchID).ToListAsync();

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
            MatchType = (byte) (matchStatistics.GameMode.Contains("rank") ? 1 : 0), // FIX: Infer from GameMode
            Options = MatchOptions.None, // Data loss, but allows viewing stats
            ServerName = "Unknown"
        };

        // Define matchSummary
        MatchSummary matchSummary = new(matchStatistics, allPlayerStatistics, matchStartData);

        // Populate stats for ALL players
        Dictionary<int, MatchPlayerStatistics> matchPlayerStatistics = [];
        Dictionary<int, MatchPlayerInventory> matchPlayerInventories = [];

        // Fetch real accounts
        List<int> playerAccountIDs = allPlayerStatistics.Select(s => s.AccountID).Distinct().ToList();
        List<Account> playerAccounts = await MerrickContext.Accounts
            .Include(a => a.User)
            .Include(a => a.Clan)
            .Where(a => playerAccountIDs.Contains(a.ID))
            .ToListAsync();

        // Pre-create a dummy Role for the fallback User
        Role dummyRole = new() { ID = 1, Name = "User" };

        foreach (PlayerStatistics stats in allPlayerStatistics)
        {
            // Use real account if available, otherwise create lightweight fallback
            Account? playerAccount = playerAccounts.SingleOrDefault(a => a.ID == stats.AccountID);

            if (playerAccount is null)
            {
                playerAccount = new Account
                {
                    ID = stats.AccountID,
                    Name = stats.AccountName,
                    IsMain = true,
                    User = new User
                    {
                        ID = stats.AccountID,
                        EmailAddress = "dummy@kongor.net",
                        Role = dummyRole,
                        SRPPasswordHash = "",
                        SRPPasswordSalt = ""
                    },
                    Clan = stats.ClanID.HasValue
                        ? new Clan
                        {
                            ID = stats.ClanID.Value, Tag = stats.ClanTag ?? "", Name = stats.ClanTag ?? ""
                        }
                        : null
                };
            }

            matchPlayerStatistics[stats.AccountID] = new MatchPlayerStatistics(playerAccount, stats)
            {
                TotalWonMatches = "0", // TODO: Implement
                TotalLostMatches = "0", // TODO: Implement
                TotalConcededMatches = "0", // TODO: Implement
                TotalDisconnections = "0", // TODO: Implement
                TotalKicks = "0", // TODO: Implement
                PublicMatchRating = "1500.0", // TODO: Implement
                PublicMatchCount = "0", // TODO: Implement
                SoloRankedMatchRating = "1500.0", // TODO: Implement
                SoloRankedMatchCount = "0", // TODO: Implement
                TeamRankedMatchRating = "1500.0", // TODO: Implement
                TeamRankedMatchCount = "0", // TODO: Implement
                PerformanceScore = "0.00", // TODO: Implement
                HeroIdentifier = "Hero_Backpack", // TODO: Implement Hero Identifier Mapping
                SeasonProgress = new SeasonProgress
                {
                    AccountID = stats.AccountID,
                    MatchID = stats.MatchID,
                    IsCasual = "0",
                    MMRBefore = "1500",
                    MMRAfter = "1500",
                    MedalBefore = "0",
                    MedalAfter = "0",
                    Season = "12",
                    PlacementMatches = 0,
                    PlacementWins = "0"
                }
            };

            matchPlayerInventories[stats.AccountID] = new MatchPlayerInventory
            {
                AccountID = stats.AccountID,
                MatchID = matchStatistics.ID,
                Slot1 = "",
                Slot2 = "",
                Slot3 = "",
                Slot4 = "",
                Slot5 = "",
                Slot6 = "" // TODO: Populate from Inventory History/Events if available
            };
        }

        // Build MatchMastery With Placeholder Values
        int matchMasteryExperience = 100; // TODO: Calculate Based On Match Duration And Result
        int bonusExperience = 10; // TODO: Calculate Based On Max-Level Heroes Owned

        MatchMastery matchMastery = new(
            "Hero_Gauntlet", // TODO: Get Actual Hero Identifier
            0, // TODO: Retrieve From Mastery System
            matchMasteryExperience,
            bonusExperience)
        {
            HeroIdentifier = "Hero_Gauntlet",
            CurrentMasteryExperience = 0,
            MatchMasteryExperience = matchMasteryExperience,
            MasteryExperienceBonus = 0,
            MasteryExperienceBoost = 0,
            MasteryExperienceSuperBoost = 0,
            MasteryExperienceMaximumLevelHeroesCount = 0, // TODO: Count Heroes At Max Mastery Level
            MasteryExperienceHeroesBonus = bonusExperience,
            MasteryExperienceToBoost = (matchMasteryExperience + bonusExperience) * 2,
            MasteryExperienceEventBonus = 0,
            MasteryExperienceCanBoost = true,
            MasteryExperienceCanSuperBoost = true,
            MasteryExperienceBoostProductIdentifier = 3609,
            MasteryExperienceSuperBoostProductIdentifier = 4605,
            MasteryExperienceBoostProductCount = 0, // TODO: Count "ma.Mastery Boost" Items
            MasteryExperienceSuperBoostProductCount = 0 // TODO: Count "ma.Super Mastery Boost" Items
        };

        MatchStatsResponse response = new()
        {
            GoldCoins = account?.User.GoldCoins.ToString() ?? "0",
            SilverCoins = account?.User.SilverCoins.ToString() ?? "0",
            MatchSummary = [matchSummary],
            MatchPlayerStatistics = [matchPlayerStatistics],
            MatchPlayerInventories = [matchPlayerInventories],
            MatchMastery = matchMastery,
            OwnedStoreItems = account?.User.OwnedStoreItems ?? [],
            OwnedStoreItemsData = account is not null ? SetOwnedStoreItemsData(account) : [],
            SelectedStoreItems = account?.SelectedStoreItems ?? [],
            CustomIconSlotID = account is not null ? SetCustomIconSlotID(account) : "0",
            CampaignReward = new CampaignReward() // Using Default Values From Model
        };

        string json = JsonSerializer.Serialize(response);
        string php = PhpSerialization.Serialize(response);

        return Ok(php);
    }

    private static string SetCustomIconSlotID(Account account)
    {
        return account.SelectedStoreItems.Any(item => item.StartsWith("ai.custom_icon"))
            ? account.SelectedStoreItems.FirstOrDefault(item => item.StartsWith("ai.custom_icon"))
                ?.Replace("ai.custom_icon:", string.Empty) ?? "0"
            : "0";
    }

    private static Dictionary<string, object> SetOwnedStoreItemsData(Account account)
    {
        // 2026-01-08: FIX - Aligned with Upgrades.cs.
        // We must populate my_upgrades_info for ALL owned items, otherwise the client may crash or log out when parsing stats/inventory.
        Dictionary<string, object> items = new();

        foreach (string item in account.User.OwnedStoreItems)
        {
            // We use default StoreItemData which implies permanent ownership
            items[item] = new StoreItemData();
        }

        // TODO: Add Mastery Boosts And Coupons (cp. items) when implemented.

        return items;
    }
}