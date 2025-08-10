namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

[ChatCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_JOIN_QUEUE)]
public class GroupJoinQueue(ILogger<GroupJoinQueue> logger) : CommandProcessorsBase, ICommandProcessor
{
    private ILogger<GroupJoinQueue> Logger { get; } = logger;

    public async Task Process(ChatSession session, ChatBuffer buffer)
    {
        byte[] commandBytes = buffer.ReadCommandBytes();
        int callerId = session.ClientInformation.Account.ID;
        var group = MatchmakingService.FindGroupForAccount(callerId);
        if (group == null)
        {
            Logger.LogWarning("GroupJoinQueue: No group for account {AccountId}", callerId);
            // TODO: Send FAILED_TO_JOIN (0x0E0A) with reason ALREADY_QUEUED / GROUP_FULL etc.
            return;
        }

        bool isLeader = group.Leader.Account.ID == callerId;
        if (!isLeader)
        {
            Logger.LogDebug("GroupJoinQueue: Non-leader {AccountId} attempted to queue group led by {LeaderId}", callerId, group.Leader.Account.ID);
            // TODO: Optionally send generic response denying action.
            return; // For now silently ignore.
        }

        // Transition state if eligible (simplified checks)
        if (group.State == MatchmakingGroupState.LoadingResources || group.State == MatchmakingGroupState.WaitingToStart)
        {
            // TODO: Require all members ready & loaded before entering queue.
            group.State = MatchmakingGroupState.InQueue;
        }
        else if (group.State == MatchmakingGroupState.InQueue)
        {
            // Already in queue -> treat as rejoin request (no-op or convert to REJOINED update)
            // TODO: Distinguish REJOIN (TMM_GROUP_REJOINED_QUEUE) if we add persistence of wait time.
        }
        else
        {
            // Unexpected state, but allow (e.g., from MatchFound might be invalid) -> guard
            Logger.LogDebug("GroupJoinQueue: Received in state {State}", group.State);
        }

        // ACK original request with bare 0x0D01 (mirrors legacy behavior)
        Response.Clear();
        Response.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_JOIN_QUEUE);
        Response.PrependBufferSize();
        session.SendAsync(Response.Data);

        // Broadcast a GROUP_JOINED_QUEUE update (0x0D03 with update type 6)
        BroadcastQueueJoined(group);
    }

    private void BroadcastQueueJoined(MatchmakingGroup group)
    {
        Response.Clear();
        Response.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_UPDATE); // 0x0D03
        Response.WriteInt8(Convert.ToByte(ChatProtocol.TMMUpdateType.TMM_GROUP_JOINED_QUEUE));
        Response.WriteInt32(group.Leader.Account.ID);          // Actor: leader (placeholder)
        Response.WriteInt8(Convert.ToByte(group.Members.Count));
        Response.WriteInt16(1500);                             // TODO: Average rating
        Response.WriteInt32(group.Leader.Account.ID);
        Response.WriteInt8(Convert.ToByte(ChatProtocol.ArrangedMatchType.AM_MATCHMAKING)); // TODO derive from config
        Response.WriteInt8(Convert.ToByte(group.GameType));
        Response.WriteString(group.MapName);
        Response.WriteString(group.GameModes);
        Response.WriteString(group.Regions);
        Response.WriteBool(group.Ranked);
        Response.WriteBool(group.MatchFidelity);
        Response.WriteInt8(group.BotDifficulty);
        Response.WriteBool(group.RandomizeBots);
        Response.WriteString(string.Empty);                    // Country restrictions TODO
        Response.WriteString(string.Empty);                    // Invitation responses TODO
        Response.WriteInt8(group.TeamSize);
        Response.WriteInt8(Convert.ToByte(group.GroupType));

        foreach (var m in group.Members)
        {
            Response.WriteInt32(m.Account.ID);
            Response.WriteString(m.Account.Name);
            Response.WriteInt8(m.Slot);
            Response.WriteInt16(1500); // TODO per-player rating based on game type
            Response.WriteInt8(m.LoadingPercent);
            Response.WriteBool(m.IsReady);
            Response.WriteBool(m.IsInGame);
            Response.WriteBool(m.IsEligibleForMatchmaking);
            Response.WriteString(m.Account.ChatNameColour);
            Response.WriteString(m.Account.Icon);
            Response.WriteString(m.Country);
            Response.WriteBool(m.HasGameModeAccess);
            Response.WriteString(m.GameModeAccess);
        }
        foreach (var _ in group.Members)
            Response.WriteBool(false); // TODO friend flags

        Response.PrependBufferSize();
        group.Broadcast(Response.Clone());

        // TODO: Schedule periodic queue time updates (TMM_GROUP_QUEUE_UPDATE) including elapsed wait and estimated time.
        // TODO: Integrate popularity updates (TMM_POPULARITY_UPDATE) after a service collects metrics.
    }
}

/*
GroupJoinQueue (0x0D01) parity update attempt:
- Validates caller is group leader (else ignored; future: explicit rejection).
- Transitions group state to InQueue (simplified; real flow requires all loaded/ready).
- Sends bare 0x0D01 ACK followed by TMM_GROUP_JOINED_QUEUE roster update (update type 6) to all members.
- Provides placeholders for ratings, friend flags, country restrictions, invitation responses.
Pending TODOs for fuller parity with MatchmakingGroupJoinQueueResponse:
- Rejoin handling (TMM_GROUP_REJOINED_QUEUE) with preserved wait time.
- Queue time estimation & periodic TMM_GROUP_QUEUE_UPDATE packets.
- Popularity update feed (0x0D07) linking to live metrics.
- Validation of readiness / loading state before allowing queue entry.
- Failure response packet (0x0E0A) with specific TMMFailedToJoinReason codes.
- Integration with reconnect-to-last-game path prior to queue join.
- Enforce group size vs selected map/team size constraints.
*/
