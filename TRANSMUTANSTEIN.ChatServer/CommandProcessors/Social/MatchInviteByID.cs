namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Social;

/// <summary>
///     Handles match invite requests by target account ID.
///     C++ reference: <c>c_client.cpp:1986</c> — <c>HandleInviteIDToServer</c>.
/// </summary>
[ChatCommand(ChatProtocol.Command.CHAT_CMD_INVITE_USER_ID)]
public class MatchInviteByID : ISynchronousCommandProcessor<ClientChatSession>
{
    public void Process(ClientChatSession session, ChatBuffer buffer)
    {
        MatchInviteByIDRequestData requestData = new (buffer);

        ClientChatSession? targetSession = Context.ClientChatSessions.Values
            .SingleOrDefault(chatSession => chatSession.Account.ID == requestData.TargetAccountID);

        MatchInvite.Send(session, targetSession);
    }
}

file class MatchInviteByIDRequestData
{
    public byte[] CommandBytes { get; init; }

    public int TargetAccountID { get; init; }

    public MatchInviteByIDRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        TargetAccountID = buffer.ReadInt32();
    }
}
