using System.Collections.Generic;

namespace ASPIRE.Tests.Data;

/// <summary>
/// Consolidated payload examples for the Server Requester.
/// Contains both Verified (proven to work) and Unverified (theoretical) payloads.
/// </summary>
public static class ServerRequestPayloads
{
    public static class Verified
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
                cgt: 60000); // 1 minute in
        }

        public static Dictionary<string, string> ReplayAuth(string login, string password)
        {
            return new Dictionary<string, string>
            {
                { "f", "replay_auth" },
                { "login", login },
                { "pass", password }
            };
        }

        public static Dictionary<string, string> GetSpectatorHeader()
        {
             return new Dictionary<string, string>
            {
                { "f", "get_spectator_header" }
            };
        }

        public static Dictionary<string, string> NewSession(string login, string password, int port, string name, string description, string location, string ip)
        {
            return new Dictionary<string, string>
            {
                { "f", "new_session" },
                { "login", login },
                { "pass", password }, // Note: In reality this is HASHED
                { "port", port.ToString() },
                { "name", name },
                { "desc", description },
                { "location", location },
                { "ip", ip },
                { "version", "4.10.1.0" }, // standard version
                { "max_players", "10" }
            };
        }

        public static Dictionary<string, string> ClientConnection(string cookie, string ip, int accountId)
        {
            return new Dictionary<string, string>
            {
                { "f", "c_conn" },
                { "cookie", cookie },
                { "session", "dummy_server_session" }, // Required by HandleConnectClient
                { "ip", ip },
                { "cas", "0" }, // Required parameter
                { "new", "1" }, // Required parameter (ArrangedMatchType + 1)
                { "account_id", accountId.ToString() }
            };
        }

        public static Dictionary<string, string> AcceptKey(string cookie, int accountId)
        {
             return new Dictionary<string, string>
            {
                { "f", "accept_key" },
                { "session", cookie }, // HandleAcceptKey expects 'session', not 'cookie'
                { "acc_key", "test_key" }, // Required
                { "account_id", accountId.ToString() }
            };
        }

        public static Dictionary<string, string> Aids2Cookie(string cookie)
        {
            return new Dictionary<string, string>
            {
                { "f", "aids2cookie" },
                { "cookie", cookie }
            };
        }

        public static Dictionary<string, string> GetQuickStats(string session)
        {
            return new Dictionary<string, string>
            {
                { "f", "get_quickstats" },
                { "session", session } // server session cookie
            };
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

        public static Dictionary<string, string> Shutdown(string cookie)
        {
            return new Dictionary<string, string>
            {
                { "f", "shutdown" },
                { "session", cookie }
            };
        }

        public static Dictionary<string, string> StartGame(string cookie, int matchId, string mname = "Test Game")
        {
            return new Dictionary<string, string>
            {
                { "f", "start_game" },
                { "session", cookie },
                { "map", "caldavar" },
                { "version", "4.10.1.0" },
                { "mname", mname },
                { "mstr", "ServerHostAccount:" }, // Controller appends this
                { "casual", "0" },
                { "arrangedmatchtype", "0" },
                { "match_mode", "1" }
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

    public static class Unverified
    {
        public static Dictionary<string, string> SetReplaySize(string matchId, int size)
        {
            return new Dictionary<string, string>
            {
                { "f", "set_replay_size" },
                { "match_id", matchId },
                { "size", size.ToString() }
            };
        }

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
}
