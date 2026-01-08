using System.Collections.Generic;

namespace ASPIRE.Tests.Data;

/// <summary>
/// Consolidated payload examples for Miscellaneous Controllers (Stats, Store, Patcher, Quest, Message, StorageStatus).
/// Contains primarily Unverified payloads as these controllers are less used.
/// </summary>
public static class MiscRequestPayloads
{
    public static class Unverified
    {
        // Stats Requester - Resubmit
        public static Dictionary<string, string> ResubmitStats(string cookie, int matchId, string date, string compressedData)
        {
            // Flattened dictionary to match ASP.NET Core Model Binding for StatsForSubmissionRequestForm
            // which expects keys like match_stats[match_id], player_stats[0][Hero_Legionnaire][nickname], etc.
            return new Dictionary<string, string>
            {
                { "f", "resubmit_stats" }, // Function name for routing
                // The controller actually checks 'login', 'pass', 'resubmission_key'
                { "login", "ServerHostAccount" },
                { "pass", "password_hash" },
                { "resubmission_key", "valid_key" }, // Needs to be valid hash in real scenario
                { "server_id", "1" },
                { "session", cookie },
                
                // match_stats dictionary
                { "match_stats[match_id]", matchId.ToString() },
                
                // player_stats dictionary: [playerIndex][heroKey][statKey]
                { "player_stats[0][Hero_Legionnaire][nickname]", "TestPlayer" },
                { "player_stats[0][Hero_Legionnaire][wins]", "1" },
                
                // Other required dictionaries implicitly
                { "team_stats[1][win]", "1" },
                
                // Legacy / Extra params
                { "date", date },
                { "data", compressedData }
            };
        }

        // Store Requester
        public static Dictionary<string, string> StoreRequest(string cookie, string category = "featured")
        {
            // Generic stub as StoreRequester logic is currently a stub
            return new Dictionary<string, string>
            {
                // 'f' parameter is not explicitly switched in the stub, but often used
                { "f", "get_catalog" }, 
                { "cookie", cookie },
                { "category", category }
            };
        }

        // Patcher
        public static Dictionary<string, string> LatestPatch(string cookie, string os, string arch, string currentVersion)
        {
            return new Dictionary<string, string>
            {
                // Function implicitly handled by route, but parameters are bound from form
                { "update", "1" },
                { "version", "0.0.0.0" },
                { "current_version", currentVersion },
                { "os", os },
                { "arch", arch },
                { "cookie", cookie }
            };
        }

        // Quest
        public static Dictionary<string, string> GetCurrentQuests(string cookie)
        {
            return new Dictionary<string, string>
            {
                // Route: master/quest/getcurrentquests (Action is GetCurrentQuests)
                { "cookie", cookie }
            };
        }

        public static Dictionary<string, string> GetPlayerQuests(string cookie)
        {
            return new Dictionary<string, string>
            {
                 // Route: master/quest/getplayerquests
                { "cookie", cookie }
            };
        }

        // Message
        public static Dictionary<string, string> ListMessages(string cookie, int id)
        {
            return new Dictionary<string, string>
            {
                // Route: message/list/{id}
                { "cookie", cookie } 
                // 'id' is in route, but usually cookies are sent in body
            };
        }

        // Storage Status
        public static Dictionary<string, string> StorageStatus(string cookie)
        {
            return new Dictionary<string, string>
            {
                // Route: master/storage/status
                { "cookie", cookie }
            };
        }
    }
}
