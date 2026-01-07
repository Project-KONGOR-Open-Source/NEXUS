
using System.Collections.Generic;

namespace ASPIRE.Tests.Data;

/// <summary>
/// Verified payload examples for the Server Requester.
/// </summary>
public static class ServerRequesterVerifiedPayloads
{
    public static Dictionary<string, string> Heartbeat(string cookie)
    {
        // Heartbeat is effectively a set_online call without 'new' flag
        // and updating connection state.
        return SetOnline(
            cookie: cookie, 
            matchId: 0, // Not strictly used in handler but good for compat
            isNew: false,
            c_state: "SERVER_STATUS_ACTIVE",
            prev_c_state: "SERVER_STATUS_ACTIVE",
            num_conn: 0,
            cgt: 60000 // 1 minute in
        );
    }

    public static Dictionary<string, string> SetOnline(
        string cookie, 
        int matchId, 
        int port = 11235, 
        string ip = "127.0.0.1",
        string version = "4.10.1",
        string map = "caldavar",
        string name = "Test Server",
        string location = "US",
        bool isNew = true,
        // Required by HandleSetOnline
        string c_state = "SERVER_STATUS_ACTIVE", 
        string prev_c_state = "SERVER_STATUS_LOADING", 
        int num_conn = 0,
        int cgt = 0
        )
    {
            Dictionary<string, string> payload = new Dictionary<string, string>
        {
            { "f", "set_online" },
            { "session", cookie }, // Code uses 'session' param
            { "match_id", matchId.ToString() },
            { "port", port.ToString() },
            { "ip", ip },
            { "version", version },
            { "map", map },
            { "name", name },
            { "location", location },
            { "new", isNew ? "1" : "0" }, // Logic uses existence check?
            
            // Required parameters found in HandleSetOnline
            { "num_conn", num_conn.ToString() },
            { "cgt", cgt.ToString() }, // Current Game Time
            { "private", "0" }, // 'private' keyword reserved? Use string key
            { "vip", "0" },
            { "c_state", c_state },
            { "prev_c_state", prev_c_state }
        };

        if (!isNew)
        {
            payload.Remove("new"); // HandleSetOnline checks existence: calling with null might differ
        }

        return payload;
    }

        public static Dictionary<string, string> GameEnded(string cookie, int matchId)
    {
        return new Dictionary<string, string>
        {
            { "f", "game_ended" },
            { "cookie", cookie },
            { "match_id", matchId.ToString() }
        };
    }

    public static class ExpectedResponses
    {
        public static Dictionary<string, object> Heartbeat()
        {
            // Simple OK response
            return new Dictionary<string, object> { { "0", true } }; 
        }

        public static Dictionary<string, object> SetOnline(int matchId)
        {
            // Returns the match ID upon success
            return new Dictionary<string, object> { { "match_id", matchId } };
        }

         public static Dictionary<string, object> GameEnded()
        {
            // Returns bool true on success
            return new Dictionary<string, object> { { "0", true } };
        }
    }
}
