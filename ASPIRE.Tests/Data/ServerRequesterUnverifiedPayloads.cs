using System.Collections.Generic;

namespace ASPIRE.Tests.Data;

/// <summary>
/// Unverified payload examples for the Server Requester.
/// These payloads are placeholders or theoretical structures based on controller logic,
/// but have not yet been fully verified against a live server or captured traffic.
/// </summary>
public static class ServerRequesterUnverifiedPayloads
{
    // Server Manager
    public static Dictionary<string, string> ReplayAuth(string login, string password)
    {
        return new Dictionary<string, string>
        {
            { "f", "replay_auth" },
            { "login", login },
            { "pass", password }
        };
    }

    public static Dictionary<string, string> GetSpectatorHeader(string matchId)
    {
        return new Dictionary<string, string>
        {
            { "f", "get_spectator_header" },
            { "match_id", matchId }
        };
    }

    public static Dictionary<string, string> SetReplaySize(string matchId, int size)
    {
        return new Dictionary<string, string>
        {
            { "f", "set_replay_size" },
            { "match_id", matchId },
            { "size", size.ToString() }
        };
    }

    public static Dictionary<string, string> GetQuickStats(string session)
    {
        return new Dictionary<string, string>
        {
            { "f", "get_quickstats" },
            { "session", session }
        };
    }

    // Server
    public static Dictionary<string, string> AcceptKey(string cookie, int accountId)
    {
        return new Dictionary<string, string>
        {
            { "f", "accept_key" },
            { "cookie", cookie },
            { "account_id", accountId.ToString() }
        };
    }

    public static Dictionary<string, string> Auth(string login, string password)
    {
        return new Dictionary<string, string>
        {
            { "f", "auth" },
            { "login", login },
            { "pass", password }
        };
    }

    public static Dictionary<string, string> CConn(string cookie, string ip, int accountId)
    {
        return new Dictionary<string, string>
        {
            { "f", "c_conn" },
            { "cookie", cookie },
            { "ip", ip },
            { "account_id", accountId.ToString() }
        };
    }

    public static Dictionary<string, string> NewSession(string ip, int port)
    {
        return new Dictionary<string, string>
        {
            { "f", "new_session" },
            { "ip", ip },
            { "port", port.ToString() }
        };
    }

    public static Dictionary<string, string> Shutdown(string cookie)
    {
        return new Dictionary<string, string>
        {
            { "f", "shutdown" },
            { "session", cookie }
        };
    }

    public static Dictionary<string, string> StartGame(string cookie, int matchId)
    {
        return new Dictionary<string, string>
        {
            { "f", "start_game" },
            { "session", cookie },
            { "map", "caldavar" },
            { "version", "4.10.1.0" },
            { "mname", "Test Game" },
            { "mstr", "ServerHostAccount:" },
            { "casual", "0" },
            { "arrangedmatchtype", "0" },
            { "match_mode", "1" }
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
}
