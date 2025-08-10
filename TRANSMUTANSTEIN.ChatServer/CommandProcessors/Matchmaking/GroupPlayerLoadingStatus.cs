namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

[ChatCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_PLAYER_LOADING_STATUS)]
public class GroupPlayerLoadingStatus(ILogger<GroupPlayerLoadingStatus> logger) : CommandProcessorsBase, ICommandProcessor
{
    private ILogger<GroupPlayerLoadingStatus> Logger { get; } = logger;

    public async Task Process(ChatSession session, ChatBuffer buffer)
    {
        // Decode minimal request (loadingStatus byte)
        byte[] commandBytes = buffer.ReadCommandBytes();
        byte loadingStatus = buffer.ReadInt8();

        // TODO: Persist loading status in group object when group model exists.
        // Single-player placeholder logic: when reaches 100 we simulate queue join -> match found -> lobby.

        // Always echo a partial group update reflecting loading percent.
        Response.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_UPDATE);
        Response.WriteInt8(Convert.ToByte(ChatProtocol.TMMUpdateType.TMM_PARTIAL_GROUP_UPDATE));
        Response.WriteInt32(session.ClientInformation.Account.ID); // Account changing status
        Response.WriteInt8(1);    // Group size
        Response.WriteInt16(1500);// Avg rating placeholder
        Response.WriteInt32(session.ClientInformation.Account.ID); // Leader
        Response.WriteInt8(Convert.ToByte(ChatProtocol.ArrangedMatchType.AM_MATCHMAKING));
        Response.WriteInt8(Convert.ToByte(ChatProtocol.TMMGameType.TMM_GAME_TYPE_NORMAL));
        Response.WriteString("caldavar");
        Response.WriteString("ap|ar|sd");
        Response.WriteString("EU|USE|USW");
        Response.WriteBool(true);  // Ranked
        Response.WriteBool(true);  // Match Fidelity
        Response.WriteInt8(1);     // Bot Difficulty
        Response.WriteBool(false); // Randomize Bots
        Response.WriteString(string.Empty); // Country Restrictions
        Response.WriteString(string.Empty); // Invite Responses
        Response.WriteInt8(5); // Team size
        Response.WriteInt8(Convert.ToByte(ChatProtocol.TMMType.TMM_TYPE_CAMPAIGN)); // Group Type

        // Member block
        Response.WriteInt32(session.ClientInformation.Account.ID);
        Response.WriteString(session.ClientInformation.Account.Name);
        Response.WriteInt8(1);                // Slot
        Response.WriteInt16(1500);            // Rating
        Response.WriteInt8(loadingStatus);    // Loading Percent
        Response.WriteBool(false);            // Ready (unchanged here)
        Response.WriteBool(false);            // In game
        Response.WriteBool(true);             // Eligible
        Response.WriteString(session.ClientInformation.Account.ChatNameColour);
        Response.WriteString(session.ClientInformation.Account.Icon);
        Response.WriteString("NEWERTH");      // Country placeholder
        Response.WriteBool(true);             // Has game mode access
        Response.WriteString("true|true|true");

        Response.WriteBool(false); // Friend flag placeholder

        Response.PrependBufferSize();
        session.SendAsync(Response.Data);

        if (loadingStatus == 100)
        {
            // Simulate: group joins queue (0x0D01), then immediately finds a match (0x0D09), and enters game lobby (0x1C09).
            await SendBare(session, ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_JOIN_QUEUE); // 0x0D01
            await SendBare(session, ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_MATCH_FOUND_UPDATE); // 0x0D09
            await SendBare(session, ChatProtocol.ChatServerToClient.NET_CHAT_CL_GAME_LOBBY_JOINED); // 0x1C09
        }
    }

    private async Task SendBare(ChatSession session, ushort command)
    {
        Response.Clear();
        Response.WriteCommand(command);
        Response.PrependBufferSize();
        session.SendAsync(Response.Data);
    }
}

/*
Minimal Placeholder Flow Extension (0x0D04 -> Lobby)

Purpose:
Provide a skeletal pathway from player loading status updates to a simulated game lobby entry without implementing real matchmaking / queue logic.

Decoded Request:
- Bytes: [ size-prefixed command ][ loadingStatus ]

Behavior Implemented:
1. Echo partial group update (0x0D03 semantics) including updated loading percent for a single-member group.
2. When loadingStatus == 100:
   a. Send bare GroupJoinQueue (0x0D01) as if the group transitioned into the matchmaking queue.
   b. Send bare MatchFoundUpdate (0x0D09) instantly (skipping timing, queue logic, region / MMR matching).
   c. Send bare GAME_LOBBY_JOINED (0x1C09) to emulate successful transition into a lobby.

Skipped / Deferred (TODO):
- Accumulating multi-member loading status and only proceeding when all reach 100.
- Maintaining a real MatchmakingGroup state machine (WaitingToStart -> LoadingResources -> InQueue -> MatchFound ...).
- Sending StartLoading (0x0F03) or PendingMatch (0x0F04) intermediate steps.
- Emitting proper MatchFoundUpdate payload (currently empty frame only).
- Constructing valid Game Lobby payload (0x1C09 currently sent with no body; real clients may expect lobby metadata and slots).
- Queue rejoin (0x0E0C) vs fresh join differentiation.
- Queue time updates (0x0D06) and popularity updates (0x0D07).

Future Incremental Steps:
1. Introduce in-memory group store with per-member LoadingPercent and Ready flag.
2. Add Ready handler (already placeholder) to set group state and emit StartLoading (0x0F03) before first 0x0D04.
3. Replace immediate jump with:
   - After all 100%: send GroupJoinQueue (0x0D01) then a delayed MatchFoundUpdate (0x0D09).
4. Implement minimal payload structures for 0x0D09 and 0x1C09 following KONGOR formats.
5. Add PendingMatch (0x0F04) / AcceptPendingMatch (0x0F05) cycle.
6. Populate lobby roster using real group membership.

This file documents each placeholder so future work can fill in real matchmaking logic without guessing current shortcuts.
*/
