using System.Collections.Generic;

namespace ASPIRE.Tests.Data;

/// <summary>
/// Consolidated payload examples for the Client Requester.
/// </summary>
public static class ClientRequestPayloads
{
    // ==================================================================================
    // VERIFIED PAYLOADS
    // These payloads have been confirmed against legacy client captures or working logic.
    // ==================================================================================

    /// <summary>
    /// [VERIFIED] Golden reference. Successfully loads player profile.
    /// </summary>
    public static Dictionary<string, string> GetInitStats(string cookie)
    {
        return new Dictionary<string, string>
        {
            { "f", "get_initStats" },
            { "cookie", cookie }
        };
    }

    /// <summary>
    /// [VERIFIED] Confirmed working for match history.
    /// </summary>
    public static Dictionary<string, string> GetMatchStats(string cookie, int matchId)
    {
        return new Dictionary<string, string>
        {
            { "f", "get_match_stats" },
            { "cookie", cookie },
            { "match_id", matchId.ToString() }
        };
    }

    /// <summary>
    /// [VERIFIED] Standard store product retrieval.
    /// </summary>
    public static Dictionary<string, string> GetProducts(string cookie)
    {
        return new Dictionary<string, string>
        {
            { "f", "get_products" },
            { "cookie", cookie }
        };
    }

    /// <summary>
    /// [VERIFIED] Season reward claiming.
    /// </summary>
    public static Dictionary<string, string> ClaimSeasonRewards(string cookie)
    {
        return new Dictionary<string, string>
        {
            { "f", "claim_season_rewards" },
            { "cookie", cookie }
        };
    }

    /// <summary>
    /// [VERIFIED] Standard messages retrieval.
    /// </summary>
    public static Dictionary<string, string> GetSpecialMessages(string cookie)
    {
        return new Dictionary<string, string>
        {
            { "f", "get_special_messages" },
            { "cookie", cookie }
        };
    }

    /// <summary>
    /// [VERIFIED] Client event info retrieval.
    /// </summary>
    public static Dictionary<string, string> ClientEventsInfo(string cookie)
    {
        return new Dictionary<string, string>
        {
            { "f", "client_events_info" },
            { "cookie", cookie }
        };
    }

    /// <summary>
    /// [VERIFIED] Standard server list retrieval.
    /// </summary>
    public static Dictionary<string, string> ServerList(string cookie)
    {
        return new Dictionary<string, string>
        {
            { "f", "server_list" },
            { "cookie", cookie },
            { "gametype", "10" } // 10 = Standard/Normal
        };
    }

    /// <summary>
    /// [VERIFIED] Standard logout.
    /// </summary>
    public static Dictionary<string, string> Logout(string cookie)
    {
        return new Dictionary<string, string>
        {
            { "f", "logout" },
            { "cookie", cookie }
        };
    }

    /// <summary>
    /// [VERIFIED] Helper for creating custom games.
    /// </summary>
    public static Dictionary<string, string> CreateGame(string cookie, string name = "Test Game", string map = "caldavar", string mode = "ap")
    {
        return new Dictionary<string, string>
        {
            { "f", "create_game" },
            { "cookie", cookie },
            { "name", name },
            { "map", map },
            { "mode", mode },
            { "team_size", "5" },
            { "private", "0" }
        };
    }

    /// <summary>
    /// [VERIFIED] Check for new games.
    /// </summary>
    public static Dictionary<string, string> NewGameAvailable(string cookie)
    {
        return new Dictionary<string, string>
        {
            { "f", "new_game_available" },
            { "cookie", cookie }
        };
    }

    // ==================================================================================
    // UNVERIFIED / UNDER INVESTIGATION
    // These payloads are either theoretical or currently causing issues/crashes.
    // ==================================================================================

    /// <summary>
    /// [UNVERIFIED] [CRASH SUSPECT]
    /// This request is crashing the client immediately after match finish.
    /// Issue: Type mismatch in `selected_upgrades` (List vs Dictionary).
    /// </summary>
    public static Dictionary<string, string> GetUpgrades(string cookie)
    {
        return new Dictionary<string, string>
        {
            { "f", "get_upgrades" },
            { "cookie", cookie }
        };
    }

    /// <summary>
    /// [UNVERIFIED] [REGRESSION RISK]
    /// Used for scoreboards/hovers. Previously caused login regressions when return type changed.
    /// Often seen without 'table' parameter in wild.
    /// </summary>
    public static Dictionary<string, string> ShowSimpleStats(string cookie, string nickname)
    {
        return new Dictionary<string, string>
        {
            { "f", "show_simple_stats" },
            { "cookie", cookie },
            { "nickname", nickname },
            { "table", "mastery" } // Optional?
        };
    }

    /// <summary>
    /// [UNVERIFIED] [CRASH TRIGGER]
    /// The payload itself seems correct (legacy style), but the RESPONSE or side-effects
    /// (session handling) are triggering the logout crash loop.
    /// </summary>
    public static Dictionary<string, string> SubmitStats(string session, int serverId, int matchId = 705750)
    {
        return new Dictionary<string, string>
        {
            ["f"] = "submit_stats",
            ["session"] = session,
            ["match_stats[server_id]"] = serverId.ToString(),
            ["match_stats[match_id]"] = matchId.ToString(),
            ["match_stats[map]"] = "caldavar",
            ["match_stats[map_version]"] = "4.10.1",
            ["match_stats[time_played]"] = "1800",
            ["match_stats[file_size]"] = "102400",
            ["match_stats[file_name]"] = $"M{matchId}.honreplay",
            ["match_stats[c_state]"] = "0",
            ["match_stats[version]"] = "4.10.1.0",
            ["match_stats[avgpsr]"] = "1500",
            ["match_stats[avgpsr_team1]"] = "1500",
            ["match_stats[avgpsr_team2]"] = "1500",
            ["match_stats[gamemode]"] = "ap",
            ["match_stats[teamscoregoal]"] = "0",
            ["match_stats[playerscoregoal]"] = "0",
            ["match_stats[numrounds]"] = "1",
            ["match_stats[release_stage]"] = "stable",
            ["match_stats[awd_mann]"] = "0",
            ["match_stats[awd_mqk]"] = "0",
            ["match_stats[awd_lgks]"] = "5",
            ["match_stats[awd_msd]"] = "0",
            ["match_stats[awd_mkill]"] = "10",
            ["match_stats[awd_masst]"] = "5",
            ["match_stats[awd_ledth]"] = "2",
            ["match_stats[awd_mbdmg]"] = "5000",
            ["match_stats[awd_mwk]"] = "2",
            ["match_stats[awd_mhdd]"] = "15000",
            ["match_stats[awd_hcs]"] = "150",
            ["match_stats[submission_debug]"] = "debug_info",

            // Team Stats
            ["team_stats[1][score]"] = "0",
            ["team_stats[2][score]"] = "0",

            // Player Stats - Default Host Player
            ["player_stats[0][Hero_Legionnaire][nickname]"] = "TestPlayer1",
            ["player_stats[0][Hero_Legionnaire][clan_id]"] = "0",
            ["player_stats[0][Hero_Legionnaire][team]"] = "1",
            ["player_stats[0][Hero_Legionnaire][position]"] = "0",
            ["player_stats[0][Hero_Legionnaire][group_num]"] = "0",
            ["player_stats[0][Hero_Legionnaire][benefit]"] = "0",
            ["player_stats[0][Hero_Legionnaire][hero_id]"] = "12",
            ["player_stats[0][Hero_Legionnaire][wins]"] = "1",
            ["player_stats[0][Hero_Legionnaire][losses]"] = "0",
            ["player_stats[0][Hero_Legionnaire][discos]"] = "0",
            ["player_stats[0][Hero_Legionnaire][concedes]"] = "0",
            ["player_stats[0][Hero_Legionnaire][kicked]"] = "0",
            ["player_stats[0][Hero_Legionnaire][social_bonus]"] = "0",
            ["player_stats[0][Hero_Legionnaire][used_token]"] = "0",
            ["player_stats[0][Hero_Legionnaire][concedevotes]"] = "0",
            ["player_stats[0][Hero_Legionnaire][herokills]"] = "5",
            ["player_stats[0][Hero_Legionnaire][herodmg]"] = "5000",
            ["player_stats[0][Hero_Legionnaire][herokillsgold]"] = "1500",
            ["player_stats[0][Hero_Legionnaire][heroassists]"] = "2",
            ["player_stats[0][Hero_Legionnaire][heroexp]"] = "2000",
            ["player_stats[0][Hero_Legionnaire][deaths]"] = "2",
            ["player_stats[0][Hero_Legionnaire][buybacks]"] = "0",
            ["player_stats[0][Hero_Legionnaire][goldlost2death]"] = "500",
            ["player_stats[0][Hero_Legionnaire][secs_dead]"] = "60",
            ["player_stats[0][Hero_Legionnaire][teamcreepkills]"] = "0",
            ["player_stats[0][Hero_Legionnaire][teamcreepdmg]"] = "0",
            ["player_stats[0][Hero_Legionnaire][teamcreepgold]"] = "0",
            ["player_stats[0][Hero_Legionnaire][teamcreepexp]"] = "0",
            ["player_stats[0][Hero_Legionnaire][neutralcreepkills]"] = "10",
            ["player_stats[0][Hero_Legionnaire][neutralcreepdmg]"] = "500",
            ["player_stats[0][Hero_Legionnaire][neutralcreepgold]"] = "300",
            ["player_stats[0][Hero_Legionnaire][neutralcreepexp]"] = "400",
            ["player_stats[0][Hero_Legionnaire][bdmg]"] = "1000",
            ["player_stats[0][Hero_Legionnaire][razed]"] = "1",
            ["player_stats[0][Hero_Legionnaire][bdmgexp]"] = "200",
            ["player_stats[0][Hero_Legionnaire][bgold]"] = "500",
            ["player_stats[0][Hero_Legionnaire][denies]"] = "5",
            ["player_stats[0][Hero_Legionnaire][exp_denied]"] = "100",
            ["player_stats[0][Hero_Legionnaire][gold]"] = "10000",
            ["player_stats[0][Hero_Legionnaire][gold_spent]"] = "9000",
            ["player_stats[0][Hero_Legionnaire][exp]"] = "5000",
            ["player_stats[0][Hero_Legionnaire][actions]"] = "1000",
            ["player_stats[0][Hero_Legionnaire][secs]"] = "1800",
            ["player_stats[0][Hero_Legionnaire][level]"] = "15",
            ["player_stats[0][Hero_Legionnaire][consumables]"] = "5",
            ["player_stats[0][Hero_Legionnaire][wards]"] = "2",
            ["player_stats[0][Hero_Legionnaire][bloodlust]"] = "1",
            ["player_stats[0][Hero_Legionnaire][doublekill]"] = "0",
            ["player_stats[0][Hero_Legionnaire][triplekill]"] = "0",
            ["player_stats[0][Hero_Legionnaire][quadkill]"] = "0",
            ["player_stats[0][Hero_Legionnaire][annihilation]"] = "0",
            ["player_stats[0][Hero_Legionnaire][ks3]"] = "1",
            ["player_stats[0][Hero_Legionnaire][ks4]"] = "0",
            ["player_stats[0][Hero_Legionnaire][ks5]"] = "0",
            ["player_stats[0][Hero_Legionnaire][ks6]"] = "0",
            ["player_stats[0][Hero_Legionnaire][ks7]"] = "0",
            ["player_stats[0][Hero_Legionnaire][ks8]"] = "0",
            ["player_stats[0][Hero_Legionnaire][ks9]"] = "0",
            ["player_stats[0][Hero_Legionnaire][ks10]"] = "0",
            ["player_stats[0][Hero_Legionnaire][ks15]"] = "0",
            ["player_stats[0][Hero_Legionnaire][smackdown]"] = "0",
            ["player_stats[0][Hero_Legionnaire][humiliation]"] = "0",
            ["player_stats[0][Hero_Legionnaire][nemesis]"] = "0",
            ["player_stats[0][Hero_Legionnaire][retribution]"] = "0",
            ["player_stats[0][Hero_Legionnaire][score]"] = "0",
            ["player_stats[0][Hero_Legionnaire][gameplaystat0]"] = "0",
            ["player_stats[0][Hero_Legionnaire][gameplaystat1]"] = "0",
            ["player_stats[0][Hero_Legionnaire][gameplaystat2]"] = "0",
            ["player_stats[0][Hero_Legionnaire][gameplaystat3]"] = "0",
            ["player_stats[0][Hero_Legionnaire][gameplaystat4]"] = "0",
            ["player_stats[0][Hero_Legionnaire][gameplaystat5]"] = "0",
            ["player_stats[0][Hero_Legionnaire][gameplaystat6]"] = "0",
            ["player_stats[0][Hero_Legionnaire][gameplaystat7]"] = "0",
            ["player_stats[0][Hero_Legionnaire][gameplaystat8]"] = "0",
            ["player_stats[0][Hero_Legionnaire][gameplaystat9]"] = "0",
            ["player_stats[0][Hero_Legionnaire][time_earning_exp]"] = "1700",

            // Inventory
            ["inventory[0][0]"] = "Item_LoggersHatchet",
        };
    }

    /// <summary>
    /// [UNVERIFIED] Auth helper.
    /// </summary>
    public static Dictionary<string, string> Auth(string login, string password)
    {
        return new Dictionary<string, string>
        {
            { "f", "auth" },
            { "login", login },
            { "password", password }
        };
    }

    /// <summary>
    /// [VERIFIED] PreAuth helper.
    /// </summary>
    public static Dictionary<string, string> PreAuth(string login)
    {
        return new Dictionary<string, string>
        {
            { "f", "pre_auth" },
            { "login", login },
            { "A", "0000000000000000000000000000000000000000" }, 
            { "SysInfo", "Matches|UserAgent" } 
        };
    }

    /// <summary>
    /// [UNVERIFIED] SRP Auth helper.
    /// </summary>
    public static Dictionary<string, string> SrpAuth(string login, string proof)
    {
        return new Dictionary<string, string>
        {
            { "f", "srpAuth" },
            { "login", login },
            { "proof", proof },
            { "OSType", "wa" },
            { "MajorVersion", "4" },
            { "MinorVersion", "10" },
            { "MicroVersion", "1" },
            { "SysInfo", "hash|hash|hash|hash|hash" }
        };
    }

    /// <summary>
    /// [UNVERIFIED] SRP Auth with raw 'A' parameter.
    /// </summary>
    public static Dictionary<string, string> SrpAuthWithA(string login, string A)
    {
        return new Dictionary<string, string>
        {
            { "f", "srpAuth" },
            { "login", login },
            { "A", A }
        };
    }
    
    /// <summary>
    /// [UNVERIFIED] Internal tool for creating cookies from AccountID.
    /// </summary>
    public static Dictionary<string, string> Aids2Cookie(string accountId, string ip, string authHash)
    {
        return new Dictionary<string, string>
        {
            { "f", "aids2cookie" },
            { "account_id", accountId },
            { "ip", ip },
            { "auth_hash", authHash }
        };
    }

    /// <summary>
    /// [UNVERIFIED] Variant of server list with 'get_server_list' function.
    /// </summary>
    public static Dictionary<string, string> GetServerList(string cookie)
    {
        return new Dictionary<string, string>
        {
            { "f", "get_server_list" },
            { "cookie", cookie },
            { "gametype", "10" }
        };
    }

    /// <summary>
    /// [UNVERIFIED] Variant of server list with 'grab_server_list' function.
    /// </summary>
    public static Dictionary<string, string> GrabServerList(string cookie)
    {
        return new Dictionary<string, string>
        {
            { "f", "grab_server_list" },
            { "cookie", cookie },
            { "gametype", "10" }
        };
    }

    /// <summary>
    /// [UNVERIFIED] Hero list retrieval.
    /// </summary>
    public static Dictionary<string, string> GetAllHeroes(string cookie)
    {
        return new Dictionary<string, string>
        {
            { "f", "get_all_heroes" },
            { "cookie", cookie }
        };
    }

    /// <summary>
    /// [UNVERIFIED] Hero list retrieval (variant).
    /// </summary>
    public static Dictionary<string, string> GetHeroList(string cookie)
    {
        return new Dictionary<string, string>
        {
            { "f", "get_hero_list" },
            { "cookie", cookie }
        };
    }

    /// <summary>
    /// [UNVERIFIED] Final match stats retrieval.
    /// </summary>
    public static Dictionary<string, string> FinalMatchStats(string cookie, int matchId)
    {
        return new Dictionary<string, string>
        {
            { "f", "final_match_stats" },
            { "cookie", cookie },
            { "match_id", matchId.ToString() }
        };
    }

    /// <summary>
    /// [UNVERIFIED] Account all hero stats.
    /// </summary>
    public static Dictionary<string, string> GetAccountAllHeroStats(string cookie)
    {
        return new Dictionary<string, string>
        {
            { "f", "get_account_all_hero_stats" },
            { "cookie", cookie }
        };
    }

    /// <summary>
    /// [UNVERIFIED] Account mastery retrieval.
    /// </summary>
    public static Dictionary<string, string> GetAccountMastery(string cookie, string nickname)
    {
        return new Dictionary<string, string>
        {
            { "f", "get_account_mastery" },
            { "cookie", cookie },
            { "nickname", nickname }
        };
    }

    /// <summary>
    /// [UNVERIFIED] Show stats (generic).
    /// </summary>
    public static Dictionary<string, string> ShowStats(string cookie, string nickname)
    {
        return new Dictionary<string, string>
        {
            { "f", "show_stats" },
            { "cookie", cookie },
            { "nickname", nickname }
        };
    }

    /// <summary>
    /// [UNVERIFIED] InitStats with snake_case.
    /// </summary>
    public static Dictionary<string, string> GetInitStats_SnakeCase(string cookie)
    {
        return new Dictionary<string, string>
        {
            { "f", "get_init_stats" }, 
            { "cookie", cookie }
        };
    }

    /// <summary>
    /// [UNVERIFIED] Daily special retrieval.
    /// </summary>
    public static Dictionary<string, string> GetDailySpecial(string cookie)
    {
        return new Dictionary<string, string>
        {
            { "f", "get_daily_special" },
            { "cookie", cookie }
        };
    }

    /// <summary>
    /// [UNVERIFIED] Guide list retrieval.
    /// </summary>
    public static Dictionary<string, string> GetGuideListFiltered(string cookie, string hero)
    {
        return new Dictionary<string, string>
        {
            { "f", "get_guide_list_filtered" },
            { "cookie", cookie },
            { "hero", hero }
        };
    }

    /// <summary>
    /// [UNVERIFIED] Specific guide retrieval.
    /// </summary>
    public static Dictionary<string, string> GetGuide(string cookie, string guideId)
    {
        return new Dictionary<string, string>
        {
            { "f", "get_guide" },
            { "cookie", cookie },
            { "guide_id", guideId }
        };
    }

    public static class ExpectedResponses
    {
        public static Dictionary<string, object> GetInitStats(
            string nickname = "TestUser", 
            int accountId = 1, 
            int level = 5, 
            int gold = 1000)
        {
            // Matches the structure defined in ClientRequesterController.Stats.cs (HandleInitStats)
            return new Dictionary<string, object>
            {
                { "nickname", nickname },
                { "account_id", accountId.ToString() }, // Legacy expects string sometimes, but object safe
                { "level", level },
                { "level_exp", 5000 },
                { "avatar_num", 0 },
                { "hero_num", 0 },
                { "total_played", 10 },
                { "season_id", 12 },
                { "season_level", 0 },
                { "creep_level", 0 },
                { "season_normal", new Dictionary<string, object>() }, // Simplified
                { "season_casual", new Dictionary<string, object>() }, // Simplified
                { "mvp_num", 0 },
                { "award_top4_name", new List<string>() },
                { "award_top4_num", new List<int>() },
                { "slot_id", "0" },
                { "selected_upgrades", new List<string>() },
                { "dice_tokens", 0 },
                { "game_tokens", 0 },
                { "timestamp", 1736190000 },
                { "vested_threshold", 5 },
                { "0", true } // Simple boolean true
            };
        }

        public static Dictionary<string, object> SubmitStatsSuccess() 
        {
                return new Dictionary<string, object> { }; // Usually empty or specific success structure? Reference Check likely needed, assuming HTTP 200 OK.
        }

        public static Dictionary<string, object> GetUpgrades()
        {
            return new Dictionary<string, object>
            {
                { "selected_upgrades", new List<string>() },
                { "my_upgrades", new List<string>() },
                { "my_upgrades_info", new Dictionary<string, object>() },
                { "0", true }
            };
        }

        public static Dictionary<string, object> GetMatchStats(int matchId)
        {
            return new Dictionary<string, object>
            {
                { "points", "1000" },
                { "mmpoints", "500" },
                { "match_summ", new List<object> 
                    { 
                        new Dictionary<string, object> 
                        {
                            { "match_id", matchId },
                            { "map", "caldavar" },
                            { "name", "Test Game" },
                            { "c_state", "0" },
                            { "time_played", "1800" }
                        } 
                    } 
                },
                { "match_player_stats", new List<object>() },
                { "inventory", new List<object>() },
                { "my_upgrades", new List<string>() },
                { "selected_upgrades", new List<string>() },
                { "dice_tokens", 100 },
                { "game_tokens", 100 },
                { "vested_threshold", 5 },
                { "0", true }
            };
        }
    }
}
