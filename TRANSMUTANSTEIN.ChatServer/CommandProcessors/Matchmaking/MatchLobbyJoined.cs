namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

[ChatCommand(ChatProtocol.ChatServerToClient.NET_CHAT_CL_GAME_LOBBY_JOINED)]
public class MatchLobbyJoined(ILogger<MatchLobbyJoined> logger) : CommandProcessorsBase, ICommandProcessor
{
    private ILogger<MatchLobbyJoined> Logger { get; } = logger;

    public async Task Process(ChatSession session, ChatBuffer buffer)
    {
        // Normally server only sends this; placeholder accepts and echoes for symmetry.
        byte[] commandBytes = buffer.ReadCommandBytes();
        Response.WriteCommand(ChatProtocol.ChatServerToClient.NET_CHAT_CL_GAME_LOBBY_JOINED);
        // TODO: Populate lobby composition (teams, slots, map, mode, timers) aligned with HoN lobby protocol.
        Response.PrependBufferSize();
        session.SendAsync(Response.Data);
    }
}

/*
Placeholder Handler: GAME_LOBBY_JOINED (0x1C09)

Purpose:
Represent arrival into a game lobby after matchmaking. Real implementation is server-emitted only, carrying full lobby state for UI initialization.

Deferred Payload Elements:
- Lobby identifier / match id.
- Team slot roster (accountId, name, slot index, hero picks if any, readiness).
- Game options (map, modes, spectators allowed, bans/picks timers).
- Access flags, permissions for host/leader.
- Countdown / state (banning, picking, ready, launching).

Current Behavior:
- Returns bare command frame with no body to satisfy client parser expecting header.

Roadmap:
1. Define LobbyState model storing members and phase.
2. Emit LOBBY_CLIENT_ON_ENTER style updates (1C0C/1C0D/1C0E analogs) after initial join frame.
3. Transition to hero select flow when implemented.
4. Hook to match server allocation step rather than direct sequence.
*/
