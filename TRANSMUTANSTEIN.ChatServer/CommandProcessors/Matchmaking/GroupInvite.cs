namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

[ChatCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_INVITE)]
public class GroupInvite(MerrickContext merrick, MatchLockService matchLockService) : IAsynchronousCommandProcessor<ClientChatSession>
{
    public async Task Process(ClientChatSession session, ChatBuffer buffer)
    {
        GroupInviteRequestData requestData = new (buffer);

        // A Locked Player's Invite Is An Attempt To Play
        if (await matchLockService.Enforce(session))
            return;

        if (Context.ClientChatSessions.TryGetValue(requestData.InviteReceiverName, out ClientChatSession? receiverSession) && await matchLockService.HoldsLock(receiverSession))
        {
            // There Is No Failure Reply, Because "CChatManager::HandleTMMJoinFailed" Would Make The Inviter's Client Leave Its Own Group, And Inviting Leaves Nothing To Unlock On The Inviter's Screen
            session.SendNotice(MatchLockService.MessageSenderName, MatchLockService.InviteeLockedMessage(receiverSession.Account.NameWithClanTag));

            Log.Information(@"Not Inviting Account ""{ReceiverAccountName}"" For Account ""{InviterAccountName}"", Because The Receiver Holds A Match Lock",
                receiverSession.Account.NameWithClanTag, session.Account.NameWithClanTag);

            return;
        }

        MatchmakingGroup
            .GetByMemberAccountID(session.Account.ID)
            .Invite(session, merrick, requestData.InviteReceiverName);
    }
}

file class GroupInviteRequestData
{
    public byte[] CommandBytes { get; init; }

    public string InviteReceiverName { get; init; }

    public GroupInviteRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        InviteReceiverName = buffer.ReadString();
    }
}
