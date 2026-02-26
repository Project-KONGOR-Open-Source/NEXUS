using System.Globalization;
using System.Collections;

using MERRICK.DatabaseContext.Extensions;
using KONGOR.MasterServer.Models.RequestResponse.Stats;
using SimpleSeasonStats = KONGOR.MasterServer.Models.RequestResponse.Stats.SimpleSeasonStats;

namespace KONGOR.MasterServer.Services.Requester;

public static class ClientRequestHelper
{
    public static string? GetCookie(HttpRequest request)
    {
        if (request.Form.ContainsKey("cookie"))
        {
            return request.Form["cookie"];
        }

        if (request.Query.ContainsKey("cookie"))
        {
            return request.Query["cookie"];
        }

        return null;
    }

    public static ShowSimpleStatsResponse CreateShowSimpleStatsResponse(
        Account account,
        PlayerStatisticsAggregatedDTO stats,
        int currentSeason,
        IHeroDefinitionService heroDefinitions)
    {
        long totalSeconds = stats.RankedSeconds + stats.CasualSeconds;
        // Avoid division by zero
        if (totalSeconds < 1) totalSeconds = 1;

        // Helper to get hero info safely
        // Calculate awards
        // Keys derived from MatchStats.cs (Standard Submission Keys)
        // Enforce specific order for awards: Smackdowns, Annihilations, Assists, Kills
        // This ensures all users see the same awards in the same order, matching the requested design.
        List<KeyValuePair<string, int>> top4 = new List<KeyValuePair<string, int>>
        {
            new("awd_msd", stats.Smackdowns),
            new("awd_mann", stats.Annihilations),
            new("awd_masst", stats.Assists),
            new("awd_mkill", stats.Kills)
        };
        
        // Sort MVP awards by count descending (Most to Least)
        top4.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));

        // Sort Top Heroes by Seconds Played descending (Most to Least)
        stats.TopHeroes.Sort((h1, h2) => h2.SecondsPlayed.CompareTo(h1.SecondsPlayed));

        // Cubic Level Calculation: XP = 100 * Level^3
        // Level = (XP / 100)^(1/3)
        // Cap at Level 999
        long totalXp = account.User.TotalExperience > 0 ? account.User.TotalExperience : (long)(stats.RankedExp + stats.CasualExp);
        int calculatedLevel = (int)Math.Floor(Math.Pow(totalXp / 100.0, 1.0 / 3.0));
        if (calculatedLevel < 1) calculatedLevel = 1;
        if (calculatedLevel > 99) calculatedLevel = 99; // Cap at 99 to prevent K2 Engine from warning "Insufficient arg count" which silently aborts the subsequent Player Profile UI rendering stack.

        // Populate from account statistics.
        ShowSimpleStatsResponse response = new ShowSimpleStatsResponse
        {
            NameWithClanTag = account.GetNameWithClanTag(),
            ID = account.ID,
            
            Level = calculatedLevel, // Ignore User.TotalLevel which is currently holding TotalExperience data
            LevelExperience = (int)totalXp,
            NumberOfAvatarsOwned = account.User.OwnedStoreItems.Count(i => i.StartsWith("aa.", StringComparison.OrdinalIgnoreCase)),
            NumberOfHeroesOwned = account.User.OwnedStoreItems.Count(i => i.StartsWith("h.", StringComparison.OrdinalIgnoreCase)),
            TotalMatchesPlayed = stats.TotalMatches,
            TotalGamesPlayedLegacy = stats.TotalMatches,
            
            // New Fields
            AccountCreationDate = account.User.TimestampCreated.ToString("MM/dd/yyyy"),
            LastActivityDate = stats.LastMatchDate?.ToString("MM/dd/yyyy") ?? account.User.TimestampLastActive.ToString("MM/dd/yyyy"),
            TotalDisconnects = stats.Disconnected,
            RankedWins = stats.RankedWins,
            RankedLosses = stats.RankedLosses,
            
            // Initializing required fields (populated below in loop)
            FavHero1 = null!, FavHero1Time = 0,
            FavHero2 = null!, FavHero2Time = 0,
            FavHero3 = null!, FavHero3Time = 0,
            FavHero4 = null!, FavHero4Time = 0,
            FavHero5 = null!, FavHero5Time = 0,
            
            // New Required Fields for Full Identifiers
            FavHero1_2 = null!, FavHero2_2 = null!, FavHero3_2 = null!, 
            FavHero4_2 = null!, FavHero5_2 = null!,
            

            CurrentSeason = currentSeason,
            SeasonLevel = 0,
            CreepLevel = 0,
            SimpleSeasonStats = new()
            {
                RankedMatchesWon = stats.RankedWins,
                RankedMatchesLost = stats.RankedLosses,
                WinStreak = 0, // Requires match history analysis, leaving 0
                InPlacementPhase = 0,
                
                // Rank Calculation
                RankedRating = (1500.0 + stats.RankedRatingChange).ToString(CultureInfo.InvariantCulture),
                CurrentRank = ((int)RankExtensions.GetRank(1500.0 + stats.RankedRatingChange)).ToString(),
                HighestRank = ((int)RankExtensions.GetRank(1500.0 + stats.RankedRatingChange)).ToString() // Fallback to current
            },
            SimpleCasualSeasonStats = new()
            {
                RankedMatchesWon = stats.CasualWins,
                RankedMatchesLost = stats.CasualLosses,
                WinStreak = 0,
                InPlacementPhase = 0,
                
                // Casual Rank (Usually simpler or sharing logic, but using Casual change)
                RankedRating = (1500.0 + stats.CasualRatingChange).ToString(CultureInfo.InvariantCulture),
                CurrentRank = ((int)RankExtensions.GetRank(1500.0 + stats.CasualRatingChange)).ToString(),
                HighestRank = ((int)RankExtensions.GetRank(1500.0 + stats.CasualRatingChange)).ToString()
            },
            SimpleMidWarsSeasonStats = new()
            {
                RankedMatchesWon = stats.CasualWins, // Fallback to Casual for now
                RankedMatchesLost = stats.CasualLosses,
                WinStreak = 0,
                InPlacementPhase = 0,
                RankedRating = (1500.0 + stats.CasualRatingChange).ToString(CultureInfo.InvariantCulture),
                CurrentRank = ((int)RankExtensions.GetRank(1500.0 + stats.CasualRatingChange)).ToString(),
                HighestRank = ((int)RankExtensions.GetRank(1500.0 + stats.CasualRatingChange)).ToString()
            },
            MVPAwardsCount = stats.MVP.ToString(),
            Top4AwardNames = top4.Select(x => x.Key).ToList(),
            Top4AwardCounts = top4.Select(x => x.Value.ToString()).ToList(),
            
            CustomIconSlotID = GetCustomIconSlotID(account),
            OwnedStoreItems = account.User.OwnedStoreItems ?? new List<string>(),
            SelectedStoreItems = account.SelectedStoreItems ?? new List<string>(),
            OwnedStoreItemsData = SetOwnedStoreItemsData(account),
            DiceTokens = "1", // Hardcoded string for parity
            GameTokens = 0,
            ServerTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            
            // Statistics Top Level Bindings
            HeroKills = stats.RankedKills.ToString(),
            Deaths = stats.RankedDeaths.ToString(),
            HeroAssists = stats.RankedAssists.ToString(),
            KDA = $"{stats.RankedKills}/{stats.RankedDeaths}/{stats.RankedAssists}",
            AvgGameLength = stats.RankedMatches > 0 ? (stats.RankedTimeEarningExp / (float)stats.RankedMatches / 60.0f).ToString("0.0") : "0",
            AvgXPMin = stats.RankedTimeEarningExp > 0 ? (stats.RankedExp / ((float)stats.RankedTimeEarningExp / 60.0f)).ToString("0.0") : "0",
            AvgDenies = stats.RankedMatches > 0 ? (stats.RankedDenies / (float)stats.RankedMatches).ToString("0.0") : "0",
            AvgCreepKills = stats.RankedMatches > 0 ? (stats.RankedTeamCreepKills / (float)stats.RankedMatches).ToString("0.0") : "0",
            AvgNeutralKills = stats.RankedMatches > 0 ? (stats.RankedNeutralCreepKills / (float)stats.RankedMatches).ToString("0.0") : "0",
            AvgActionsMin = stats.RankedTimeEarningExp > 0 ? (stats.RankedActions / ((float)stats.RankedTimeEarningExp / 60.0f)).ToString("0.0") : "0",
            AvgWardsUsed = stats.RankedMatches > 0 ? (stats.RankedWards / (float)stats.RankedMatches).ToString("0.0") : "0",
            Gold = stats.RankedHeroGold.ToString(),
            
            Humiliation = stats.RankedHumiliations.ToString(),
            Smackdown = stats.RankedSmackdowns.ToString(),
            Nemesis = stats.RankedNemesis.ToString(),
            Retribution = stats.RankedRetribution.ToString(),
            
            // Top Level Rank Fields (Defaulting to Ranked/Normal stats)
            Rank = ((int)RankExtensions.GetRank(1500.0 + stats.RankedRatingChange)).ToString(),
            CurrentRankTop = ((int)RankExtensions.GetRank(1500.0 + stats.RankedRatingChange)).ToString(),
            RankedRatingTop = (1500.0 + stats.RankedRatingChange).ToString(CultureInfo.InvariantCulture),
            HighestRankTop = ((int)RankExtensions.GetRank(1500.0 + stats.RankedRatingChange)).ToString(),
            
            // Mastery Fields
            MasteryInfo = GenerateMasteryInfo(stats.TopHeroes, heroDefinitions),
            MasteryRewards = GenerateMasteryRewards()

        };

        for (int i = 0; i < 5; i++)
        {
            if (stats.TopHeroes.Count > i)
            {
                FavHeroDTO hero = stats.TopHeroes[i];

                // Format Name: "Hero_Jereziah" (Keep raw for correct client lookup)
                string identifier = heroDefinitions.GetHeroIdentifier(hero.HeroId);
                string formattedName = identifier.ToLowerInvariant().Replace("hero_", "");
                // string formattedName = identifier;

                // Populate property by index
                // FavHeroX -> Short Name (Texture Lookup, e.g. "jereziah")
                // FavHeroX_2 -> Full Identifier (Display Name, e.g. "Hero_Jereziah")
                string propName = $"FavHero{i + 1}";
                string propName2 = $"FavHero{i + 1}_2";
                string timeProp = $"FavHero{i + 1}Time";

                PropertyInfo? unknownProp = response.GetType().GetProperty(propName);
                if (unknownProp != null) unknownProp.SetValue(response, formattedName);

                PropertyInfo? unknownProp2 = response.GetType().GetProperty(propName2);
                if (unknownProp2 != null) unknownProp2.SetValue(response, identifier);

                PropertyInfo? unknownTime = response.GetType().GetProperty(timeProp);
                if (unknownTime != null) unknownTime.SetValue(response, (int)Math.Round((double)hero.SecondsPlayed / totalSeconds * 100));
            }
        }
        return response;
    }
    
    public static List<Dictionary<string, object>> GenerateMasteryInfo(List<FavHeroDTO> heroes, IHeroDefinitionService heroDefinitions)
    {
        List<Dictionary<string, object>> list = new();

        for (int i = 0; i < heroes.Count; i++)
        {
            FavHeroDTO h = heroes[i];
            string identifier = heroDefinitions.GetHeroIdentifier(h.HeroId);
            long xp = h.SecondsPlayed * 5;

            // The original S2 Client Engine natively expects an array of objects
            // where each object contains 'heroname' and 'exp' keys!
            Dictionary<string, object> heroDict = new Dictionary<string, object>
            {
                { "heroname", identifier },
                { "exp", (int)xp }
            };

            list.Add(heroDict);
        }

        Console.WriteLine($"[ClientRequestHelper] GenerateMasteryInfo generated {list.Count} iterative dictionary records for native C++ json unrolling.");
        return list;
    }
    
    public static List<Dictionary<string, object>> GenerateMasteryRewards()
    {
        // Logan (2025-02-25): The S2 client's K2 engine strictly requires mastery_rewards to be an indexed PHP array
        // containing associative arrays describing level milestones. Mocking real tokens via client paths!
        List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();

        var rewards = new[]
        {
            new { Level = 5, Qty = 250, Icon = "/ui/icons/silver_coin_stack.tga", Name = "Silver Coins" },
            new { Level = 10, Qty = 50, Icon = "/ui/icons/gold_coin_stack.tga", Name = "Gold Coins" },
            new { Level = 15, Qty = 150, Icon = "/ui/icons/tickets_stack.tga", Name = "Plinko Tickets" },
            new { Level = 20, Qty = 500, Icon = "/ui/icons/gold_coin_stack.tga", Name = "Gold Coins" }
        };

        foreach (var r in rewards)
        {
            Dictionary<string, object> rewardDef = new Dictionary<string, object>
            {
                { "product_id", 3600 + r.Level },
                { "product_name", r.Name },
                { "product_local_content", r.Icon },
                { "quantity", r.Qty },
                { "points", 0 },
                { "mmpoints", 0 },
                { "tickets", 0 }
            };

            Dictionary<string, object> levelBadge = new Dictionary<string, object>
            {
                { "level", r.Level },
                { "alreadygot", 0 }, // K2 predominantly parses 0/1 ints instead of php booleans natively.
                { "reward", rewardDef }
            };

            list.Add(levelBadge);
        }
        return list;
    }

    public static string SetCustomIconSlotID(Account account)
    {
        return GetCustomIconSlotID(account);
    }

    private static string GetCustomIconSlotID(Account account)
    {
        // The client expects the 1-BASED INDEX of the equipped icon in the selected_upgrades list.
        // If no icon is equipped, it expects "0".
        // Note: For ID, strict typing might require it to be a string or int depending on context,
        // but here it returns string which fits 'CustomIconSlotID'.
        if (account.SelectedStoreItems == null) return "0";

        int index = account.SelectedStoreItems.FindIndex(i => i.StartsWith("ai.", StringComparison.OrdinalIgnoreCase));
        return index >= 0 ? (index + 1).ToString(CultureInfo.InvariantCulture) : "0";
    }

    public static Dictionary<string, OneOf<object, Dictionary<string, object>>> SetOwnedStoreItemsData(Account account)
    {
        Dictionary<string, OneOf<object, Dictionary<string, object>>> data = new();
        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        long forever = now + 999999999;

        if (account.User.OwnedStoreItems != null)
        {
            foreach (string item in account.User.OwnedStoreItems)
            {
                data[item] = new Dictionary<string, object>
                {
                    { "data", "" },
                    { "start_time", now.ToString() },
                    { "end_time", forever.ToString() }, // Log shows 33325185006
                    { "used", 0 },
                    { "score", "0" },
                    { "expiration_date", "0" },
                    { "perm", "1" }
                };
            }
        }
        return data;
    }
    private static Dictionary<object, object> ToHashtable(global::KONGOR.MasterServer.Models.RequestResponse.Stats.SimpleSeasonStats stats, bool isCasual = false)
    {
        Dictionary<object, object> t = new Dictionary<object, object>();
        
        // Standard Keys (Logan 2025-02-13: Convert to String for Legacy Client)
        t["wins"] = stats.RankedMatchesWon.ToString();
        t["losses"] = stats.RankedMatchesLost.ToString();
        t["h_wins"] = stats.HardcoreWins.ToString();
        t["h_losses"] = stats.HardcoreLosses.ToString();
        t["con_wins"] = stats.CasualWins.ToString();
        t["con_losses"] = stats.CasualLosses.ToString();
        t["win_streak"] = stats.WinStreak.ToString();
        t["rank"] = stats.CurrentRank?.ToString() ?? "0";
        t["current_level"] = stats.LegacyLevel?.ToString() ?? "0";
        t["smr"] = stats.RankedRating?.ToString() ?? "1500";
        t["highest_level_current"] = stats.HighestRank?.ToString() ?? "0";
        t["is_placement"] = stats.InPlacementPhase.ToString();
        t["pub_skill"] = stats.PublicRating?.ToString() ?? "1500";
        t["kam_rating"] = stats.CasualRating?.ToString() ?? "1500";
        
        // Legacy Aliases (Explicitly added to ensure serialization)
        // Lua expects these for Normal ('rnk_') and Casual ('cs_')
        if (isCasual)
        {
            t["cs_wins"] = stats.CasualWins.ToString();
            t["cs_losses"] = stats.CasualLosses.ToString();
            t["cs_games_played"] = (stats.CasualWins + stats.CasualLosses).ToString(); 
            // Add other cs_ keys if needed

            if (stats.GenericStats != null)
            {
                PlayerStatisticsAggregatedDTO? g = stats.GenericStats;
                t["cs_denies"] = g.CasualDenies.ToString();
                t["cs_herodmg"] = g.CasualHeroDamage.ToString();
                t["cs_heroexp"] = "0"; // Derived or missing? 
                t["cs_herokillsgold"] = g.CasualHeroGold.ToString();
                t["cs_heroassists"] = g.CasualAssists.ToString();
                t["cs_deaths"] = g.CasualDeaths.ToString();
                t["cs_herokills"] = g.CasualKills.ToString(); // herokills = kills
                t["cs_goldlost2death"] = g.CasualGoldLost.ToString();
                t["cs_secs_dead"] = g.CasualSecondsDead.ToString();
                t["cs_teamcreepkills"] = g.CasualTeamCreepKills.ToString();
                t["cs_teamcreepdmg"] = g.CasualTeamCreepDmg.ToString();
                t["cs_teamcreepgold"] = g.CasualTeamCreepGold.ToString();
                t["cs_teamcreepexp"] = g.CasualTeamCreepExp.ToString();
                t["cs_neutralcreepkills"] = g.CasualNeutralCreepKills.ToString();
                t["cs_neutralcreepdmg"] = g.CasualNeutralCreepDmg.ToString();
                t["cs_neutralcreepgold"] = g.CasualNeutralCreepGold.ToString();
                t["cs_neutralcreepexp"] = g.CasualNeutralCreepExp.ToString();
                t["cs_bdmg"] = g.CasualBuildingDmg.ToString();
                t["cs_razed"] = g.CasualBuildingsRazed.ToString();
                t["cs_bdmgexp"] = g.CasualBuildingExp.ToString();
                t["cs_bgold"] = g.CasualBuildingGold.ToString();
                t["cs_exp_denied"] = g.CasualExpDenied.ToString();
                t["cs_gold"] = g.CasualGold.ToString();
                t["cs_gold_spend"] = g.CasualGoldSpent.ToString();
                t["cs_actions"] = g.CasualActions.ToString();
                t["cs_consumables"] = g.CasualConsumables.ToString();
                t["cs_wards"] = g.CasualWards.ToString();
                t["cs_bloodlust"] = g.CasualFirstBloods.ToString();
                t["cs_doublekill"] = g.CasualDoubleKills.ToString();
                t["cs_triplekill"] = g.CasualTripleKills.ToString();
                t["cs_quadkill"] = g.CasualQuadKills.ToString();
                t["cs_annihilation"] = g.CasualAnnihilations.ToString();
                t["cs_ks3"] = g.CasualKS3.ToString();
                t["cs_ks4"] = g.CasualKS4.ToString();
                t["cs_ks5"] = g.CasualKS5.ToString();
                t["cs_ks6"] = g.CasualKS6.ToString();
                t["cs_ks7"] = g.CasualKS7.ToString();
                t["cs_ks8"] = g.CasualKS8.ToString();
                t["cs_ks9"] = g.CasualKS9.ToString();
                t["cs_ks10"] = g.CasualKS10.ToString();
                t["cs_ks15"] = g.CasualKS15.ToString();
                t["cs_smackdown"] = g.CasualSmackdowns.ToString();
                t["cs_humiliation"] = g.CasualHumiliations.ToString();
                t["cs_nemesis"] = g.CasualNemesis.ToString();
                t["cs_retribution"] = g.CasualRetribution.ToString();
                t["cs_time_earning_exp"] = g.CasualTimeEarningExp.ToString();
                t["cs_buybacks"] = g.CasualBuybacks.ToString();
                
                // Map cam_cs_ aliases
                t["cam_cs_denies"] = t["cs_denies"];
                t["cam_cs_herodmg"] = t["cs_herodmg"];
                t["cam_cs_herokills"] = t["cs_herokills"];
                t["cam_cs_deaths"] = t["cs_deaths"];
                t["cam_cs_gold"] = t["cs_gold"];
                t["cam_cs_exp"] = g.CasualExp.ToString();
                t["cam_cs_level"] = "0"; // Calc?
                
                // Basic missing ones
                t["cs_exp"] = g.CasualExp.ToString();
                t["cs_secs"] = g.CasualSeconds.ToString();
                t["cam_cs_secs"] = t["cs_secs"];
                t["cam_cs_games_played"] = t["cs_games_played"];
                t["cam_cs_wins"] = t["cs_wins"];
                t["cam_cs_losses"] = t["cs_losses"];
            }
        }
        else
        {
            t["rnk_wins"] = stats.RankedMatchesWon.ToString();
            t["rnk_losses"] = stats.RankedMatchesLost.ToString();
            t["rnk_games_played"] = (stats.RankedMatchesWon + stats.RankedMatchesLost).ToString();
            t["acc_wins"] = stats.RankedMatchesWon.ToString();
            t["acc_games_played"] = (stats.RankedMatchesWon + stats.RankedMatchesLost).ToString();

            if (stats.GenericStats != null)
            {
                PlayerStatisticsAggregatedDTO? g = stats.GenericStats;
                t["rnk_denies"] = g.RankedDenies.ToString();
                t["rnk_herodmg"] = g.RankedHeroDamage.ToString();
                t["rnk_herokillsgold"] = g.RankedHeroGold.ToString();
                t["rnk_heroassists"] = g.RankedAssists.ToString(); // Use base RankedAssists from DTO
                t["rnk_deaths"] = g.RankedDeaths.ToString();
                t["rnk_herokills"] = g.RankedKills.ToString();
                t["rnk_goldlost2death"] = g.RankedGoldLost.ToString();
                t["rnk_secs_dead"] = g.RankedSecondsDead.ToString();
                t["rnk_teamcreepkills"] = g.RankedTeamCreepKills.ToString();
                t["rnk_teamcreepdmg"] = g.RankedTeamCreepDmg.ToString();
                t["rnk_teamcreepgold"] = g.RankedTeamCreepGold.ToString();
                t["rnk_teamcreepexp"] = g.RankedTeamCreepExp.ToString();
                t["rnk_neutralcreepkills"] = g.RankedNeutralCreepKills.ToString();
                t["rnk_neutralcreepdmg"] = g.RankedNeutralCreepDmg.ToString();
                t["rnk_neutralcreepgold"] = g.RankedNeutralCreepGold.ToString();
                t["rnk_neutralcreepexp"] = g.RankedNeutralCreepExp.ToString();
                t["rnk_bdmg"] = g.RankedBuildingDmg.ToString();
                t["rnk_razed"] = g.RankedBuildingsRazed.ToString();
                t["rnk_bdmgexp"] = g.RankedBuildingExp.ToString();
                t["rnk_bgold"] = g.RankedBuildingGold.ToString();
                t["rnk_exp_denied"] = g.RankedExpDenied.ToString();
                t["rnk_gold"] = g.RankedGold.ToString();
                t["rnk_gold_spend"] = g.RankedGoldSpent.ToString();
                t["rnk_actions"] = g.RankedActions.ToString();
                t["rnk_consumables"] = g.RankedConsumables.ToString();
                t["rnk_wards"] = g.RankedWards.ToString();
                t["rnk_bloodlust"] = g.RankedFirstBloods.ToString();
                t["rnk_doublekill"] = g.RankedDoubleKills.ToString();
                t["rnk_triplekill"] = g.RankedTripleKills.ToString();
                t["rnk_quadkill"] = g.RankedQuadKills.ToString();
                t["rnk_annihilation"] = g.RankedAnnihilations.ToString();
                t["rnk_ks3"] = g.RankedKS3.ToString();
                t["rnk_ks4"] = g.RankedKS4.ToString();
                t["rnk_ks5"] = g.RankedKS5.ToString();
                t["rnk_ks6"] = g.RankedKS6.ToString();
                t["rnk_ks7"] = g.RankedKS7.ToString();
                t["rnk_ks8"] = g.RankedKS8.ToString();
                t["rnk_ks9"] = g.RankedKS9.ToString();
                t["rnk_ks10"] = g.RankedKS10.ToString();
                t["rnk_ks15"] = g.RankedKS15.ToString();
                t["rnk_smackdown"] = g.RankedSmackdowns.ToString();
                t["rnk_humiliation"] = g.RankedHumiliations.ToString();
                t["rnk_nemesis"] = g.RankedNemesis.ToString();
                t["rnk_retribution"] = g.RankedRetribution.ToString();
                t["rnk_time_earning_exp"] = g.RankedTimeEarningExp.ToString();
                t["rnk_buybacks"] = g.RankedBuybacks.ToString();

                // Map cam_ aliases (Campaign/Normal)
                t["cam_denies"] = t["rnk_denies"];
                t["cam_herodmg"] = t["rnk_herodmg"];
                t["cam_herokills"] = t["rnk_herokills"];
                t["cam_deaths"] = t["rnk_deaths"];
                t["cam_gold"] = t["rnk_gold"];
                t["cam_exp"] = g.RankedExp.ToString();
                t["cam_secs"] = g.RankedSeconds.ToString();
                
                // Basic missing ones
                t["rnk_exp"] = g.RankedExp.ToString();
                t["rnk_secs"] = g.RankedSeconds.ToString();
                
                // Logan (2025-02-15): Fix for Profile Overview Blank Stats
                // Lua expects 'cam_' keys for the normal season stats, but we only provided 'rnk_'.
                // Mapping required for: wins, losses, discos, concedes, concedevotes, buybacks, 
                // herokills, heroassists, deaths, gold, exp, secs, consumables, wards, bloodlust, etc.
                
                t["cam_wins"] = t["rnk_wins"];
                t["cam_losses"] = t["rnk_losses"];
                t["cam_discos"] = (isCasual ? stats.GenericStats?.CasualDiscos : stats.GenericStats?.RankedDiscos)?.ToString() ?? "0";
                t["cam_concedes"] = "0";      // Missing in DTO
                t["cam_concedevotes"] = "0";  // Missing in DTO
                t["cam_buybacks"] = t["rnk_buybacks"];
                t["cam_herokills"] = t["rnk_herokills"];
                t["cam_heroassists"] = t["rnk_heroassists"];
                t["cam_deaths"] = t["rnk_deaths"];
                t["cam_gold"] = t["rnk_gold"];
                t["cam_exp"] = t["rnk_exp"];
                t["cam_secs"] = t["rnk_secs"];
                t["cam_consumables"] = t["rnk_consumables"];
                t["cam_wards"] = t["rnk_wards"];
                t["cam_bloodlust"] = t["rnk_bloodlust"];
                t["cam_doublekill"] = t["rnk_doublekill"];
                t["cam_triplekill"] = t["rnk_triplekill"];
                t["cam_quadkill"] = t["rnk_quadkill"];
                t["cam_annihilation"] = t["rnk_annihilation"];
                t["cam_ks3"] = t["rnk_ks3"];
                t["cam_ks4"] = t["rnk_ks4"];
                t["cam_ks5"] = t["rnk_ks5"];
                t["cam_ks6"] = t["rnk_ks6"];
                t["cam_ks7"] = t["rnk_ks7"];
                t["cam_ks8"] = t["rnk_ks8"];
                t["cam_ks9"] = t["rnk_ks9"];
                t["cam_ks10"] = t["rnk_ks10"];
                t["cam_ks15"] = t["rnk_ks15"];
                t["cam_smackdown"] = t["rnk_smackdown"];
                t["cam_humiliation"] = t["rnk_humiliation"];
                t["cam_nemesis"] = t["rnk_nemesis"];
                t["cam_retribution"] = t["rnk_retribution"];
                t["cam_teamkillexp"] = t["rnk_teamcreepexp"];
                t["cam_teamkillgold"] = t["rnk_teamcreepgold"];
                t["cam_actions"] = t["rnk_actions"];
                t["cam_amm_team_rating"] = "0.000"; // Legacy Matchmaking Rating
                t["cam_level"] = "0"; // Mapped to arg[102] in some contexts
            }
        }

        // CoN Reward (Required for Overview Page to prevent crash)
        // Format: "0,0,0,0,0,0,0" (Current Level, ?, ?, ?, ?, ?, Percentage)
        // Lua uses string.find(con_reward, "(.+),(.+),(.+),(.+),(.+),(.+),(.+)")
        // Generate placeholder reward string derived from Level if no real data is available yet
        // This ensures the client displays *something* (like a chest and progress) instead of a broken UI.
        // Assuming:
        // Index 1: Reward Level? (Using stats.LegacyLevel)
        // Index 7: Percentage (0.0-1.0)

        string levelStr = stats.LegacyLevel?.ToString() ?? "1";

        // Calculate Percentage based on XP?
        // Reuse XP logic from DTO if available?
        // For now, we don't have XP in SimpleSeasonStats directly easily,
        // but we can parse it or pass it.
        // Or default to 0.

        // Use a generic placeholder that is valid.
        // If we want to show 0%, use 0.
        // If we want to test with 66% (0.66) as per legacy/test requirements, we can't hardcode it for prod.
        // Prod should reflect reality.
        // Reality: We don't track CoN Rewards fully yet.
        // Best effort: "Level,1,3,2,11,0,0"

        t["con_reward"] = $"{levelStr},1,3,2,11,0,0";
        t["cam_con_reward"] = $"{levelStr},1,3,2,11,0,0";

        // Logan (2025-02-13): Debug Log to Verify Keys
        Console.WriteLine($"[ClientRequestHelper] ToHashtable (Casual={isCasual}) Keys: " + string.Join(", ", t.Keys.Cast<string>()));
        
        return t;
    }
    
    public static object[] CreateLegacyPositionalPlayerStats(ShowSimpleStatsResponse props)
    {
        PlayerStatisticsAggregatedDTO stats = props.SimpleSeasonStats.GenericStats ?? new PlayerStatisticsAggregatedDTO();
        
        long totalXp = props.LevelExperience;
        int currentLevel = props.Level;
    
        int currentLevelBaseXp = 100 * (int)Math.Pow(currentLevel, 3);
        int nextLevelBaseXp = 100 * (int)Math.Pow(currentLevel + 1, 3);
        
        float levelPercent = 0;
        if (nextLevelBaseXp > currentLevelBaseXp)
        {
            levelPercent = (float)(totalXp - currentLevelBaseXp) / (nextLevelBaseXp - currentLevelBaseXp) * 100f;
            if (levelPercent < 0) levelPercent = 0;
            if (levelPercent > 100) levelPercent = 100;
        }

        object[] array = new object[149];
        // 1-based mapped to 0-based array.
        array[0] = props.NameWithClanTag ?? "0";
        array[1] = "0"; // name
        array[2] = props.CurrentRankTop ?? "0";
        array[3] = props.SeasonLevel.ToString();
        array[4] = props.ID.Value.ToString() ?? "0";
        array[5] = stats.RankedMatches.ToString();
        array[6] = props.RankedWins.ToString();
        array[7] = props.RankedLosses.ToString();
        array[8] = "0"; // cam_concedes
        array[9] = "0"; // cam_concedevotes
        array[10] = "0"; // cam_buybacks
        array[11] = stats.RankedDiscos.ToString();
        array[12] = "0"; // cam_kicked
        array[13] = props.RankedRatingTop ?? "1500.000";
        array[14] = "0"; // cam_pub_count
        array[15] = props.RankedRatingTop ?? "1500.000";
        array[16] = stats.RankedMatches.ToString();
        array[17] = props.RankedRatingTop ?? "1500.000";
        array[18] = stats.RankedMatches.ToString();
        array[19] = "0"; // rnk_avg_score
        array[20] = stats.RankedKills.ToString();
        array[21] = stats.RankedHeroDamage.ToString();
        array[22] = "0"; // cam_heroexp
        array[23] = stats.RankedHeroGold.ToString();
        array[24] = stats.RankedAssists.ToString();
        array[25] = stats.RankedDeaths.ToString();
        array[26] = stats.RankedGoldLost.ToString();
        array[27] = stats.RankedSecondsDead.ToString();
        array[28] = stats.RankedTeamCreepKills.ToString();
        array[29] = stats.RankedTeamCreepDmg.ToString();
        array[30] = stats.RankedTeamCreepExp.ToString();
        array[31] = stats.RankedTeamCreepGold.ToString();
        array[32] = stats.RankedNeutralCreepKills.ToString();
        array[33] = stats.RankedNeutralCreepDmg.ToString();
        array[34] = stats.RankedNeutralCreepExp.ToString();
        array[35] = stats.RankedNeutralCreepGold.ToString();
        array[36] = stats.RankedBuildingDmg.ToString();
        array[37] = stats.RankedBuildingExp.ToString();
        array[38] = stats.RankedBuildingsRazed.ToString();
        array[39] = stats.RankedBuildingGold.ToString();
        array[40] = stats.RankedDenies.ToString();
        array[41] = stats.RankedExpDenied.ToString();
        array[42] = stats.RankedGold.ToString();
        array[43] = stats.RankedGoldSpent.ToString();
        array[44] = stats.RankedExp.ToString();
        array[45] = stats.RankedActions.ToString();
        array[46] = stats.RankedTimeEarningExp.ToString();
        array[47] = stats.RankedConsumables.ToString();
        array[48] = stats.RankedWards.ToString();
        array[49] = "0"; // cam_em_played
        array[50] = "0"; // maxXP
        array[51] = props.LastActivityDate ?? "0";
        array[52] = "0"; // matchIds
        array[53] = "0"; // matchDates
        array[54] = props.FavHero1 ?? "0";
        array[55] = props.FavHero2 ?? "0";
        array[56] = props.FavHero3 ?? "0";
        array[57] = props.FavHero4 ?? "0";
        array[58] = props.FavHero5 ?? "0";
        array[59] = props.FavHero1Time.ToString();
        array[60] = props.FavHero2Time.ToString();
        array[61] = props.FavHero3Time.ToString();
        array[62] = props.FavHero4Time.ToString();
        array[63] = props.FavHero5Time.ToString();
        array[64] = "0"; // xp2nextLevel
        array[65] = "0"; // xpPercent
        array[66] = "0"; // percentEM
        array[67] = $"{stats.RankedKills}/{stats.RankedDeaths}/{stats.RankedAssists}"; // k_d_a
        array[68] = stats.RankedMatches > 0 ? (stats.RankedTimeEarningExp / (float)stats.RankedMatches / 60.0f).ToString("0.0") : "0"; // avgGameLength
        array[69] = stats.RankedTimeEarningExp > 0 ? (stats.RankedExp / ((float)stats.RankedTimeEarningExp / 60.0f)).ToString("0.0") : "0"; // avgXP_min
        array[70] = stats.RankedMatches > 0 ? (stats.RankedDenies / (float)stats.RankedMatches).ToString("0.0") : "0"; // avgDenies
        array[71] = stats.RankedMatches > 0 ? (stats.RankedTeamCreepKills / (float)stats.RankedMatches).ToString("0.0") : "0"; // avgCreepKills
        array[72] = stats.RankedMatches > 0 ? (stats.RankedNeutralCreepKills / (float)stats.RankedMatches).ToString("0.0") : "0"; // avgNeutralKills
        array[73] = stats.RankedTimeEarningExp > 0 ? (stats.RankedActions / ((float)stats.RankedTimeEarningExp / 60.0f)).ToString("0.0") : "0"; // avgActions_min
        array[74] = stats.RankedMatches > 0 ? (stats.RankedWards / (float)stats.RankedMatches).ToString("0.0") : "0"; // avgWardsUsed
        array[75] = props.AccountCreationDate ?? "0";
        array[76] = props.FavHero1_2 ?? "0";
        array[77] = props.FavHero2_2 ?? "0";
        array[78] = props.FavHero3_2 ?? "0";
        array[79] = props.FavHero4_2 ?? "0";
        array[80] = props.FavHero5_2 ?? "0";
        array[81] = "0"; // favHero1id
        array[82] = "0"; // favHero2id
        array[83] = "0"; // favHero3id
        array[84] = "0"; // favHero4id
        array[85] = "0"; // favHero5id
        array[86] = "0"; // error
        array[87] = props.SeasonLevel.ToString(); // cam_level duplicate?
        array[88] = "0"; // selected_upgrades
        array[89] = props.TotalMatchesPlayed.ToString();
        array[90] = stats.CasualMatches.ToString();
        array[91] = props.TotalDisconnects.ToString();
        array[92] = stats.CasualDiscos.ToString();
        array[93] = stats.RankedFirstBloods.ToString();
        array[94] = stats.RankedDoubleKills.ToString();
        array[95] = stats.RankedTripleKills.ToString();
        array[96] = stats.RankedQuadKills.ToString();
        array[97] = stats.RankedAnnihilations.ToString();
        array[98] = stats.RankedKS3.ToString();
        array[99] = stats.RankedKS4.ToString();
        array[100] = stats.RankedKS5.ToString();
        array[101] = stats.RankedKS6.ToString();
        array[102] = stats.RankedKS7.ToString();
        array[103] = stats.RankedKS8.ToString();
        array[104] = stats.RankedKS9.ToString();
        array[105] = stats.RankedKS10.ToString();
        array[106] = stats.RankedKS15.ToString();
        array[107] = stats.RankedSmackdowns.ToString();
        array[108] = stats.RankedHumiliations.ToString();
        array[109] = stats.RankedNemesis.ToString();
        array[110] = stats.RankedRetribution.ToString();
        array[111] = props.LevelExperience.ToString();
        array[112] = stats.RankedTimeEarningExp.ToString();
        array[113] = props.Level.ToString(); // level
        array[114] = props.LevelExperience.ToString(); // level_exp
        array[115] = props.TotalDisconnects.ToString();
        array[116] = "0"; // possible_discos
        array[117] = props.TotalMatchesPlayed.ToString();
        array[118] = "1"; // account_type
        array[119] = "100"; // standing
        array[120] = levelPercent.ToString(System.Globalization.CultureInfo.InvariantCulture); // level_percent
        array[121] = nextLevelBaseXp.ToString(); // max_exp
        array[122] = currentLevelBaseXp.ToString(); // min_exp
        array[123] = stats.PublicMatches.ToString(); // mid_games_played
        array[124] = stats.PublicDiscos.ToString(); // mid_discos
        array[125] = props.TotalMatchesPlayed.ToString(); // total_games_played
        array[126] = props.TotalDisconnects.ToString();
        array[127] = "0"; // event_id
        array[128] = "0"; // events
        array[129] = "0"; // uncs_discos
        array[130] = "0"; // unrnk_discos
        array[131] = "0"; // uncs_games_played
        array[132] = "0"; // unrnk_games_played
        array[133] = "0"; // rift_games_played
        array[134] = "0"; // rift_discos
        array[135] = props.CurrentRankTop ?? "0"; 
        array[136] = props.HighestRankTop ?? "0";
        array[137] = props.CurrentSeason.ToString();
        array[138] = props.RankedRatingTop ?? "1500.000";
        array[139] = stats.RankedMatches.ToString();
        array[140] = stats.CasualMatches.ToString();
        array[141] = stats.RankedDiscos.ToString();
        array[142] = stats.CasualDiscos.ToString();
        array[143] = "0"; // prev seasons
        array[144] = "0"; // prev seasons cs
        array[145] = "0"; // prev seasons discos
        array[146] = "0"; // prev seasons cs
        array[147] = props.RankedRatingTop ?? "1500.000"; // highest_ranking
        array[148] = props.ConReward ?? "0";
        
        return array;
    }

    public static Dictionary<object, object> CreateHybridSimpleStats(ShowSimpleStatsResponse props, string? table = null)
    {
        Dictionary<object, object> hybridData = new Dictionary<object, object>();

        // 1. Populate Named Keys (Hybrid/Modern Support)
        foreach (PropertyInfo prop in typeof(ShowSimpleStatsResponse).GetProperties())
        {
            PHPPropertyAttribute? attr = prop.GetCustomAttribute<PHPPropertyAttribute>();
            if (attr != null)
            {
                // Logan (2025-02-13): Explicitly convert nested stats objects to Hashtables
                // to ensure aliases are serialized correctly.
                if (prop.Name == nameof(ShowSimpleStatsResponse.SimpleSeasonStats))
                {
                    SimpleSeasonStats val = props.SimpleSeasonStats ?? new() 
                    { 
                        RankedMatchesWon = 0, RankedMatchesLost = 0, WinStreak = 0, 
                        CurrentRank = "0.000", RankedRating = "1500.000", InPlacementPhase = 0 
                    };
                    hybridData[attr.PropertyKey.Value] = ToHashtable(val, isCasual: false);
                }
                else if (prop.Name == nameof(ShowSimpleStatsResponse.SimpleCasualSeasonStats))
                {
                     SimpleSeasonStats val = props.SimpleCasualSeasonStats ?? new() 
                     { 
                        RankedMatchesWon = 0, RankedMatchesLost = 0, WinStreak = 0, 
                        CurrentRank = "0.000", RankedRating = "1500.000", InPlacementPhase = 0 
                     };
                     hybridData[attr.PropertyKey.Value] = ToHashtable(val, isCasual: true);
                }
                else if (prop.Name == nameof(ShowSimpleStatsResponse.SimpleMidWarsSeasonStats))
                {
                      SimpleSeasonStats val = props.SimpleMidWarsSeasonStats ?? new() 
                      { 
                        RankedMatchesWon = 0, RankedMatchesLost = 0, WinStreak = 0, 
                        CurrentRank = "0.000", RankedRating = "1500.000", InPlacementPhase = 0 
                      };
                      hybridData[attr.PropertyKey.Value] = ToHashtable(val, isCasual: false);
                }
                else if (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(OneOf<,>))
                {
                    object? oneOfValue = prop.GetValue(props);
                    
                    if (oneOfValue != null)
                    {
                        // Logan (2025-02-14): RECURSIVE HANDLE ONEOF VALUE
                        // If the inner value of OneOf is a complex type (List, Dictionary), we must process it too.
                        dynamic? innerValue = ((dynamic)oneOfValue).Value;
                        
                        // Debug Logging for OneOf Properties
                        if (attr.PropertyKey.IsT1) 
                        {
                            string strKey = attr.PropertyKey.AsT1;
                            if (strKey == "nickname" || strKey == "account_id")
                            {
                                Console.WriteLine($"[CreateHybridSimpleStats] OneOf Key: {strKey}, Value: {innerValue}");
                            }
                        }

                        hybridData[attr.PropertyKey.Value] = ConvertToPhp(innerValue);
                    }
                    else
                    {
                        hybridData[attr.PropertyKey.Value] = "0";
                    }
                }
                else
                {
                    // Logan (2025-02-14): General Serialization Logic (Recursive)
                    object? val = prop.GetValue(props);
                    
                    // Debug Logging for Profile Top
                    if (attr.PropertyKey.IsT1) // String key (T1 is string, T0 is int)
                    {
                        string strKey = attr.PropertyKey.AsT1;
                        if (strKey == "nickname" || strKey == "account_id" || strKey == "total_played")
                        {
                            Console.WriteLine($"[CreateHybridSimpleStats] Key: {strKey}, Value: {val}");
                        }
                    }
                    
                    hybridData[attr.PropertyKey.Value] = ConvertToPhp(val);
                }
            }
        }
        Console.WriteLine($"[CreateHybridSimpleStats] Generated {hybridData.Count} keys.");



        // Logan (2025-02-14): Map 'acc_' keys (Account Total)
        // Sum Ranked + Casual + Public stats from the DTO.
        PlayerStatisticsAggregatedDTO? simpleStats = props.SimpleSeasonStats?.GenericStats;
        if (simpleStats != null)
        {
            string Sum(int r, int c, int p) => (r + c + p).ToString();
            string SumL(long r, long c, long p) => (r + c + p).ToString();

            hybridData["acc_denies"] = Sum(simpleStats.RankedDenies, simpleStats.CasualDenies, simpleStats.PublicDenies);
            hybridData["acc_herodmg"] = Sum(simpleStats.RankedHeroDamage, simpleStats.CasualHeroDamage, simpleStats.PublicHeroDamage);
            hybridData["acc_herokillsgold"] = Sum(simpleStats.RankedHeroGold, simpleStats.CasualHeroGold, simpleStats.PublicHeroGold);
            hybridData["acc_heroassists"] = Sum(simpleStats.RankedAssists, simpleStats.CasualAssists, simpleStats.PublicAssists);
            hybridData["acc_deaths"] = Sum(simpleStats.RankedDeaths, simpleStats.CasualDeaths, simpleStats.PublicDeaths);
            hybridData["acc_herokills"] = Sum(simpleStats.RankedKills, simpleStats.CasualKills, simpleStats.PublicKills);
            hybridData["acc_goldlost2death"] = Sum(simpleStats.RankedGoldLost, simpleStats.CasualGoldLost, simpleStats.PublicGoldLost);
            hybridData["acc_secs_dead"] = Sum(simpleStats.RankedSecondsDead, simpleStats.CasualSecondsDead, simpleStats.PublicSecondsDead);
            hybridData["acc_teamcreepkills"] = Sum(simpleStats.RankedTeamCreepKills, simpleStats.CasualTeamCreepKills, simpleStats.PublicTeamCreepKills);
            hybridData["acc_teamcreepdmg"] = Sum(simpleStats.RankedTeamCreepDmg, simpleStats.CasualTeamCreepDmg, simpleStats.PublicTeamCreepDmg);
            hybridData["acc_teamcreepgold"] = Sum(simpleStats.RankedTeamCreepGold, simpleStats.CasualTeamCreepGold, simpleStats.PublicTeamCreepGold);
            hybridData["acc_teamcreepexp"] = Sum(simpleStats.RankedTeamCreepExp, simpleStats.CasualTeamCreepExp, simpleStats.PublicTeamCreepExp);
            hybridData["acc_neutralcreepkills"] = Sum(simpleStats.RankedNeutralCreepKills, simpleStats.CasualNeutralCreepKills, simpleStats.PublicNeutralCreepKills);
            hybridData["acc_neutralcreepdmg"] = Sum(simpleStats.RankedNeutralCreepDmg, simpleStats.CasualNeutralCreepDmg, simpleStats.PublicNeutralCreepDmg);
            hybridData["acc_neutralcreepgold"] = Sum(simpleStats.RankedNeutralCreepGold, simpleStats.CasualNeutralCreepGold, simpleStats.PublicNeutralCreepGold);
            hybridData["acc_neutralcreepexp"] = Sum(simpleStats.RankedNeutralCreepExp, simpleStats.CasualNeutralCreepExp, simpleStats.PublicNeutralCreepExp);
            hybridData["acc_bdmg"] = Sum(simpleStats.RankedBuildingDmg, simpleStats.CasualBuildingDmg, simpleStats.PublicBuildingDmg);
            hybridData["acc_razed"] = Sum(simpleStats.RankedBuildingsRazed, simpleStats.CasualBuildingsRazed, simpleStats.PublicBuildingsRazed);
            hybridData["acc_bdmgexp"] = Sum(simpleStats.RankedBuildingExp, simpleStats.CasualBuildingExp, simpleStats.PublicBuildingExp);
            hybridData["acc_bgold"] = Sum(simpleStats.RankedBuildingGold, simpleStats.CasualBuildingGold, simpleStats.PublicBuildingGold);
            hybridData["acc_exp_denied"] = Sum(simpleStats.RankedExpDenied, simpleStats.CasualExpDenied, simpleStats.PublicExpDenied);
            hybridData["acc_gold"] = SumL(simpleStats.RankedGold, simpleStats.CasualGold, simpleStats.PublicGold);
            hybridData["acc_gold_spend"] = Sum(simpleStats.RankedGoldSpent, simpleStats.CasualGoldSpent, simpleStats.PublicGoldSpent);
            hybridData["acc_actions"] = Sum(simpleStats.RankedActions, simpleStats.CasualActions, simpleStats.PublicActions);
            hybridData["acc_consumables"] = Sum(simpleStats.RankedConsumables, simpleStats.CasualConsumables, simpleStats.PublicConsumables);
            hybridData["acc_wards"] = Sum(simpleStats.RankedWards, simpleStats.CasualWards, simpleStats.PublicWards);
            hybridData["acc_bloodlust"] = Sum(simpleStats.RankedFirstBloods, simpleStats.CasualFirstBloods, simpleStats.PublicFirstBloods);
            hybridData["acc_doublekill"] = Sum(simpleStats.RankedDoubleKills, simpleStats.CasualDoubleKills, simpleStats.PublicDoubleKills);
            hybridData["acc_triplekill"] = Sum(simpleStats.RankedTripleKills, simpleStats.CasualTripleKills, simpleStats.PublicTripleKills);
            hybridData["acc_quadkill"] = Sum(simpleStats.RankedQuadKills, simpleStats.CasualQuadKills, simpleStats.PublicQuadKills);
            hybridData["acc_annihilation"] = Sum(simpleStats.RankedAnnihilations, simpleStats.CasualAnnihilations, simpleStats.PublicAnnihilations);
            hybridData["acc_ks3"] = Sum(simpleStats.RankedKS3, simpleStats.CasualKS3, simpleStats.PublicKS3);
            hybridData["acc_ks4"] = Sum(simpleStats.RankedKS4, simpleStats.CasualKS4, simpleStats.PublicKS4);
            hybridData["acc_ks5"] = Sum(simpleStats.RankedKS5, simpleStats.CasualKS5, simpleStats.PublicKS5);
            hybridData["acc_ks6"] = Sum(simpleStats.RankedKS6, simpleStats.CasualKS6, simpleStats.PublicKS6);
            hybridData["acc_ks7"] = Sum(simpleStats.RankedKS7, simpleStats.CasualKS7, simpleStats.PublicKS7);
            hybridData["acc_ks8"] = Sum(simpleStats.RankedKS8, simpleStats.CasualKS8, simpleStats.PublicKS8);
            hybridData["acc_ks9"] = Sum(simpleStats.RankedKS9, simpleStats.CasualKS9, simpleStats.PublicKS9);
            hybridData["acc_ks10"] = Sum(simpleStats.RankedKS10, simpleStats.CasualKS10, simpleStats.PublicKS10);
            hybridData["acc_ks15"] = Sum(simpleStats.RankedKS15, simpleStats.CasualKS15, simpleStats.PublicKS15);
            hybridData["acc_smackdown"] = Sum(simpleStats.RankedSmackdowns, simpleStats.CasualSmackdowns, simpleStats.PublicSmackdowns);
            hybridData["acc_humiliation"] = Sum(simpleStats.RankedHumiliations, simpleStats.CasualHumiliations, simpleStats.PublicHumiliations);
            hybridData["acc_nemesis"] = Sum(simpleStats.RankedNemesis, simpleStats.CasualNemesis, simpleStats.PublicNemesis);
            hybridData["acc_retribution"] = Sum(simpleStats.RankedRetribution, simpleStats.CasualRetribution, simpleStats.PublicRetribution);
            hybridData["acc_time_earning_exp"] = Sum(simpleStats.RankedTimeEarningExp, simpleStats.CasualTimeEarningExp, simpleStats.PublicTimeEarningExp);
            hybridData["acc_buybacks"] = Sum(simpleStats.RankedBuybacks, simpleStats.CasualBuybacks, simpleStats.PublicBuybacks);
            
            hybridData["acc_exp"] = SumL(simpleStats.RankedExp, simpleStats.CasualExp, simpleStats.PublicExp);
            hybridData["acc_secs"] = SumL(simpleStats.RankedSeconds, simpleStats.CasualSeconds, simpleStats.PublicSeconds);
        }

        // 2. Add Legacy Indexed Keys (0..124+)
        // FIX: Manually inject named keys that are likely mapped in Client XML but missing in DTO.
        // If we don't send these, CUIForm will find the mapping, fail to find the key, and insert "" (TSNULL) into the index,
        // overwriting any integer-keyed value we set below and causing Lua crashes (tonumber("") -> nil).
        hybridData["possible_discos"] = "0";    // Mapped to 117
        hybridData["account_type"] = "0";       // Mapped to 119
        hybridData["standing"] = "0";           // Mapped to 120
        hybridData["level_percent"] = "0";      // Mapped to 121
        hybridData["max_exp"] = "0";            // Mapped to 122
        hybridData["min_exp"] = "0";            // Mapped to 123
        hybridData["mid_games_played"] = "0";   // Mapped to 124
        hybridData["mid_discos"] = "0";         // Mapped to 125
        hybridData["event_id"] = "0";           // Mapped to 128
        hybridData["events"] = "0";             // Mapped to 129
        hybridData["season_id"] = props.CurrentSeason.ToString(); // Ensure this is explicit string (though DTO handles it)

        void Set(int luaIndex, object? value)
        {
            int phpIndex = luaIndex - 1; // Map 1-based Lua index back to 0-based legacy PHP index expectations

            if (value == null)
            {
                hybridData[phpIndex] = "0";
                return;
            }

            if (value is bool b)
            {
                hybridData[phpIndex] = b ? "1" : "0";
                return;
            }

            if (value is int || value is long)
            {
                hybridData[phpIndex] = value;
                return;
            }

            // Primitive types that legacy expected as strings
            if (value is string || value is double || value is float || value is decimal)
            {
                hybridData[phpIndex] = value.ToString()!;
                return;
            }

            // Complex types (Lists, Dictionaries) needed for modern/hybrid structure
            // Enforce PHP associative rules (integer conversion for numeric keys)
            hybridData[phpIndex] = ConvertToPhp(value);
        }

        // Logan (2025-02-16): Determine stats context based on 'table' parameter
        // Keep reference to Ranked Stats for legacy 'rnk_' key population later
        SimpleSeasonStats stats = props.SimpleSeasonStats ?? new SimpleSeasonStats 
        { 
            RankedMatchesWon = 0, RankedMatchesLost = 0, WinStreak = 0,
            CurrentRank = "0.000", RankedRating = "1500.000", InPlacementPhase = 0
        };

        SimpleSeasonStats activeStats = stats;

        // Override for Casual
        if (table == "casual" || table == "campaign_casual")
        {
            activeStats = props.SimpleCasualSeasonStats ?? new SimpleSeasonStats
            {
                RankedMatchesWon = 0, RankedMatchesLost = 0, WinStreak = 0,
                CurrentRank = "0.000", RankedRating = "1500.000", InPlacementPhase = 0
            };
        }
        else if (table == "player") // Public
        {
            // Synthesize SimpleSeasonStats from Public DTO data for mapping purposes
            PlayerStatisticsAggregatedDTO? pub = props.SimpleSeasonStats?.GenericStats;
            activeStats = new SimpleSeasonStats
            {
                RankedMatchesWon = (int)(pub?.PublicWins ?? 0),
                RankedMatchesLost = (int)(pub?.PublicLosses ?? 0),
                RankedRating = (1500.0 + (pub?.PublicRatingChange ?? 0)).ToString(CultureInfo.InvariantCulture),
                CurrentRank = "0", // No ranks in public
                HighestRank = "0",
                WinStreak = 0,
                InPlacementPhase = 0
            };
        }

        // Logan (2025-02-20): Fulfill Modern Root Payload Contract
        // main.lua expects 'nickname' for the top right corner.
        // If 'nickname' is present, player_stats_v2.lua looks for stats at the root using string keys!
        int matchesPlayed = activeStats.RankedMatchesWon + activeStats.RankedMatchesLost;
        hybridData["wins"] = activeStats.RankedMatchesWon.ToString();
        hybridData["rnk_wins"] = activeStats.RankedMatchesWon.ToString();
        hybridData["losses"] = activeStats.RankedMatchesLost.ToString();
        hybridData["rnk_losses"] = activeStats.RankedMatchesLost.ToString();
        hybridData["matches"] = matchesPlayed.ToString();
        hybridData["total_games_played"] = matchesPlayed.ToString();
        hybridData["rnk_games_played"] = matchesPlayed.ToString();
        hybridData["level"] = props.SeasonLevel.ToString();
        hybridData["con_reward"] = props.ConReward ?? "";
        hybridData["rank"] = props.Rank ?? "0";
        hybridData["highest_level_current"] = props.HighestRankTop ?? "0";
        
        hybridData["favHero1"] = props.FavHero1 ?? "0";
        hybridData["favHero2"] = props.FavHero2 ?? "0";
        hybridData["favHero3"] = props.FavHero3 ?? "0";
        hybridData["favHero4"] = props.FavHero4 ?? "0";
        hybridData["favHero5"] = props.FavHero5 ?? "0";

        hybridData["favHero1_2"] = props.FavHero1_2 ?? "0";
        hybridData["favHero2_2"] = props.FavHero2_2 ?? "0";
        hybridData["favHero3_2"] = props.FavHero3_2 ?? "0";
        hybridData["favHero4_2"] = props.FavHero4_2 ?? "0";
        hybridData["favHero5_2"] = props.FavHero5_2 ?? "0";

        hybridData["favHero1Time"] = props.FavHero1Time.ToString();
        hybridData["favHero2Time"] = props.FavHero2Time.ToString();
        hybridData["favHero3Time"] = props.FavHero3Time.ToString();
        hybridData["favHero4Time"] = props.FavHero4Time.ToString();
        hybridData["favHero5Time"] = props.FavHero5Time.ToString();
        
        PlayerStatisticsAggregatedDTO generic = props.SimpleSeasonStats?.GenericStats ?? new PlayerStatisticsAggregatedDTO();

        // Mapping (Using activeStats for mode-specific fields)
        Set(1, props.NameWithClanTag);          // arg[1]
        Set(2, props.NameWithClanTag);          // arg[2]
        Set(3, props.Rank);                     // arg[3]
        Set(4, props.SeasonLevel);              // arg[4]
        Set(5, props.ID.Value);                 // arg[5]
        Set(6, activeStats.RankedMatchesWon + activeStats.RankedMatchesLost); // arg[6] Games Played (Context Aware)
        Set(7, activeStats.RankedMatchesWon);   // arg[7] Wins (Context Aware)
        Set(8, activeStats.RankedMatchesLost);  // arg[8] Losses (Context Aware)
        Set(9, 0); // cam_concedes
        Set(10, 0); // cam_concedevotes
        
        // Fix: Map Mastery Info and Rewards to positions expected by OnPlayerStatsMasteryResult
        // Lua: local masteryInfo = arg[11]
        // Lua: local rewardsInfo = arg[12]
        Set(11, props.MasteryInfo); // arg[11]
        Set(12, props.MasteryRewards); // arg[12]
        
        Set(13, 0); // cam_kicked
        Set(14, activeStats.RankedRating);      // arg[14] MMR / Rating (Context Aware)
        Set(15, 0); // cam_pub_count
        Set(16, 0); // cam_amm_solo_rating
        Set(17, 0); // cam_amm_solo_count
        Set(18, activeStats.RankedRating);      // arg[18] Team MMR (Context Aware - typically duplicates 14 for Normal)
        Set(19, 0); // cam_amm_team_count
        Set(20, 0); // rnk_avg_score
        
        // Mappings restored
        Set(21, generic.Kills);                 // cam_herokills (Was RankedKills)
        Set(22, 0); // cam_herodmg (Not tracked in DTO yet)
        Set(23, generic.RankedExp + generic.CasualExp); // cam_heroexp (Total Exp)
        Set(24, 0); // cam_herokillsgold
        Set(25, generic.Assists);               // cam_heroassists (Was RankedAssists)
        Set(26, generic.Deaths);                // cam_deaths (Was RankedDeaths)
        Set(27, 0); // cam_goldlost2death
        Set(28, 0); // cam_secs_dead
        Set(29, 0); // cam_teamcreepkills
        Set(30, 0); // cam_teamcreepdmg
        Set(31, 0); // cam_teamcreepexp
        Set(32, 0); // cam_teamcreepgold
        Set(33, 0); // cam_neutralcreepkills
        Set(34, 0); // cam_neutralcreepdmg
        Set(35, 0); // cam_neutralcreepexp
        Set(36, 0); // cam_neutralcreepgold
        Set(37, 0); // cam_bdmg
        Set(38, 0); // cam_bdmgexp
        Set(39, 0); // cam_razed
        Set(40, 0); // cam_bgold
        Set(41, 0); // cam_denies
        Set(42, 0); // cam_exp_denied
        Set(43, generic.RankedGold + generic.CasualGold); // cam_gold (Total Gold)
        Set(44, 0); // cam_gold_spend
        Set(45, generic.RankedExp + generic.CasualExp);   // cam_exp
        Set(46, 0); // cam_actions
        Set(47, generic.RankedSeconds + generic.CasualSeconds); // cam_secs
        Set(48, 0); // cam_consumables
        Set(49, 0); // cam_wards
        Set(50, 0); // cam_em_played
        Set(51, 0); // maxXP

        Set(52, props.LastActivityDate);        // arg[52]
        Set(53, 1053);
        Set(54, 1054);
        
        // Context-Aware Population for Stats Tab (Indices 68-75 + Named Keys)
        // Determine which stats to show based on requested table.
        // Defaults to Ranked (Season) if table is missing/campaign.
        // table: "campaign" (Ranked), "casual" (Casual), "player" (Public)

        long matches = 0, kills = 0, deaths = 0, assists = 0, secs = 0, exp = 0;
        int denies = 0, actions = 0, wards = 0, creeps = 0, neutrals = 0;
        long firstBloods = 0, doubleKills = 0, tripleKills = 0, quadKills = 0, annihilations = 0;
        long ks3 = 0, ks4 = 0, ks5 = 0, ks6 = 0, ks7 = 0, ks8 = 0, ks9 = 0, ks10 = 0, ks15 = 0;
        long smackdowns = 0, humiliations = 0, nemesis = 0, retribution = 0;

        if (simpleStats != null)
        {
            if (table == "casual" || table == "campaign_casual")
            {
                matches = simpleStats.CasualMatches;
                kills = simpleStats.CasualKills;
                deaths = simpleStats.CasualDeaths;
                assists = simpleStats.CasualAssists;
                secs = simpleStats.CasualSeconds;
                exp = simpleStats.CasualExp;
                denies = simpleStats.CasualDenies;
                actions = simpleStats.CasualActions;
                wards = simpleStats.CasualWards;
                creeps = simpleStats.CasualTeamCreepKills;
                neutrals = simpleStats.CasualNeutralCreepKills;
                firstBloods = simpleStats.CasualFirstBloods;
                doubleKills = simpleStats.CasualDoubleKills;
                tripleKills = simpleStats.CasualTripleKills;
                quadKills = simpleStats.CasualQuadKills;
                annihilations = simpleStats.CasualAnnihilations;
                ks3 = simpleStats.CasualKS3;
                ks4 = simpleStats.CasualKS4;
                ks5 = simpleStats.CasualKS5;
                ks6 = simpleStats.CasualKS6;
                ks7 = simpleStats.CasualKS7;
                ks8 = simpleStats.CasualKS8;
                ks9 = simpleStats.CasualKS9;
                ks10 = simpleStats.CasualKS10;
                ks15 = simpleStats.CasualKS15;
                smackdowns = simpleStats.CasualSmackdowns;
                humiliations = simpleStats.CasualHumiliations;
                nemesis = simpleStats.CasualNemesis;
                retribution = simpleStats.CasualRetribution;
            }
            else if (table == "player") // Public
            {
                matches = simpleStats.PublicMatches;
                kills = simpleStats.PublicKills;
                deaths = simpleStats.PublicDeaths;
                assists = simpleStats.PublicAssists;
                secs = simpleStats.PublicSeconds;
                exp = simpleStats.PublicExp;
                denies = simpleStats.PublicDenies;
                actions = simpleStats.PublicActions;
                wards = simpleStats.PublicWards;
                creeps = simpleStats.PublicTeamCreepKills;
                neutrals = simpleStats.PublicNeutralCreepKills;
                firstBloods = simpleStats.PublicFirstBloods;
                doubleKills = simpleStats.PublicDoubleKills;
                tripleKills = simpleStats.PublicTripleKills;
                quadKills = simpleStats.PublicQuadKills;
                annihilations = simpleStats.PublicAnnihilations;
                ks3 = simpleStats.PublicKS3;
                ks4 = simpleStats.PublicKS4;
                ks5 = simpleStats.PublicKS5;
                ks6 = simpleStats.PublicKS6;
                ks7 = simpleStats.PublicKS7;
                ks8 = simpleStats.PublicKS8;
                ks9 = simpleStats.PublicKS9;
                ks10 = simpleStats.PublicKS10;
                ks15 = simpleStats.PublicKS15;
                smackdowns = simpleStats.PublicSmackdowns;
                humiliations = simpleStats.PublicHumiliations;
                nemesis = simpleStats.PublicNemesis;
                retribution = simpleStats.PublicRetribution;
            }
            else // Ranked/Campaign (Default)
            {
                matches = simpleStats.RankedMatches;
                kills = simpleStats.RankedKills;
                deaths = simpleStats.RankedDeaths;
                assists = simpleStats.RankedAssists;
                secs = simpleStats.RankedSeconds;
                exp = simpleStats.RankedExp;
                denies = simpleStats.RankedDenies;
                actions = simpleStats.RankedActions;
                wards = simpleStats.RankedWards;
                creeps = simpleStats.RankedTeamCreepKills;
                neutrals = simpleStats.RankedNeutralCreepKills;
                firstBloods = simpleStats.RankedFirstBloods;
                doubleKills = simpleStats.RankedDoubleKills;
                tripleKills = simpleStats.RankedTripleKills;
                quadKills = simpleStats.RankedQuadKills;
                annihilations = simpleStats.RankedAnnihilations;
                ks3 = simpleStats.RankedKS3;
                ks4 = simpleStats.RankedKS4;
                ks5 = simpleStats.RankedKS5;
                ks6 = simpleStats.RankedKS6;
                ks7 = simpleStats.RankedKS7;
                ks8 = simpleStats.RankedKS8;
                ks9 = simpleStats.RankedKS9;
                ks10 = simpleStats.RankedKS10;
                ks15 = simpleStats.RankedKS15;
                smackdowns = simpleStats.RankedSmackdowns;
                humiliations = simpleStats.RankedHumiliations;
                nemesis = simpleStats.RankedNemesis;
                retribution = simpleStats.RankedRetribution;
            }
        }

        string kda = "0.00:1";
        if (deaths > 0)
            kda = $"{(double)(kills + assists) / deaths:0.00}:1";
        else if (kills + assists > 0)
            kda = $"{kills + assists}:0";
        Set(13, 1013); // arg[13]
        Set(14, "1500.000"); // arg[14]

        for (int i = 15; i <= 17; i++) 
        {
            Set(i, 1000 + i); 
        }

        Set(18, firstBloods.ToString());
        Set(19, doubleKills.ToString());
        Set(20, tripleKills.ToString());
        Set(21, quadKills.ToString());
        Set(22, annihilations.ToString());
        Set(23, ks3.ToString());
        Set(24, ks4.ToString());
        Set(25, ks5.ToString());
        Set(26, ks6.ToString());
        Set(27, ks7.ToString());
        Set(28, ks8.ToString());
        Set(29, ks9.ToString());
        Set(30, ks10.ToString());
        Set(31, ks15.ToString());
        Set(32, smackdowns.ToString());
        Set(33, humiliations.ToString());
        Set(34, nemesis.ToString());
        Set(35, retribution.ToString());

        Set(36, 0); // cam_teamkillexp
        Set(37, 0); // cam_teamkillgold
        Set(38, actions.ToString()); // cam_actions

        // Duplicate unsafe null assignments removed (already safely assigned at Line 929)
        
        for (int i = 54; i <= 62; i++) 
        {
            Set(i, 1000 + i); 
        }
        // Legacy avg stats are safely omitted from integer packing to prevent FavHero collisions (55-64).
        // UI extracts these via associative string keys at the tail of the array if required.

        double avg(long total, long divisor) => divisor > 0 ? (double)total / divisor : 0.0;
        
        hybridData["avgCreepKills"] = avg(creeps, matches).ToString("0.0", CultureInfo.InvariantCulture);
        hybridData["avgDenies"] = avg(denies, matches).ToString("0.0", CultureInfo.InvariantCulture);
        
        long minutes = secs / 60;
        hybridData["avgXP_min"] = avg(exp, minutes).ToString("0.0", CultureInfo.InvariantCulture);
        
        long gold = 0;
        if (simpleStats != null)
        {
            if (table == "casual" || table == "campaign_casual") gold = simpleStats.CasualGold;
            else if (table == "player") gold = simpleStats.PublicGold;
            else gold = simpleStats.RankedGold;
        }
        
        hybridData["avgGold_min"] = avg(gold, minutes).ToString("0.0", CultureInfo.InvariantCulture);
        hybridData["avgActions_min"] = avg(actions, minutes).ToString("0.0", CultureInfo.InvariantCulture);
        hybridData["avgWardsUsed"] = avg(wards, matches).ToString("0.0", CultureInfo.InvariantCulture);
        hybridData["k_d_a"] = kda;
        
        // Game log expects HH:MM length, e.g. 35:00
        hybridData["avgGameLength"] = matches > 0 ? TimeSpan.FromSeconds((double)secs / matches).ToString(@"mm\:ss") : "00:00";
        hybridData["avgNeutralKills"] = avg(neutrals, matches).ToString("0.0", CultureInfo.InvariantCulture);

        // Map Raw Lifetime Stats for Statistics Tab
        hybridData["herokills"] = kills.ToString();
        hybridData["heroassists"] = assists.ToString();
        hybridData["deaths"] = deaths.ToString();
        hybridData["gold"] = gold.ToString();
        hybridData["exp"] = exp.ToString();
        hybridData["secs"] = secs.ToString();

        hybridData["ks3"] = ks3.ToString();
        hybridData["ks4"] = ks4.ToString();
        hybridData["ks5"] = ks5.ToString();
        hybridData["ks6"] = ks6.ToString();
        hybridData["ks7"] = ks7.ToString();
        hybridData["ks8"] = ks8.ToString();
        hybridData["ks9"] = ks9.ToString();
        hybridData["ks10"] = ks10.ToString();
        hybridData["ks15"] = ks15.ToString();
        hybridData["doublekill"] = doubleKills.ToString();
        hybridData["triplekill"] = tripleKills.ToString();
        hybridData["quadkill"] = quadKills.ToString();
        hybridData["annihilation"] = annihilations.ToString();
        hybridData["bloodlust"] = firstBloods.ToString();
        
        hybridData["smackdown"] = smackdowns.ToString();
        hybridData["humiliation"] = humiliations.ToString();
        hybridData["nemesis"] = nemesis.ToString();
        hybridData["retribution"] = retribution.ToString();

            // Calculate XP and Percentile BEFORE setting indices 65-66
            long currentLevel = props.Level;
            long currentLevelMinXp = (long)(100 * Math.Pow(currentLevel, 3));
            long nextLevelMinXp = (long)(100 * Math.Pow(currentLevel + 1, 3));
            
            double percent = 0.0;
            if (props.LevelExperience >= currentLevelMinXp && nextLevelMinXp > currentLevelMinXp)
            {
                 long xpIntoLevel = props.LevelExperience - currentLevelMinXp;
                 long xpNeeded = nextLevelMinXp - currentLevelMinXp;
                 percent = (double)xpIntoLevel / (double)xpNeeded;
                 if (percent > 1.0) percent = 1.0;
                 if (percent < 0.0) percent = 0.0;
            }
        string pctStr = (percent * 100.0).ToString("0.00", CultureInfo.InvariantCulture);

        Set(63, props.LevelExperience);         // arg[63] level_exp
        Set(64, pctStr);                        // arg[64] level_percent
        Set(65, nextLevelMinXp.ToString());     // arg[65] max_exp
        Set(66, currentLevelMinXp.ToString());  // arg[66] min_exp
        
        Set(67, 1067);                             // arg[67] mid_games_played
        Set(68, 1068);                             // arg[68] mid_discos
        Set(69, generic.TotalMatches);          // arg[69] total_games_played
        Set(70, generic.Disconnected);          // arg[70] total_discos
        Set(71, 1071);                             // arg[71] event_id
        Set(72, 1072);                             // arg[72] events
        Set(73, generic.CasualDiscos);          // arg[73] uncs_discos
        Set(74, 1074);                             // arg[74] unrnk_discos
        Set(75, generic.CasualMatches);         // arg[75] uncs_games_played
        Set(76, props.AccountCreationDate);     // arg[76] create_date (or unrnk_games_played)
        Set(77, 1077);                             // arg[77] rift_games_played
        Set(78, 1078);                             // arg[78] rift_discos
        Set(79, props.Level);                   // arg[79] highest_level
        Set(80, props.HighestRankTop ?? "0");   // arg[80] highest_level_current

        for (int i = 81; i <= 85; i++) Set(i, 1000 + i); // favHero1id to favHero5id


        Set(86, 1086);                             // arg[86] error
        Set(87, string.IsNullOrWhiteSpace(props.CurrentRankTop) ? "0" : props.CurrentRankTop);   // arg[87] season_level (cam_level)
        Set(88, "1088");                           // arg[88] selected_upgrades
        
        Set(89, generic.TotalMatches);          // arg[89] acc_games_played
        Set(90, stats.RankedMatchesWon + stats.RankedMatchesLost); // arg[90] rnk_games_played
        Set(91, generic.Disconnected);          // arg[91] acc_discos
        Set(92, 1092);                             // arg[92] rnk_discos
        Set(93, generic.CasualDiscos);          // arg[93] cs_discos
        
        for (int i = 94; i <= 101; i++) Set(i, 1000 + i);

        Set(102, stats.RankedMatchesWon);       // arg[102] rnk_wins
        Set(103, stats.RankedMatchesLost);      // arg[103] rnk_losses

        for (int i = 104; i <= 111; i++) Set(i, 1000 + i);

        
        Set(112, 1112); 
        Set(113, 1113); 
        string? rankBadgeId = string.IsNullOrWhiteSpace(props.CurrentRankTop) ? "0" : props.CurrentRankTop;
        
        if (table == "player")
        {
            Set(114, props.Level);                  // arg[114] level (Player Level)
            hybridData["level"] = props.Level.ToString(); // Explicit Lua safety
        }
        else
        {
            Set(114, rankBadgeId);                  // arg[114] level (Rank Badge ID)
            hybridData["level"] = rankBadgeId; // Explicit Lua safety for Ranked Tabs
            hybridData["current_level"] = rankBadgeId;
        }
        Set(115, props.LevelExperience);        // arg[115] level_exp
        Set(116, props.TotalDisconnects);       // arg[116] total_disconnects
        Set(117, "0"); 
        Set(118, (stats.RankedMatchesWon + stats.RankedMatchesLost).ToString()); // arg[118]
        Set(119, "0"); // account_type
        Set(120, "0"); // standing
        Set(121, pctStr); // arg[121] level_percent
        hybridData["level_percent"] = pctStr; // Explicit Lua safety for modern parser branch
        Set(122, nextLevelMinXp.ToString()); // max_exp
        Set(123, currentLevelMinXp.ToString()); // min_exp
        Set(124, "0"); // mid_games_played
        Set(125, "0"); // mid_discos
        for (int i = 127; i <= 135; i++) Set(i, (1000 + i).ToString());

        // Logan (2025-02-13): Inject Legacy Root Keys for Direct Lua Access (Normal)
        // This fixes OnPlayerStatsNormalSeasonResult which expects keys at arg[1]
        // Use local 'stats' variable which is already null-coalesced above (line 474)
        hybridData["rnk_wins"] = stats.RankedMatchesWon.ToString();
        hybridData["rnk_losses"] = stats.RankedMatchesLost.ToString();
        hybridData["rnk_games_played"] = (stats.RankedMatchesWon + stats.RankedMatchesLost).ToString();
        hybridData["acc_wins"] = stats.RankedMatchesWon.ToString();
        hybridData["acc_games_played"] = (stats.RankedMatchesWon + stats.RankedMatchesLost).ToString();

        // Logan (2025-02-13): Inject standard 'wins'/'losses' for Public Stats (Overview)
        // Lua OnPlayerStatsPublicResult looks for 'wins'/'losses', but Model sends 'cam_wins'/'cam_losses'
        if (table == "player")
        {
            hybridData["wins"] = simpleStats?.PublicWins.ToString() ?? "0";
            hybridData["losses"] = simpleStats?.PublicLosses.ToString() ?? "0";
            hybridData["matches"] = simpleStats?.PublicMatches.ToString() ?? "0";
            hybridData["total_games_played"] = simpleStats?.PublicMatches.ToString() ?? "0";
            hybridData["acc_pub_skill"] = (1500.0 + (simpleStats?.PublicRatingChange ?? 0)).ToString(CultureInfo.InvariantCulture);

            // Map Index 14 for Public Tab
            Set(14, (1500.0 + (simpleStats?.PublicRatingChange ?? 0)).ToString(CultureInfo.InvariantCulture));
        }
        else
        {
            hybridData["wins"] = stats.RankedMatchesWon.ToString();
            hybridData["losses"] = stats.RankedMatchesLost.ToString();
        }

        // Logan (2025-02-13): Inject Legacy Root Keys for Direct Lua Access (Casual)
        // This fixes OnPlayerStatsCasualSeasonResult
        SimpleSeasonStats? casualStats = props.SimpleCasualSeasonStats;
        hybridData["cs_wins"] = (casualStats?.RankedMatchesWon ?? 0).ToString();
        hybridData["cs_losses"] = (casualStats?.RankedMatchesLost ?? 0).ToString();
        hybridData["cs_games_played"] = ((casualStats?.RankedMatchesWon ?? 0) + (casualStats?.RankedMatchesLost ?? 0)).ToString();

    Set(136, string.IsNullOrWhiteSpace(props.CurrentRankTop) ? "0" : props.CurrentRankTop);  // arg[136] current_level (Badge ID)
    Set(137, string.IsNullOrWhiteSpace(props.HighestRankTop) ? "0" : props.HighestRankTop);  // arg[137] highest_level_current
    Set(138, props.CurrentSeason.ToString()); // arg[138] season_id - Force String
    
    Set(139, stats.RankedRating); // arg[139] current_ranking
    Set(140, stats.RankedMatchesWon + stats.RankedMatchesLost); // arg[140] curr_season_cam_games_played
    Set(141, stats.RankedMatchesWon + stats.RankedMatchesLost); // arg[141] rnk_games_played
    Set(142, generic.Disconnected); // arg[142] curr_season_cam_discos
    Set(143, generic.Disconnected); // arg[143] rnk_discos
    Set(144, "0"); // arg[144] prev_seasons_cam_games_played
    Set(145, "0"); // arg[145] prev_seasons_cam_cs_games_played
    Set(146, "0"); // arg[146] prev_seasons_cam_discos
    Set(147, "0"); // arg[147] prev_seasons_cam_cs_discos
    Set(148, string.IsNullOrWhiteSpace(props.HighestRankTop) ? "0" : props.HighestRankTop); // arg[148] highest_ranking
    Set(149, props.ConReward ?? ""); // arg[149] con_reward
    
    return hybridData;
}

    /// <summary>
    /// Recursively converts objects to PHP-compatible types (Hashtable, ArrayList, String).
    /// </summary>
    private static object ConvertToPhp(object? value)
    {
        if (value == null) return "0"; 
        
        // 1. Primitives & Strings
        if (value is string || value is int || value is long || value is float || value is double || value is bool || value is decimal)
        {
            if (value is bool b) return b ? "1" : "0";
            return value.ToString()!;
        }

        // Handle System.Text.Json elements that might sneak in from DB deserialization
        if (value is System.Text.Json.JsonElement je)
        {
            switch (je.ValueKind)
            {
                case System.Text.Json.JsonValueKind.Object:
                    Dictionary<object, object> hash = new Dictionary<object, object>();
                    foreach (System.Text.Json.JsonProperty prop in je.EnumerateObject())
                    {
                        object k = int.TryParse(prop.Name, out int i) ? i : prop.Name;
                        hash[k] = ConvertToPhp(prop.Value);
                    }
                    return hash;
                case System.Text.Json.JsonValueKind.Array:
                    System.Collections.ArrayList arr = new System.Collections.ArrayList();
                    foreach (System.Text.Json.JsonElement el in je.EnumerateArray())
                    {
                        arr.Add(ConvertToPhp(el));
                    }
                    return arr;
                case System.Text.Json.JsonValueKind.String:
                    return je.GetString() ?? "0";
                case System.Text.Json.JsonValueKind.Number:
                    return je.GetRawText();
                case System.Text.Json.JsonValueKind.True: return "1";
                case System.Text.Json.JsonValueKind.False: return "0";
                default: return "0";
            }
        }

        // 2. Dictionaries -> Hashtable
        // IDictionary catches non-generic, but we need to check if it's a generic one too via interface.
        Type type = value.GetType();
        bool isDictionary = typeof(System.Collections.IDictionary).IsAssignableFrom(type) || 
                            (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>));

        if (isDictionary)
        {
            Dictionary<object, object> phpDict = new Dictionary<object, object>();
            // Cast as IEnumerable to iterate anything
            System.Collections.IEnumerable enumerableDict = (System.Collections.IEnumerable)value;
            foreach (object entry in enumerableDict)
            {
                // Unbox DictionaryEntry or KeyValuePair
                object? k = null;
                object? v = null;
                
                if (entry is System.Collections.DictionaryEntry de)
                {
                    k = de.Key; v = de.Value;
                }
                else
                {
                    Type entryType = entry.GetType();
                    k = entryType.GetProperty("Key")?.GetValue(entry, null);
                    v = entryType.GetProperty("Value")?.GetValue(entry, null);
                }

                if (k != null)
                {
                    // PHP vectors require integer keys, not string "0".
                    object parsedKey = k;
                    if (k is string strKey && int.TryParse(strKey, out int intK)) 
                    {
                        parsedKey = intK;
                    }
                    else if (k is string sKey)
                    {
                        parsedKey = sKey;
                    }

                    phpDict[parsedKey] = ConvertToPhp(v);
                }
            }
            return phpDict;
        }

        // 3. Lists/Arrays -> ArrayList (which maps to PHP indexed array)
        if (value is System.Collections.IEnumerable list && !(value is string))
        {
            System.Collections.ArrayList phpList = new System.Collections.ArrayList();
            foreach (object? item in list)
            {
                phpList.Add(ConvertToPhp(item));
            }
            return phpList;
        }

        return value.ToString()!;
    }
}
