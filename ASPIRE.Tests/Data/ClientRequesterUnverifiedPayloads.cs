using System.Collections.Generic;

namespace ASPIRE.Tests.Data;

/// <summary>
/// Unverified payload examples for the Client Requester.
/// These payloads are placeholders or theoretical structures based on controller logic,
/// but have not yet been fully verified against a live client or captured traffic.
/// </summary>
public static class ClientRequesterUnverifiedPayloads
{
    // Authentication
    public static Dictionary<string, string> Auth(string login, string password)
    {
        return new Dictionary<string, string>
        {
            { "f", "auth" },
            { "login", login },
            { "password", password }
        };
    }

    public static Dictionary<string, string> PreAuth(string login)
    {
        return new Dictionary<string, string>
        {
            { "f", "pre_auth" },
            { "login", login }
        };
    }

    public static Dictionary<string, string> SrpAuth(string login, string A)
    {
        return new Dictionary<string, string>
        {
            { "f", "srpAuth" },
            { "login", login },
            { "A", A }
        };
    }

    public static Dictionary<string, string> Logout(string cookie)
    {
        return new Dictionary<string, string>
        {
            { "f", "logout" },
            { "cookie", cookie }
        };
    }

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

    // Server List
    public static Dictionary<string, string> GetServerList(string cookie)
    {
        return new Dictionary<string, string>
        {
            { "f", "get_server_list" },
            { "cookie", cookie },
            { "gametype", "10" } // 10 = Join List, 90 = Create List
        };
    }

    // Heroes
    public static Dictionary<string, string> GetAllHeroes(string cookie)
    {
        return new Dictionary<string, string>
        {
            { "f", "get_all_heroes" }, // or get_hero_list
            { "cookie", cookie }
        };
    }

    // Game
    public static Dictionary<string, string> CreateGame(string cookie, string name)
    {
        return new Dictionary<string, string>
        {
            { "f", "create_game" },
            { "cookie", cookie },
            { "name", name },
            { "map", "caldavar" },
            { "mode", "botmatch" }, // Added required mode parameter
            { "team_size", "5" },
            { "private", "0" }
        };
    }

    public static Dictionary<string, string> NewGameAvailable(string cookie)
    {
        return new Dictionary<string, string>
        {
            { "f", "new_game_available" },
            { "cookie", cookie }
        };
    }

    public static Dictionary<string, string> FinalMatchStats(string cookie, int matchId)
    {
        return new Dictionary<string, string>
        {
            { "f", "final_match_stats" },
            { "cookie", cookie },
            { "match_id", matchId.ToString() }
        };
    }

    // Stats
    public static Dictionary<string, string> ShowSimpleStats(string cookie, string nickname)
    {
        return new Dictionary<string, string>
        {
            { "f", "show_simple_stats" }, // or show_stats
            { "cookie", cookie },
            { "nickname", nickname }
        };
    }

    public static Dictionary<string, string> GetAccountAllHeroStats(string cookie, string nickname)
    {
        return new Dictionary<string, string>
        {
            { "f", "get_account_all_hero_stats" },
            { "cookie", cookie },
            { "nickname", nickname }
        };
    }
    
    public static Dictionary<string, string> GetAccountMastery(string cookie, string nickname)
    {
        return new Dictionary<string, string>
        {
            { "f", "get_account_mastery" },
            { "cookie", cookie },
            { "nickname", nickname }
        };
    }

    // Upgrades/Store
    public static Dictionary<string, string> GetDailySpecial(string cookie)
    {
        return new Dictionary<string, string>
        {
            { "f", "get_daily_special" },
            { "cookie", cookie }
        };
    }

    // Guides
    public static Dictionary<string, string> GetGuideListFiltered(string cookie, string hero)
    {
        return new Dictionary<string, string>
        {
            { "f", "get_guide_list_filtered" },
            { "cookie", cookie },
            { "hero", hero }
        };
    }

    public static Dictionary<string, string> GetGuide(string cookie, string guideId)
    {
        return new Dictionary<string, string>
        {
            { "f", "get_guide" },
            { "cookie", cookie },
            { "guide_id", guideId }
        };
    }

    // Events & Messages
    public static Dictionary<string, string> ClientEventsInfo(string cookie)
    {
        return new Dictionary<string, string>
        {
            { "f", "client_events_info" },
            { "cookie", cookie }
        };
    }

    public static Dictionary<string, string> GetSpecialMessages(string cookie)
    {
        return new Dictionary<string, string>
        {
            { "f", "get_special_messages" },
            { "cookie", cookie }
        };
    }
}
