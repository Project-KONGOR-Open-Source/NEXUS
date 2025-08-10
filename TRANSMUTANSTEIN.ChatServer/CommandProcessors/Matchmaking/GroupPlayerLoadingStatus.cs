namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

[ChatCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_PLAYER_LOADING_STATUS)]
public class GroupPlayerLoadingStatus(ILogger<GroupPlayerLoadingStatus> logger) : CommandProcessorsBase, ICommandProcessor
{
    private ILogger<GroupPlayerLoadingStatus> Logger { get; } = logger;

    public async Task Process(ChatSession session, ChatBuffer buffer)
    {
        // Decode request (single byte loadingStatus)
        byte[] commandBytes = buffer.ReadCommandBytes();
        byte loadingStatus = buffer.ReadInt8();
        int accountId = session.ClientInformation.Account.ID;

        // TODO: Reconnect flow (if player should reconnect to match/lobby, skip processing)

        var group = MatchmakingService.FindGroupForAccount(accountId);
        if (group == null)
        {
            Logger.LogWarning("LoadingStatus: No group for account {AccountId}", accountId);
            // TODO: Failure response (generic / NET_CHAT_CL_TMM_GENERIC_RESPONSE)
            return;
        }
        var member = group.Members.FirstOrDefault(m => m.Account.ID == accountId);
        if (member == null)
        {
            Logger.LogWarning("LoadingStatus: Member not in group for account {AccountId}", accountId);
            return;
        }

        // Persist loading percent (0-100)
        member.LoadingPercent = loadingStatus; // TODO: clamp + validate monotonic increase

        // Broadcast partial update reflecting single member change (legacy KONGOR builds a partial group update)
        BroadcastPartial(group, actorAccountId: accountId);

        // When ALL members reach 100% and group was in LoadingResources -> move to queue
        if (group.AllMembersLoaded && group.State == MatchmakingGroupState.LoadingResources)
        {
            group.State = MatchmakingGroupState.InQueue;
            // Broadcast GROUP_JOINED_QUEUE (update type 6)
            BroadcastGroupUpdateWithType(group, ChatProtocol.TMMUpdateType.TMM_GROUP_JOINED_QUEUE);
            // TODO: Start queue timers / periodic TMM_GROUP_QUEUE_UPDATE packets (update type 11)
            // TODO: Defer match found simulation to a separate service / scheduler instead of immediate progression here.
        }
    }

    private void BroadcastPartial(MatchmakingGroup group, int actorAccountId)
    {
        Response.Clear();
        Response.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_UPDATE);
        Response.WriteInt8(Convert.ToByte(ChatProtocol.TMMUpdateType.TMM_PARTIAL_GROUP_UPDATE));
        Response.WriteInt32(actorAccountId);
        Response.WriteInt8(Convert.ToByte(group.Members.Count));
        Response.WriteInt16(1500); // TODO: average rating
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
        Response.WriteString(string.Empty); // Country restrictions TODO
        Response.WriteString(string.Empty); // Invitation responses TODO
        Response.WriteInt8(group.TeamSize);
        Response.WriteInt8(Convert.ToByte(group.GroupType));
        foreach (var m in group.Members)
        {
            Response.WriteInt32(m.Account.ID);
            Response.WriteString(m.Account.Name);
            Response.WriteInt8(m.Slot);
            Response.WriteInt16(1500); // TODO per-player rating
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
    }

    private void BroadcastGroupUpdateWithType(MatchmakingGroup group, ChatProtocol.TMMUpdateType type)
    {
        Response.Clear();
        Response.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_UPDATE);
        Response.WriteInt8(Convert.ToByte(type));
        Response.WriteInt32(group.Leader.Account.ID); // actor = leader placeholder
        Response.WriteInt8(Convert.ToByte(group.Members.Count));
        Response.WriteInt16(1500); // TODO avg rating
        Response.WriteInt32(group.Leader.Account.ID);
        Response.WriteInt8(Convert.ToByte(ChatProtocol.ArrangedMatchType.AM_MATCHMAKING));
        Response.WriteInt8(Convert.ToByte(group.GameType));
        Response.WriteString(group.MapName);
        Response.WriteString(group.GameModes);
        Response.WriteString(group.Regions);
        Response.WriteBool(group.Ranked);
        Response.WriteBool(group.MatchFidelity);
        Response.WriteInt8(group.BotDifficulty);
        Response.WriteBool(group.RandomizeBots);
        Response.WriteString(string.Empty);
        Response.WriteString(string.Empty);
        Response.WriteInt8(group.TeamSize);
        Response.WriteInt8(Convert.ToByte(group.GroupType));
        foreach (var m in group.Members)
        {
            Response.WriteInt32(m.Account.ID);
            Response.WriteString(m.Account.Name);
            Response.WriteInt8(m.Slot);
            Response.WriteInt16(1500);
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
            Response.WriteBool(false);
        Response.PrependBufferSize();
        group.Broadcast(Response.Clone());
    }
}

/*
GroupPlayerLoadingStatus (0x0D04) parity-focused implementation:
- Decodes loadingStatus byte and updates member.LoadingPercent.
- Sends TMM_PARTIAL_GROUP_UPDATE reflecting all members (mirrors legacy partial update after each change).
- When all members reach 100% while in LoadingResources -> transitions to InQueue and emits TMM_GROUP_JOINED_QUEUE (update type 6).
Removed earlier immediate simulation of match found / pending match / lobby to better match original request responsibilities (those belong to later matchmaking pipeline components).
Placeholders / TODOs:
- Failure responses when group not found or invalid state.
- Validate monotonic loading progression and reject regressions.
- Queue time update scheduling (TMM_GROUP_QUEUE_UPDATE) & popularity metrics.
- Separation of actor account vs leader for partial updates (legacy may echo actor id).
- Ratings, placement flags, ping info retrieval (currently static 1500).
- Distinguish REJOINED_QUEUE vs JOINED_QUEUE if resuming after disconnect.
- Integration with a future MatchFinder to trigger 0x0D09 (match found) asynchronously.
*/
