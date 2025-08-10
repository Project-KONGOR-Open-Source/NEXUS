namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

[ChatCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_JOIN)]
public class GroupJoin(ILogger<GroupJoin> logger) : CommandProcessorsBase, ICommandProcessor
{
    private ILogger<GroupJoin> Logger { get; } = logger;

    public async Task Process(ChatSession session, ChatBuffer buffer)
    {
        GroupJoinRequestData requestData = new(buffer);
        int joiningAccountId = session.ClientInformation.Account.ID;

        // TODO: Reconnect flow (if player has a pending match/lobby) similar to KONGOR Subject.ReconnectToLastGame()
        // TODO: Ban / suspension duration lookup (MatchmakingFailedToJoinResponse equivalent)

        // Find target group by leader account name (initiator)
        var targetGroup = MatchmakingService.Groups.Values
            .FirstOrDefault(g => g.Leader.Account.Name.Equals(requestData.InitiatorAccountName, StringComparison.OrdinalIgnoreCase));

        if (targetGroup == null)
        {
            Logger.LogWarning("GroupJoin: No target group found for initiator '{Initiator}' (account {AccountId})", requestData.InitiatorAccountName, joiningAccountId);
            // TODO: Send NET_CHAT_CL_TMM_FAILED_TO_JOIN (0x0E0A) with reason (e.g. TMMFTJR_GROUP_FULL or TMMFTJR_BAD_STATS) when implemented.
            return;
        }

        // If already in a different group, TODO: remove from previous group (previousGroup.Remove(...))
        var existingGroup = MatchmakingService.FindGroupForAccount(joiningAccountId);
        if (existingGroup != null && existingGroup != targetGroup)
        {
            // Minimal: ignore removal; real impl should send PLAYER_LEFT_GROUP update to that group.
            // TODO: Remove member from existingGroup and broadcast TMM_PLAYER_LEFT_GROUP.
        }

        // If already in target group, no-op (could re-send full update)
        if (!targetGroup.Members.Any(m => m.Account.ID == joiningAccountId))
        {
            byte nextSlot = (byte)(targetGroup.Members.Count + 1); // simplistic slot assignment
            var newMember = new MatchmakingGroupMember(session)
            {
                Slot = nextSlot,
                IsLeader = false,
                IsReady = false,
                IsInGame = false,
                IsEligibleForMatchmaking = true,
                LoadingPercent = 0,
                GameModeAccess = string.Join('|', targetGroup.GameModes.Split('|').Select(_ => "true"))
            };
            targetGroup.Members.Add(newMember);
        }

        // Build FULL group update (parity-ish with KONGOR AcceptInvite path which sends updated roster)
        BuildAndBroadcastFullGroupUpdate(targetGroup, updateType: ChatProtocol.TMMUpdateType.TMM_FULL_GROUP_UPDATE, actorAccountId: joiningAccountId);
    }

    private void BuildAndBroadcastFullGroupUpdate(MatchmakingGroup group, ChatProtocol.TMMUpdateType updateType, int actorAccountId)
    {
        Response.Clear();
        Response.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_UPDATE);
        Response.WriteInt8(Convert.ToByte(updateType));
        Response.WriteInt32(actorAccountId); // Actor (joining player)
        Response.WriteInt8(Convert.ToByte(group.Members.Count));
        Response.WriteInt16(1500); // TODO: Compute average rating
        Response.WriteInt32(group.Leader.Account.ID);
        Response.WriteInt8(Convert.ToByte(ChatProtocol.ArrangedMatchType.AM_MATCHMAKING)); // TODO: derive from config
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
            Response.WriteBool(false); // TODO: friend flags

        Response.PrependBufferSize();
        group.Broadcast(Response.Clone());
    }
}

public class GroupJoinRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes = buffer.ReadCommandBytes();
    public string ClientVersion = buffer.ReadString();
    public string InitiatorAccountName = buffer.ReadString();
}

/*
GroupJoin (0x0C0B) updated toward KONGOR parity:
- Decodes version + initiator (leader) account name.
- Locates existing group by leader name (MatchmakingService.Groups scan).
- Adds joining player as new MatchmakingGroupMember (assigns next slot, copies mode access placeholders).
- Broadcasts FULL_GROUP_UPDATE to all members (simplification of separate PLAYER_JOINED_GROUP + FULL update).
Pending TODOs for full parity:
- Send FAILED_TO_JOIN (0x0E0A) with specific ChatProtocol.TMMFailedToJoinReason codes on validation failure.
- Implement invite validation (AcceptInvite semantics) and rejection path (TMM_PLAYER_REJECTED_GROUP_INVITE).
- Handle reconnect-to-last-game prior to join.
- Remove player cleanly from previous group (emit TMM_PLAYER_LEFT_GROUP) before adding to new.
- Capacity checks vs TeamSize / game type constraints.
- Ratings aggregation and per-player rating fields.
- Country restrictions & invitation responses aggregation.
- Friend flag calculation.
*/
