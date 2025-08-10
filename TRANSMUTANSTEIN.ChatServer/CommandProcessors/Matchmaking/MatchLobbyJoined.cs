namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

[ChatCommand(ChatProtocol.ChatServerToClient.NET_CHAT_CL_GAME_LOBBY_JOINED)]
public class MatchLobbyJoined(ILogger<MatchLobbyJoined> logger) : CommandProcessorsBase, ICommandProcessor
{
    private ILogger<MatchLobbyJoined> Logger { get; } = logger;

    public async Task Process(ChatSession session, ChatBuffer buffer)
    {
        // Client should not normally send this; consume if it does.
        byte[] commandBytes = buffer.ReadCommandBytes();

        // Derive placeholder lobby context from matchmaking group (if any)
        var group = MatchmakingService.FindGroupForAccount(session.ClientInformation.Account.ID);

        // TODO: Real lobby ID generation & persistent lobby store
        int lobbyId = 1; // placeholder
        byte teamSize = group?.TeamSize ?? 5;
        string map = group?.MapName ?? "caldavar";
        string gameModes = group?.GameModes ?? "ap|ar|sd"; // TODO split & choose active
        ChatProtocol.TMMGameType gameType = group?.GameType ?? ChatProtocol.TMMGameType.TMM_GAME_TYPE_NORMAL;

        // Build simplified lobby joined payload (speculative format – placeholder)
        Response.Clear();
        Response.WriteCommand(ChatProtocol.ChatServerToClient.NET_CHAT_CL_GAME_LOBBY_JOINED);
        Response.WriteInt32(lobbyId);                // Lobby / match handle (placeholder)
        Response.WriteInt8(Convert.ToByte(gameType));// Game type
        Response.WriteString(map);                   // Map
        Response.WriteString(gameModes);             // Modes string
        Response.WriteInt8(teamSize);                // Team size
        Response.WriteInt8(0);                       // Lobby state (0 = adjust lobby) TODO map to ChatProtocol.GameLobbyState
        Response.WriteBool(true);                    // PvP flag placeholder
        Response.WriteInt32(Environment.TickCount);  // Start time placeholder
        Response.WriteInt8(2);                       // Number of teams (fixed 2)

        // For each team, write placeholder slots. Team 0 contains all current group members; team 1 empty.
        for (int teamIndex = 0; teamIndex < 2; teamIndex++)
        {
            Response.WriteInt8(Convert.ToByte(teamIndex));    // Team index
            Response.WriteInt8(teamSize);                     // Team capacity
            if (teamIndex == 0 && group != null)
            {
                Response.WriteInt8(Convert.ToByte(group.Members.Count)); // current players in team
                foreach (var m in group.Members)
                {
                    Response.WriteInt32(m.Account.ID);        // Account ID
                    Response.WriteString(m.Account.Name);     // Name
                    Response.WriteInt8(m.Slot);               // Slot index
                    Response.WriteString(string.Empty);       // Hero (not picked yet) TODO: hero code
                    Response.WriteString(string.Empty);       // Avatar (skin) TODO
                    Response.WriteBool(false);                // Potential pick flag
                    Response.WriteBool(m.IsReady);            // Ready flag
                }
            }
            else
            {
                Response.WriteInt8(0); // zero players currently
            }
        }

        // TODO: Additional lobby metadata: pick/bans timers, spectators allowed, server region, version hashes, chat channel id
        // TODO: After this packet, emit per-player join updates (NET_CHAT_CL_GAME_LOBBY_PLAYER_JOINED) for clients needing incremental updates.

        Response.PrependBufferSize();
        session.SendAsync(Response.Data);
    }
}

/*
MatchLobbyJoined (0x1C09) enhanced placeholder toward c_gamelobbymanager parity:
Content now includes:
- Lobby id, game type, map, modes, team size, lobby state, PvP flag, start timestamp.
- Two team blocks: team index, capacity, player count, per-player info (accountId, name, slot, hero, avatar, potentialPick, ready).
Simplifications vs real HoN implementation (c_gamelobbymanager.cpp):
- Single lobby id constant; no dynamic handle increment.
- All group members placed on team 0; opposing team empty; no bot population.
- No hero selection phase state machine or timers.
- Omits match balancing metrics, queue time retention, combine method stats, chat channel creation.
- No subsequent PLAYER_JOINED / PLAYER_UPDATE packets; data inlined once.
TODOs for fuller parity:
1. Introduce LobbyManager to allocate unique handles and maintain CGameLobby equivalent.
2. Split gameModes string and designate active mode (and send flags separately if protocol requires).
3. Implement hero selection / banning phases with NET_CHAT_CL_GAME_LOBBY_PLAYER_UPDATE (0x1C0E) packets.
4. Add spectators, avatars, potential pick flags, pregame timers, region & server info.
5. Broadcast per-member join/leave/change updates after initial join frame for late joiners.
6. Integrate with match server allocation; delay lobby join until server ready.
7. Support bot matches (populate team 1 with bots, assign heroes).
8. Persist ready states separately from matchmaking readiness once in lobby.
*/
