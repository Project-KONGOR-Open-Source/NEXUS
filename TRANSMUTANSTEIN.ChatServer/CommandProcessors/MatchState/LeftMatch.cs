namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.MatchState;

[ChatCommand(ChatProtocol.Command.CHAT_CMD_LEFT_GAME)]
public class LeftMatch : ISynchronousCommandProcessor<ClientChatSession>
{
    public void Process(ClientChatSession session, ChatBuffer buffer)
    {
        LeftMatchRequestData requestData = new (buffer);

        session.LeaveMatch();
    }
}

file class LeftMatchRequestData
{
    public byte[] CommandBytes { get; init; }

    public LeftMatchRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
    }
}
