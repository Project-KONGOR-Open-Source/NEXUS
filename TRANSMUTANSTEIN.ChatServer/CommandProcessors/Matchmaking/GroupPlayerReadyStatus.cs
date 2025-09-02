namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

[ChatCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_PLAYER_READY_STATUS)]
public class GroupPlayerReadyStatus(ILogger<GroupPlayerReadyStatus> logger) : CommandProcessorsBase, ICommandProcessor
{
    private ILogger<GroupPlayerReadyStatus> Logger { get; } = logger;

    public async Task Process(ChatSession session, ChatBuffer buffer)
    {
        /*
            // This handles new groups being created, and players getting kicked/leaving/joining from the groups because once the group 
	        // changes another update would need to be sent anyways.  It is designed to be stateless so any update will always provide 
	        // all the information required so we can avoid synchronization complications
	        ETMMUpdateType eUpdateType(static_cast<ETMMUpdateType>(pkt.ReadByte()));
	        uint uiAccountID(pkt.ReadInt());
	        byte yGroupSize(pkt.ReadByte());
	        ushort unAverageTMR(pkt.ReadShort());
	        uint uiGroupLeaderAccountID(pkt.ReadInt());
	        EArrangedMatchType eArrangedMatchType(static_cast<EArrangedMatchType>(pkt.ReadByte()));
	        byte yGameType(pkt.ReadByte());
	        tstring sMapName(pkt.ReadTString());
	        tstring sGameModes(pkt.ReadTString());
	        tstring sRegions(pkt.ReadTString());
	        bool bRanked(pkt.ReadByte() != 0);
	        uint uiMatchFidelity(pkt.ReadByte());
	        byte yBotDifficulty(pkt.ReadByte());
	        bool bRandomizeBots(pkt.ReadByte() != 0);
	        tstring sRestrictedRegions(pkt.ReadTString());
	        tstring sPlayerInvitationResponses(pkt.ReadTString());
	        byte yTeamSize(pkt.ReadByte());
	        byte yTMMType(pkt.ReadByte());

	        if (pkt.HasFaults() || yGroupSize > MAX_GROUP_SIZE)
		        return;

	        if (cc_printTMMUpdates)
	        {
		        Console << L"Received TMM update " << g_sTMMUpdateTypes[eUpdateType] << newl;
	        }
	
	        // We need to store these out permanently so we can check for restricted regions
	        m_yGroupSize = yGroupSize;

	        m_yArrangedMatchType = eArrangedMatchType;
	        m_yGameType = yGameType;
	        m_sTMMMapName = sMapName;
	        cc_TMMMatchFidelity = uiMatchFidelity;
	        m_sRestrictedRegions = sRestrictedRegions;

	        bool bHandleFullUpdate(eUpdateType == TMM_CREATE_GROUP || eUpdateType == TMM_FULL_GROUP_UPDATE || eUpdateType == TMM_PLAYER_JOINED_GROUP || eUpdateType == TMM_PLAYER_LEFT_GROUP || eUpdateType == TMM_PLAYER_KICKED_FROM_GROUP);

            ... More Code In c_chatmanager.cpp
        */

        // Decode minimal request (readyStatus, gameType) matching KONGOR's MatchmakingGroupPlayerReadyStatusRequest
        byte[] commandBytes = buffer.ReadCommandBytes();
        byte readyStatus = buffer.ReadInt8();
        byte gameType = buffer.ReadInt8();

        // Persist and broadcast using in-memory group if available
        MatchmakingGroup? group = MatchmakingService.Groups.Values
            .SingleOrDefault(g => g.Members.Any(m => m.Account.ID == session.ClientInformation.Account.ID));

        if (group is null)
        {
            // Fallback: solo partial update (keeps single-client flow working)
            Response.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_UPDATE);
            Response.WriteInt8(Convert.ToByte(ChatProtocol.TMMUpdateType.TMM_PARTIAL_GROUP_UPDATE));
            Response.WriteInt32(session.ClientInformation.Account.ID);
            Response.WriteInt8(1);
            Response.WriteInt16(1500);
            Response.WriteInt32(session.ClientInformation.Account.ID);
            Response.WriteInt8(Convert.ToByte(ChatProtocol.ArrangedMatchType.AM_MATCHMAKING));
            Response.WriteInt8(Convert.ToByte(ChatProtocol.TMMGameType.TMM_GAME_TYPE_NORMAL));
            Response.WriteString("caldavar");
            Response.WriteString("ap|ar|sd");
            Response.WriteString("EU|USE|USW");
            Response.WriteBool(true);
            Response.WriteInt8(1);
            Response.WriteInt8(1);
            Response.WriteBool(false);
            Response.WriteString(string.Empty);
            Response.WriteString(string.Empty);
            Response.WriteInt8(5);
            Response.WriteInt8(Convert.ToByte(ChatProtocol.TMMType.TMM_TYPE_CAMPAIGN));
            Response.WriteInt8(0);
            Response.WriteInt8(readyStatus);
            Response.WriteBool(false);
            Response.PrependBufferSize();
            session.SendAsync(Response.Data);
            return;
        }

        // Update readiness according to KONGOR UX:
        // - Ignore non-leader toggles (everyone except leader is implicitly ready)
        // - Only leader's ready (non-zero) triggers loading for all
        MatchmakingGroupMember caller = group.Members.Single(m => m.Account.ID == session.ClientInformation.Account.ID);
        if (!caller.IsLeader)
        {
            // Non-leaders are implicitly ready; do not emit any updates here
            return;
        }

        bool leaderReady = readyStatus != 0;
        if (leaderReady)
        {
            // Implicitly ready all other members and broadcast StartLoading to all
            foreach (var member in group.Members)
                member.IsReady = true;

            // Tell clients to start preloading the map
            ChatBuffer startLoading = new();
            startLoading.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_START_LOADING);
            startLoading.PrependBufferSize();
            Parallel.ForEach(group.Members, m => m.Session.SendAsync(startLoading.Data));
        }

        // Broadcast group-wide partial update with per-member triplets
        ChatBuffer update = new();
        update.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_UPDATE);
        update.WriteInt8(Convert.ToByte(ChatProtocol.TMMUpdateType.TMM_PARTIAL_GROUP_UPDATE));
        update.WriteInt32(session.ClientInformation.Account.ID);           // Actor
        update.WriteInt8(Convert.ToByte(group.Members.Count));             // Group size
        update.WriteInt16(1500);                                           // Avg rating placeholder
        update.WriteInt32(group.Leader.Account.ID);                        // Leader ID
        update.WriteInt8(Convert.ToByte(ChatProtocol.ArrangedMatchType.AM_MATCHMAKING));
        update.WriteInt8(Convert.ToByte(ChatProtocol.TMMGameType.TMM_GAME_TYPE_NORMAL));
        update.WriteString("caldavar");
        update.WriteString("ap|ar|sd");
        update.WriteString("EU|USE|USW");
        update.WriteBool(true);                                            // Ranked
        update.WriteInt8(1);                                               // Match Fidelity
        update.WriteInt8(1);                                               // Bot Difficulty
        update.WriteBool(false);                                           // Randomize Bots
        update.WriteString(string.Empty);                                  // Country Restrictions
        update.WriteString(string.Empty);                                  // Invite Responses
        update.WriteInt8(5);                                               // Team size
        update.WriteInt8(Convert.ToByte(ChatProtocol.TMMType.TMM_TYPE_CAMPAIGN)); // Group Type

        foreach (var member in group.Members)
        {
            update.WriteInt8(member.LoadingPercent);                       // Loading Percent
            update.WriteInt8(Convert.ToByte(member.IsReady));              // Ready Status
            update.WriteBool(member.IsInGame);                             // In game
        }

        update.PrependBufferSize();
        Parallel.ForEach(group.Members, m => m.Session.SendAsync(update.Data));
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

Checklist

Broadcast StartLoading to all members when leader readies up
Stop requiring non-leaders to manually start loading
Track per-member loading and push group-wide partial updates
Advance beyond loading once all members hit 100%
What needed to happen

When leader sets ready (0x0D05): server must send NET_CHAT_CL_TMM_START_LOADING (0x0F03) to every group member so all clients begin preloading together. The client’s handler just PreloadWorld(m_sTMMMapName), no payload required.
While loading (0x0D04): server should broadcast NET_CHAT_CL_TMM_GROUP_UPDATE (0x0D03) as a TMM_PARTIAL_GROUP_UPDATE with only per-member triplets (loading, ready, inGame) for all members.
When everyone is at 100%: server should transition to the next step. Minimal path is to send:
NET_CHAT_CL_TMM_GROUP_JOIN_QUEUE (0x0D01)
Optionally pending match (0x0F04) + accept flow (0x0F05/0x0F06), or directly
NET_CHAT_CL_TMM_MATCH_FOUND_UPDATE (0x0D09)
NET_CHAT_CL_GAME_LOBBY_JOINED (0x1C09)
Actions taken

GroupPlayerReadyStatus:
If caller is leader and ready=true, mark all members ready and broadcast 0x0F03 to all.
Send a group-wide partial update (0x0D03) with correct header and per-member (loading, ready, inGame) bytes.
GroupPlayerLoadingStatus:
Update the caller’s LoadingPercent in the in-memory group.
Broadcast group-wide partial update with all members’ triplets.
When all members reach 100%, broadcast 0x0D01, then 0x0D09, then 0x1C09 to all.
Why this fixes your issues

Non-leaders no longer need to manually start loading: 0x0F03 goes to everyone as soon as the leader readies.
Stuck on loading: once all members report 100% via 0x0D04, the server now progresses the flow automatically.
Notes

0x0D03 partial update format is header + three bytes per member only; sending full-member fields there will crash the client.
0x0F03 has no payload (client ignores the packet body and starts preloading).
Quick test

Create group with multiple clients, toggle leader ready: all clients should enter loading without any manual action.
Let each client report 0x0D04 up to 100%: UI should move past loading automatically.
If you need the pending match accept flow, we can swap in 0x0F04/0x0F05/0x0F06 instead of the direct match found shortcut.
 */
