namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

[ChatCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_PLAYER_LOADING_STATUS)]
public class GroupPlayerLoadingStatus(ILogger<GroupPlayerLoadingStatus> logger) : CommandProcessorsBase, ICommandProcessor
{
    private ILogger<GroupPlayerLoadingStatus> Logger { get; } = logger;

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

    public async Task Process(ChatSession session, ChatBuffer buffer)
    {
        GroupPlayerLoadingStatusRequestData requestData = new (buffer);

        // Update in-memory group member loading if group exists
        MatchmakingGroup? group = MatchmakingService.Groups.Values
            .SingleOrDefault(g => g.Members.Any(m => m.Account.ID == session.ClientInformation.Account.ID));

        if (group is not null)
        {
            MatchmakingGroupMember member = group.Members.Single(m => m.Account.ID == session.ClientInformation.Account.ID);
            member.LoadingPercent = requestData.LoadingPercent;
        }

        // Broadcast a partial group update reflecting current per-member loading
        ChatBuffer update = new ();
        update.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_UPDATE);
        update.WriteInt8(Convert.ToByte(ChatProtocol.TMMUpdateType.TMM_PARTIAL_GROUP_UPDATE));
        update.WriteInt32(session.ClientInformation.Account.ID); // Actor

        if (group is null)
        {
            // Solo fallback
            update.WriteInt8(1);
            update.WriteInt16(1500);
            update.WriteInt32(session.ClientInformation.Account.ID);
            update.WriteInt8(Convert.ToByte(ChatProtocol.ArrangedMatchType.AM_MATCHMAKING));
            update.WriteInt8(Convert.ToByte(ChatProtocol.TMMGameType.TMM_GAME_TYPE_NORMAL));
            update.WriteString("caldavar");
            update.WriteString("ap|ar|sd");
            update.WriteString("EU|USE|USW");
            update.WriteBool(true);
            update.WriteInt8(1);
            update.WriteInt8(1);
            update.WriteBool(false);
            update.WriteString(string.Empty);
            update.WriteString(string.Empty);
            update.WriteInt8(5);
            update.WriteInt8(Convert.ToByte(ChatProtocol.TMMType.TMM_TYPE_CAMPAIGN));
            update.WriteInt8(requestData.LoadingPercent);
            update.WriteInt8(0);
            update.WriteBool(false);
            update.PrependBufferSize();
            session.SendAsync(update.Data);
        }
        else
        {
            update.WriteInt8(Convert.ToByte(group.Members.Count));
            update.WriteInt16(1500);
            update.WriteInt32(group.Leader.Account.ID);
            update.WriteInt8(Convert.ToByte(ChatProtocol.ArrangedMatchType.AM_MATCHMAKING));
            update.WriteInt8(Convert.ToByte(ChatProtocol.TMMGameType.TMM_GAME_TYPE_NORMAL));
            update.WriteString("caldavar");
            update.WriteString("ap|ar|sd");
            update.WriteString("EU|USE|USW");
            update.WriteBool(true);
            update.WriteInt8(1);
            update.WriteInt8(1);
            update.WriteBool(false);
            update.WriteString(string.Empty);
            update.WriteString(string.Empty);
            update.WriteInt8(5);
            update.WriteInt8(Convert.ToByte(ChatProtocol.TMMType.TMM_TYPE_CAMPAIGN));

            foreach (MatchmakingGroupMember m in group.Members)
            {
                update.WriteInt8(m.LoadingPercent);
                update.WriteInt8(Convert.ToByte(m.IsReady));
                update.WriteBool(m.IsInGame);
            }

            update.PrependBufferSize();
            Parallel.ForEach(group.Members, m => m.Session.SendAsync(update.Data));
        }

        if (group is not null)
        {
            bool allLoaded = group.Members.All(m => m.LoadingPercent >= 100);
            if (allLoaded)
            {
                // Mirror KONGOR minimal flow when all are loaded:
                // 1) Join queue (or rejoin)
                await BroadcastBare(group, ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_JOIN_QUEUE);

                // 2) Optional queue update (avg time); we’ll omit payload and keep it minimal for now
                //    In KONGOR this is MatchmakingGroupQueueUpdateResponse(updateType: 11)

                // 3) For a thin placeholder, jump to match found and lobby join
                await BroadcastBare(group, ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_MATCH_FOUND_UPDATE);
                await BroadcastBare(group, ChatProtocol.ChatServerToClient.NET_CHAT_CL_GAME_LOBBY_JOINED);
            }
        }
    }

    private async Task BroadcastBare(MatchmakingGroup group, ushort command)
    {
        ChatBuffer buf = new ();
        buf.WriteCommand(command);
        buf.PrependBufferSize();
        Parallel.ForEach(group.Members, m => m.Session.SendAsync(buf.Data));
    }
}

public class GroupPlayerLoadingStatusRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes = buffer.ReadCommandBytes();
    public byte LoadingPercent = buffer.ReadInt8();
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
