namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Social;

/// <summary>
///     Handles match invite rejection notifications.
///     Notifies the inviter that their invite was declined.
///     C++ reference: <c>c_client.cpp:2010</c> — <c>HandleInviteRejected</c>.
/// </summary>
[ChatCommand(ChatProtocol.Command.CHAT_CMD_INVITE_REJECTED)]
public class MatchInviteRejected : ISynchronousCommandProcessor<ClientChatSession>
{
    public void Process(ClientChatSession session, ChatBuffer buffer)
    {
        MatchInviteRejectedRequestData requestData = new (buffer);

        ClientChatSession? inviterSession = Context.ClientChatSessions.Values
            .SingleOrDefault(chatSession => chatSession.Account.ID == requestData.InviterAccountID);

        if (inviterSession is null || requestData.InviterAccountID == session.Account.ID)
            return;

        // C++ Reference: DND Inviter Does Not Receive The Rejection
        if (inviterSession.Metadata.ClientChatModeState is ChatProtocol.ChatModeType.CHAT_MODE_DND)
            return;

        ChatBuffer rejection = new ();

        rejection.WriteCommand(ChatProtocol.Command.CHAT_CMD_INVITE_REJECTED);
        rejection.WriteString(session.Account.Name);
        rejection.WriteInt32(session.Account.ID);

        inviterSession.Send(rejection);
    }
}

file class MatchInviteRejectedRequestData
{
    public byte[] CommandBytes { get; init; }

    public int InviterAccountID { get; init; }

    public MatchInviteRejectedRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        InviterAccountID = buffer.ReadInt32();
    }
}
