namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Social;

/// <summary>
///     Handles match invite requests by target name.
///     Sends the inviter's client info and server details to the target.
/// </summary>
[ChatCommand(ChatProtocol.Command.CHAT_CMD_INVITE_USER_NAME)]
public class MatchInviteByName : ISynchronousCommandProcessor<ClientChatSession>
{
    public void Process(ClientChatSession session, ChatBuffer buffer)
    {
        MatchInviteByNameRequestData requestData = new (buffer);

        ClientChatSession? targetSession = Context.ClientChatSessions.Values
            .SingleOrDefault(chatSession => chatSession.Account.Name.Equals(requestData.TargetName, StringComparison.OrdinalIgnoreCase));

        MatchInvite.Send(session, targetSession);
    }
}

file class MatchInviteByNameRequestData
{
    public byte[] CommandBytes { get; init; }

    public string TargetName { get; init; }

    public MatchInviteByNameRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        TargetName = buffer.ReadString();
    }
}
