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
        if (cookie is not null) cookie = cookie.Replace("-", string.Empty);

        if (cookie is null)
            return BadRequest(@"Missing Value For Form Parameter ""cookie""");

        string? accountName = await DistributedCache.GetAccountNameForSessionCookie(cookie);

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

        return Ok(PhpSerialization.Serialize(response));
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
            OwnedStoreItems = account.User.OwnedStoreItems,
            SelectedStoreItems = account.SelectedStoreItems,
            OwnedStoreItemsData = SetOwnedStoreItemsData(account)
        };
    }

    private async Task<IActionResult> HandleMatchStats()
    {
        string? cookie = Request.Form["cookie"];
        if (cookie is not null) cookie = cookie.Replace("-", string.Empty);

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

        MatchStatistics? matchStatistics = await MerrickContext.MatchStatistics.FirstOrDefaultAsync(matchStatistics => matchStatistics.MatchID == matchID);

        if (matchStatistics is null)
        {
            Logger.LogWarning("Match Stats Request Failed: Match Statistics Not Found For ID {MatchID}. Returning Soft Failure.", matchID);
            // Return "false" to indicate failure without triggering client HTTP error handling (which causes logout)
            return Ok(PhpSerialization.Serialize(false));
        }

        List<PlayerStatistics> allPlayerStatistics = await MerrickContext.PlayerStatistics.Where(playerStatistics => playerStatistics.MatchID == matchStatistics.ID).ToListAsync();

        string? accountName = HttpContext.Items["SessionAccountName"] as string
                              ?? await DistributedCache.GetAccountNameForSessionCookie(cookie);

        // Allow anonymous access; fetch account only if session exists
        Account? account = null;
        if (accountName is not null)
        {
            account = await MerrickContext.Accounts
                .Include(a => a.User)
                .SingleOrDefaultAsync(a => a.Name.Equals(accountName));
        }

        Account? account = await MerrickContext.Accounts
            .Include(account => account.User)
            .Include(account => account.Clan)
            .SingleOrDefaultAsync(account => account.Name.Equals(accountName));

        // Robustness: If MatchStartData is missing (expired cache), reconstruct from MatchStatistics
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

        MatchSummary matchSummary = new(matchStatistics, allPlayerStatistics, matchStartData);

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

        MatchStatsResponse response = new()
        {
            GoldCoins = account?.User.GoldCoins.ToString() ?? "0",
            SilverCoins = account?.User.SilverCoins.ToString() ?? "0",
            MatchSummary = [matchSummary],
            MatchPlayerStatistics = matchPlayerStatistics,
            MatchPlayerInventories = matchPlayerInventories,
            MatchMastery = new MatchMastery
            {
                HeroIdentifier = "Hero_Legionnaire", // TODO: Get from Match Stats (Requester's Hero?)
                CurrentMasteryExperience = 0,
                MatchMasteryExperience = 0,
                MasteryExperienceBonus = 0,
                MasteryExperienceBoost = 0,
                MasteryExperienceSuperBoost = 0,
                MasteryExperienceMaximumLevelHeroesCount = 0,
                MasteryExperienceHeroesBonus = 0,
                MasteryExperienceToBoost = 0,
                MasteryExperienceEventBonus = 0,
                MasteryExperienceCanBoost = true,
                MasteryExperienceCanSuperBoost = true,
                MasteryExperienceBoostProductIdentifier = 3609,
                MasteryExperienceSuperBoostProductIdentifier = 4605,
                MasteryExperienceBoostProductCount = 0,
                MasteryExperienceSuperBoostProductCount = 0
            },
            OwnedStoreItems = account?.User.OwnedStoreItems ?? [],
            SelectedStoreItems = account?.SelectedStoreItems ?? [],
            CustomIconSlotID = account != null ? SetCustomIconSlotID(account) : "0",
            CampaignReward = new CampaignReward()
        };

        return Ok(PhpSerialization.Serialize(response));
    }

    private static string SetCustomIconSlotID(Account account)
        => account.SelectedStoreItems.Any(item => item.StartsWith("ai.custom_icon"))
            ? account.SelectedStoreItems.FirstOrDefault(item => item.StartsWith("ai.custom_icon"))?.Replace("ai.custom_icon:", string.Empty) ?? "0" : "0";

    private static Dictionary<string, object> SetOwnedStoreItemsData(Account account)
    {
        // 2026-01-06: FIX - Do NOT populate metadata for standard owned items (avatars, etc.).
        // The legacy client expects 'my_upgrades_info' to contain specific data for Rentables/Coupons only.
        // Sending generic StoreItemData with empty strings for all items causes "Error when refreshing upgrades" and client logout.
        Dictionary<string, object> items = new();

        // TODO: Add Mastery Boosts And Coupons (cp. items) when implemented.
        // Legacy reference: GameConsumables.GetOwnedCoupons checks for "cp." prefix.

        return items;
    }
}
