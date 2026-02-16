namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

[ChatCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_KICK)]
public class GroupKick : ISynchronousCommandProcessor<ClientChatSession>
{
    public void Process(ClientChatSession session, ChatBuffer buffer)
    {
        GroupKickRequestData requestData = new (buffer);

        MatchmakingGroup group = MatchmakingGroup.GetByMemberAccountID(session.Account.ID);

        // Only The Leader Can Kick Members
        if (group.Leader.Account.ID != session.Account.ID)
        {
            Log.Warning(@"Non-Leader ""{AccountName}"" Tried To Kick A Member From Matchmaking Group", session.Account.Name);

            return;
        }

        // C++ Reference: HandleRequestKickTMMPlayer — Leader Cannot Kick Themselves
        if (group.Members.Any(member => member.Account.ID == session.Account.ID && member.Slot == requestData.KickTargetTeamSlot))
            return;

        group.KickMemberBySlot(requestData.KickTargetTeamSlot);
    }
}

file class GroupKickRequestData
{
    public byte[] CommandBytes { get; init; }

    /// <summary>
    ///     The team slot of the member to kick.
    ///     C++ reference: <c>c_client.cpp:3007</c> — <c>const byte yTeamSlot(pktRecv.ReadByte());</c>.
    /// </summary>
    public byte KickTargetTeamSlot { get; init; }

    public GroupKickRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        KickTargetTeamSlot = buffer.ReadInt8();
    }
}
