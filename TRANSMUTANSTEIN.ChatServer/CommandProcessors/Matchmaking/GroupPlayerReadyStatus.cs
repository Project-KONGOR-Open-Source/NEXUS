namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

[ChatCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_PLAYER_READY_STATUS)]
public class GroupPlayerReadyStatus(ILogger<GroupPlayerReadyStatus> logger) : CommandProcessorsBase, ICommandProcessor
{
    private ILogger<GroupPlayerReadyStatus> Logger { get; } = logger;

    public async Task Process(ChatSession session, ChatBuffer buffer)
    {
        byte[] commandBytes = buffer.ReadCommandBytes();
        byte readyStatus = buffer.ReadInt8();
        byte gameTypeByte = buffer.ReadInt8();
        var gameType = (ChatProtocol.TMMGameType)gameTypeByte; // TODO: validate matches group.GameType

        int accountId = session.ClientInformation.Account.ID;
        var group = MatchmakingService.FindGroupForAccount(accountId);
        if (group == null)
        {
            Logger.LogWarning("ReadyStatus: No group for account {AccountId}", accountId);
            return; // TODO: send failure response
        }
        var member = group.Members.FirstOrDefault(m => m.Account.ID == accountId);
        if (member == null)
        {
            Logger.LogWarning("ReadyStatus: Member not found in group for account {AccountId}", accountId);
            return;
        }

        // Persist readiness (even though only leader matters for flow)
        member.IsReady = readyStatus != 0;

        bool isLeader = member.IsLeader;

        // Placeholder: collect per-player rating & ping info (fixed rating 1500, no placement flags)
        // TODO: Query MerrickContext / stats tables for actual ratings by game type

        if (isLeader && member.IsReady && group.State == MatchmakingGroupState.WaitingToStart)
        {
            group.State = MatchmakingGroupState.LoadingResources;
            // Emit START_LOADING (0x0F03) before partial update to mirror legacy pipeline
            SendStartLoading(group);
        }

        // Broadcast partial update reflecting readiness change
        BroadcastPartial(group, actorAccountId: accountId);
    }

    private void SendStartLoading(MatchmakingGroup group)
    {
        Response.Clear();
        Response.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_START_LOADING);
        Response.PrependBufferSize();
        group.Broadcast(Response.Clone());
    }

    private void BroadcastPartial(MatchmakingGroup group, int actorAccountId)
    {
        Response.Clear();
        Response.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_UPDATE);
        Response.WriteInt8(Convert.ToByte(ChatProtocol.TMMUpdateType.TMM_PARTIAL_GROUP_UPDATE));
        Response.WriteInt32(actorAccountId);
        Response.WriteInt8(Convert.ToByte(group.Members.Count));
        Response.WriteInt16(1500); // TODO: average rating aggregation
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
        Response.WriteString(string.Empty); // country restrictions TODO
        Response.WriteString(string.Empty); // invite responses TODO
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
            Response.WriteBool(false); // friend flags TODO
        Response.PrependBufferSize();
        group.Broadcast(Response.Clone());
    }
}

/*
GroupPlayerReadyStatus (0x0D05) parity improvements:
- Parses readyStatus + gameType, finds in-memory group & member.
- Persists member.IsReady.
- If leader readies in WaitingToStart -> transition to LoadingResources and send START_LOADING (0x0F03) before partial update.
- Broadcasts TMM_PARTIAL_GROUP_UPDATE with roster reflecting readiness states.
Placeholders / TODOs:
- Real rating & placement data (PlayersInfo) gathering by game type.
- Validation of gameType mismatch vs stored group.GameType.
- Non-leader readiness semantics (possibly ignore or auto-ready others like legacy).
- Failure responses (0x0E0A) for invalid state/group not found.
- Country restrictions, invite responses, friend flags.
- TournamentRating / disparity & region validation prior to loading.
- Timer-driven auto-loading progression (legacy used a timer after 0x0F03 before 0x0D04 starts).
*/
