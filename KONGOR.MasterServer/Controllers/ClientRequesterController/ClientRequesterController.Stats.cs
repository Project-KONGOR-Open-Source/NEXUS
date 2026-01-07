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

        List<PlayerStatistics> playerStatistics = await MerrickContext.PlayerStatistics.Where(playerStatistics => playerStatistics.MatchID == matchStatistics.MatchID).ToListAsync();

        string? accountName = HttpContext.Items["SessionAccountName"] as string
                              ?? await DistributedCache.GetAccountNameForSessionCookie(cookie);

        Account? account = null;

        if (accountName is not null)
        {
            account = await MerrickContext.Accounts.Include(account => account.User).FirstOrDefaultAsync(account => account.Name.Equals(accountName));
        }

        if (account is null && accountName is not null)
        {
            Logger.LogWarning("Match Stats Request: Session Name {AccountName} Found But Account Not In DB. Treating As Anonymous.", accountName);
        }

        // Calculate winning team (1=Legion, 2=Hellbourne)
        string winningTeam = "0";
        PlayerStatistics? firstWinner = playerStatistics.FirstOrDefault(p => p.Win == 1);
        if (firstWinner != null)
        {
            winningTeam = firstWinner.Team.ToString();
        }

        MatchSummary matchSummary = new(matchStatistics, playerStatistics, new MatchStartData
        {
            MatchName = matchStatistics.HostAccountName + "'s Game",
            ServerID = matchStatistics.ServerID,
            ServerName = matchStatistics.HostAccountName + "'s Server", // Since we don't have the server name in MatchStatistics, we default to "User's Server"
            HostAccountName = matchStatistics.HostAccountName,
            Map = matchStatistics.Map,
            Version = matchStatistics.Version,
            IsCasual = false,
            MatchType = 0,
            MatchMode = matchStatistics.GameMode,
            Options = MatchOptions.None
        })
        {
            WinningTeam = winningTeam
        };

        // Construct match_player_stats dictionary
        List<MatchPlayerStatistics> mappedPlayerStats = playerStatistics.Select(stats => new MatchPlayerStatistics
        {
            MatchID = stats.MatchID,
            AccountID = stats.AccountID,
            AccountName = stats.AccountName,
            ClanID = stats.ClanID?.ToString() ?? "0",
            HeroID = stats.HeroProductID?.ToString() ?? "0",
            Position = stats.LobbyPosition.ToString(),
            Team = stats.Team.ToString(),
            Level = stats.HeroLevel.ToString(),
            Wins = stats.Win.ToString(),
            Losses = stats.Loss.ToString(),
            Concedes = stats.Conceded.ToString(),
            ConcedeVotes = stats.ConcedeVotes.ToString(),
            Buybacks = stats.Buybacks.ToString(),
            Disconnections = stats.Disconnected.ToString(),
            Kicked = stats.Kicked.ToString(),
            PublicSkill = "1500", // TODO: Store snapshot of PSR
            PublicCount = stats.PublicMatch.ToString(),
            AMMSoloRating = "1500", // TODO: Store snapshot of MMR
            AMMSoloCount = stats.RankedMatch.ToString(),
            AMMTeamRating = "1500", // TODO: Store snapshot of Team MMR
            AMMTeamCount = "0",
            AverageScore = stats.Score.ToString(),
            HeroKills = stats.HeroKills.ToString(),
            HeroDamage = stats.HeroDamage.ToString(),
            HeroExperience = stats.HeroExperience.ToString(),
            HeroKillsGold = stats.GoldFromHeroKills.ToString(),
            HeroAssists = stats.HeroAssists.ToString(),
            Deaths = stats.HeroDeaths.ToString(),
            GoldLostToDeath = stats.GoldLostToDeath.ToString(),
            SecondsDead = stats.SecondsDead.ToString(),
            TeamCreepKills = stats.TeamCreepKills.ToString(),
            TeamCreepDamage = stats.TeamCreepDamage.ToString(),
            TeamCreepExperience = stats.TeamCreepExperience.ToString(),
            TeamCreepGold = stats.TeamCreepGold.ToString(),
            NeutralCreepKills = stats.NeutralCreepKills.ToString(),
            NeutralCreepDamage = stats.NeutralCreepDamage.ToString(),
            NeutralCreepExperience = stats.NeutralCreepExperience.ToString(),
            NeutralCreepGold = stats.NeutralCreepGold.ToString(),
            BuildingDamage = stats.BuildingDamage.ToString(),
            BuildingExperience = stats.ExperienceFromBuildings.ToString(),
            BuildingsRazed = stats.BuildingsRazed.ToString(),
            BuildingGold = stats.GoldFromBuildings.ToString(),
            Denies = stats.Denies.ToString(),
            ExperienceDenied = stats.ExperienceDenied.ToString(),
            Gold = stats.Gold.ToString(),
            GoldSpent = stats.GoldSpent.ToString(),
            Experience = stats.Experience.ToString(),
            Actions = stats.Actions.ToString(),
            Seconds = stats.SecondsPlayed.ToString(),
            Consumables = stats.ConsumablesPurchased.ToString(),
            Wards = stats.WardsPlaced.ToString(),
            TimeEarningExperience = stats.TimeEarningExperience.ToString(),
            FirstBlood = stats.FirstBlood.ToString(),
            DoubleKill = stats.DoubleKill.ToString(),
            TripleKill = stats.TripleKill.ToString(),
            QuadKill = stats.QuadKill.ToString(),
            Annihilation = stats.Annihilation.ToString(),
            KillStreak3 = stats.KillStreak03.ToString(),
            KillStreak4 = stats.KillStreak04.ToString(),
            KillStreak5 = stats.KillStreak05.ToString(),
            KillStreak6 = stats.KillStreak06.ToString(),
            KillStreak7 = stats.KillStreak07.ToString(),
            KillStreak8 = stats.KillStreak08.ToString(),
            KillStreak9 = stats.KillStreak09.ToString(),
            KillStreak10 = stats.KillStreak10.ToString(),
            KillStreak15 = stats.KillStreak15.ToString(),
            Smackdown = stats.Smackdown.ToString(),
            Humiliation = stats.Humiliation.ToString(),
            Nemesis = stats.Nemesis.ToString(),
            Retribution = stats.Retribution.ToString(),
            UsedToken = stats.UsedToken.ToString(),
            HeroIdentifier = stats.HeroProductID?.ToString() ?? "0",
            ClanTag = stats.ClanTag ?? string.Empty,
            AlternativeAvatarName = stats.AlternativeAvatarName ?? string.Empty,
            SeasonProgress = new SeasonProgress
            {
                AccountID = stats.AccountID,
                MatchID = stats.MatchID,
                IsCasual = "0",
                MMRBefore = "1500.00",
                MMRAfter = "1500.00",
                MedalBefore = "11",
                MedalAfter = "11",
                Season = "12",
                PlacementMatches = 0,
                PlacementWins = "0"
            }
        }).ToList();

        // Match Mastery
        PlayerStatistics? requesterStats = null;
        if (account is not null)
        {
            requesterStats = playerStatistics.FirstOrDefault(stats => stats.AccountID == account.ID);
        }
        MatchMastery? matchMastery = null;

        if (requesterStats != null)
        {
            matchMastery = new MatchMastery((requesterStats?.HeroProductID.ToString() ?? "Hero_Legionnaire"), 0, 0, 0)
            {
                HeroIdentifier = (requesterStats?.HeroProductID.ToString() ?? "Hero_Legionnaire"),
                CurrentMasteryExperience = 0,
                MatchMasteryExperience = 0,
                MasteryExperienceBonus = 0,
                MasteryExperienceHeroesBonus = 0,
                MasteryExperienceBoost = 0,
                MasteryExperienceSuperBoost = 0,
                MasteryExperienceMaximumLevelHeroesCount = 0,
                MasteryExperienceToBoost = 0,
                MasteryExperienceEventBonus = 0,
                MasteryExperienceCanBoost = false,
                MasteryExperienceCanSuperBoost = false,
                MasteryExperienceBoostProductIdentifier = 3609,
                MasteryExperienceSuperBoostProductIdentifier = 4605,
                MasteryExperienceBoostProductCount = 0,
                MasteryExperienceSuperBoostProductCount = 0
            };
        }

        // Inventory
        Dictionary<int, Dictionary<string, string>> inventoryDict = new();
        foreach (PlayerStatistics stat in playerStatistics)
        {
            if (stat.Inventory != null)
            {
                Dictionary<string, string> slots = new Dictionary<string, string>();
                for (int i = 0; i < stat.Inventory.Count && i < 6; i++)
                {
                    slots[$"slot_{i + 1}"] = stat.Inventory[i];
                }
                inventoryDict[stat.AccountID] = slots;
            }
        }

        Dictionary<string, object> response = new();

        // Ordered Response for Game Client
        if (account is not null)
        {
            response["selected_upgrades"] = account.SelectedStoreItems.ToArray();
        }
        else
        {
            Logger.LogInformation("Returning Anonymous Match Stats (No Selected Upgrades)");
            response["selected_upgrades"] = Array.Empty<string>();
        }
        response["match_summ"] = new Dictionary<int, object> { { matchID, matchSummary } };
        response["match_player_stats"] = new Dictionary<int, object> { { matchID, mappedPlayerStats } };

        if (matchMastery != null)
            response["match_mastery"] = matchMastery;

        response["inventory"] = new Dictionary<int, object> { { matchID, inventoryDict } };
        response["vested_threshold"] = 5;
        response["0"] = true;

        Logger.LogInformation("Successfully Retrieved Match Stats For Match ID {MatchID} Requested By {AccountName}", matchID, accountName);

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
