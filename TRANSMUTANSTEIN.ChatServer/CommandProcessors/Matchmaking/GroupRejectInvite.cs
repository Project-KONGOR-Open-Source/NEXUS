namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

[ChatCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_REJECT_INVITE)]
public class GroupRejectInvite : ISynchronousCommandProcessor<ChatSession>
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        GroupRejectInviteRequestData requestData = new (buffer);

        // Find The Inviter's Group
        MatchmakingGroup? group = MatchmakingService.GetMatchmakingGroup(requestData.InviterName);

        if (group is null)
            return;

        // Broadcast Rejection To All Group Members
        ChatBuffer rejectBroadcast = new ();

        rejectBroadcast.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_REJECT_INVITE);
        rejectBroadcast.WriteString(session.Account.Name);    // Rejecting Player Name
        rejectBroadcast.WriteString(requestData.InviterName); // Inviter Name

        foreach (MatchmakingGroupMember member in group.Members)
            member.Session.Send(rejectBroadcast);

        // Send Partial Group Update To All Members
        group.MulticastUpdate(session.Account.ID, ChatProtocol.TMMUpdateType.TMM_PARTIAL_GROUP_UPDATE);
    }
}

file class GroupRejectInviteRequestData
{
    public byte[] CommandBytes { get; init; }

    public string InviterName { get; init; }

    public GroupRejectInviteRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        InviterName = buffer.ReadString();
    }
}

