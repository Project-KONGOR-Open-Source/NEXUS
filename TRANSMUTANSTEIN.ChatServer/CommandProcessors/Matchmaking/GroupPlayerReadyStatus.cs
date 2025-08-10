namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

[ChatCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_PLAYER_READY_STATUS)]
public class GroupPlayerReadyStatus(ILogger<GroupPlayerReadyStatus> logger) : CommandProcessorsBase, ICommandProcessor
{
    private ILogger<GroupPlayerReadyStatus> Logger { get; } = logger;

    public async Task Process(ChatSession session, ChatBuffer buffer)
    {
        // Decode minimal request (readyStatus, gameType) matching KONGOR's MatchmakingGroupPlayerReadyStatusRequest
        byte[] commandBytes = buffer.ReadCommandBytes();
        byte readyStatus = buffer.ReadInt8();
        byte gameType = buffer.ReadInt8();

        // TODO: Validate that session is in a group, distinguish leader vs member, and persist ready state.
        // TODO: When leader readies (readyStatus != 0) transition group state to LoadingResources and emit 0x0F03 later.

        Response.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_UPDATE);
        // Use partial update to better represent an in-place status change.
        Response.WriteInt8(Convert.ToByte(ChatProtocol.TMMUpdateType.TMM_PARTIAL_GROUP_UPDATE));

        // Actor account id (who changed ready state)
        Response.WriteInt32(session.ClientInformation.Account.ID);

        // Group size placeholder (solo)
        Response.WriteInt8(1);

        // Average group rating placeholder
        Response.WriteInt16(1500);

        // Leader account id (self)
        Response.WriteInt32(session.ClientInformation.Account.ID);

        // Reuse same static game option placeholders as join
        Response.WriteInt8(Convert.ToByte(ChatProtocol.ArrangedMatchType.AM_MATCHMAKING));
        Response.WriteInt8(Convert.ToByte(ChatProtocol.TMMGameType.TMM_GAME_TYPE_NORMAL));
        Response.WriteString("caldavar");
        Response.WriteString("ap|ar|sd");
        Response.WriteString("EU|USE|USW");
        Response.WriteBool(true);   // Ranked
        Response.WriteBool(true);   // Match Fidelity
        Response.WriteInt8(1);      // Bot Difficulty
        Response.WriteBool(false);  // Randomize Bots
        Response.WriteString(string.Empty); // Country Restrictions
        Response.WriteString(string.Empty); // Invite Responses
        Response.WriteInt8(5); // Team size
        Response.WriteInt8(Convert.ToByte(ChatProtocol.TMMType.TMM_TYPE_CAMPAIGN)); // Group Type

        // Single member block
        Response.WriteInt32(session.ClientInformation.Account.ID);
        Response.WriteString(session.ClientInformation.Account.Name);
        Response.WriteInt8(1);                // Slot
        Response.WriteInt16(1500);            // Rating
        Response.WriteInt8(0);                // Loading Percent
        Response.WriteBool(readyStatus != 0); // Ready
        Response.WriteBool(false);            // In game
        Response.WriteBool(true);             // Eligible
        Response.WriteString(session.ClientInformation.Account.ChatNameColour);
        Response.WriteString(session.ClientInformation.Account.Icon);
        Response.WriteString("NEWERTH");      // Country placeholder
        Response.WriteBool(true);             // Has game mode access
        Response.WriteString("true|true|true"); // Game mode access placeholder

        Response.WriteBool(false); // Friend flag list (single member)

        Response.PrependBufferSize();
        session.SendAsync(Response.Data);
    }
}

/*
 
Flow of 0x0D05 (Group Player Ready Status) in KONGOR:

Client sends 0x0D05 with two bytes: readyStatus (0/1) and gameType.
Factory decodes to MatchmakingGroupPlayerReadyStatusRequest.
BeforeProcess():
Grabs current group, gathers participant account IDs.
Loads each participant’s MMR + placement + ping info depending on game type (ranked, casual, midwars, reborn, etc.).
Builds PlayersInfo list (ratings, placement flags, ping maps).
Process():
Validates account & group.
Calls matchmakingGroup.UpdateParticipantReadyStatus(account, PlayersInfo, TournamentRating, ReadyStatus).
UpdateParticipantReadyStatus():
Ignores non-leader callers (only leader readiness matters in current UX; others treated as implicitly ready).
Stores PlayersInfo, TournamentRating.
If readyStatus != 0 → EnterQueue().
EnterQueue():
(Placement / disparity checks – some commented out).
Validates MMR disparity, region restrictions for casual.
Sets State = LoadingResources, broadcasts partial update (group update packet).
Sends MatchmakingStartLoadingResponse (0x0F03 equivalent).
Starts a timer to advance loading; players then report loading progress via 0x0D04.
Players then send 0x0D04 (loading status) until all reach 100%.
When all loaded (UpdateParticipantLoadingStatus):
Add group to GameFinder queue (may send GroupJoinQueue or GroupRejoinQueue).
Optionally sends GroupQueueUpdate (type 11, includes avg time).
Broadcasts partial update, stops timer.
Later GameFinder events drive match found, accept pending match, etc.
Summary of ready-up semantics:

Only leader’s explicit ready action triggers queue pipeline.
Other members’ readiness UI not required; assumed ready.
0x0D05 acts as the transition from idle (WaitingToStart) to LoadingResources and queue preparation.
Ratings and ping info snapshot taken at that moment for queue construction.
Actual queue insertion delayed until all clients finish pre-load (via 0x0D04 progress).
Minimal NEXUS implementation added:

Created GroupPlayerReadyStatus.cs.
Decodes two bytes (readyStatus, gameType).
Sends a placeholder TMM_PARTIAL_GROUP_UPDATE (0x0D03) echoing single-member group with updated ready flag.
Does not persist state, perform MMR aggregation, or queue logic.
Next enhancement steps (future work):

Link sessions to an in-memory MatchmakingGroup object (store in MatchmakingService).
Persist IsReady on member; when leader sets ready true transition to a simple Loading state.
Add 0x0D04 handler to update LoadingPercent; when all 100% simulate queue join.
Emit distinct update types (FULL vs PARTIAL) according to membership or status changes.
Build rating aggregation and region/mode fields from stored group configuration instead of constants.
Implement queue join responses (0x0D01 path analogs: join/rejoin/queue update).
 
 */
