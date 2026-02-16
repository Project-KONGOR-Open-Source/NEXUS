namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Social;

/// <summary>
///     Handles clan invitation rejection.
///     Removes the pending invite and notifies the inviter.
///     C++ reference: <c>c_clientmanager.cpp:1840</c> — <c>HandleClanInviteRejected</c>.
/// </summary>
[ChatCommand(ChatProtocol.Command.CHAT_CMD_CLAN_ADD_REJECTED)]
public class ClanInviteRejected : ISynchronousCommandProcessor<ClientChatSession>
{
    public void Process(ClientChatSession session, ChatBuffer buffer)
    {
        _ = new ClanInviteRejectedRequestData(buffer);

        // Remove The Pending Invite
        if (PendingClanInvites.Invites.TryRemove(session.Account.ID, out PendingClanInvite? invite) is false)
            return;

        // Notify The Inviter That The Invite Was Rejected
        ClientChatSession? originSession = Context.ClientChatSessions.Values
            .SingleOrDefault(chatSession => chatSession.Account.ID == invite.OriginAccountID);

        if (originSession is null)
            return;

        ChatBuffer rejection = new ();

        rejection.WriteCommand(ChatProtocol.Command.CHAT_CMD_CLAN_ADD_REJECTED);
        rejection.WriteString(session.Account.Name);

        originSession.Send(rejection);
    }
}

file class ClanInviteRejectedRequestData
{
    public byte[] CommandBytes { get; init; }

    public ClanInviteRejectedRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
    }
}
