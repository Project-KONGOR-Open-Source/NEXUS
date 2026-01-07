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
    // Server Manager
    // ReplayAuth moved to VerifiedPayloads
    // GetSpectatorHeader moved to VerifiedPayloads
    
    public static Dictionary<string, string> SetReplaySize(string matchId, int size)
    {
        return new Dictionary<string, string>
        {
            { "f", "set_replay_size" },
            { "match_id", matchId },
            { "size", size.ToString() }
        };
    }

    // GetQuickStats moved to VerifiedPayloads

    // Server
    // AcceptKey moved to VerifiedPayloads
    // Auth - Removed (Likely old/unused)
    // CConn moved to VerifiedPayloads
    // NewSession moved to VerifiedPayloads
    // Shutdown moved to VerifiedPayloads
    // StartGame moved to VerifiedPayloads

    public static Dictionary<string, string> Aids2Cookie(string accountId, string ip, string authHash)
    {
        return new Dictionary<string, string>
        {
            { "f", "aids2cookie" },
            { "account_id", accountId },
            { "ip", ip },
            { "auth_hash", authHash },
            { "cookie", "dummy_cookie" } // Required to pass 400 check and hit 401
        };
    }
}
