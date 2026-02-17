namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Communication;

/// <summary>
///     Handles admin broadcast messages to all connected clients.
/// </summary>
[ChatCommand(ChatProtocol.Command.CHAT_CMD_MESSAGE_ALL)]
public class MessageAll : ISynchronousCommandProcessor<ClientChatSession>
{
    public void Process(ClientChatSession session, ChatBuffer buffer)
    {
        MessageAllRequestData requestData = new (buffer);

        // Only Staff Can Broadcast Type 0 (Global) Messages
        if (requestData.MessageType is not 0 || session.Account.Type is not AccountType.Staff)
            return;

        ChatBuffer broadcast = new ();

        broadcast.WriteCommand(ChatProtocol.Command.CHAT_CMD_MESSAGE_ALL);
        broadcast.WriteString(session.Account.Name);
        broadcast.WriteString(requestData.Message);

        foreach (ClientChatSession clientSession in Context.ClientChatSessions.Values)
            clientSession.Send(broadcast);
    }
}

file class MessageAllRequestData
{
    public byte[] CommandBytes { get; init; }

    public byte MessageType { get; init; }

    public int Value { get; init; }

    public string Message { get; init; }

    public MessageAllRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        MessageType = buffer.ReadInt8();
        Value = buffer.ReadInt32();
        Message = buffer.ReadString();
    }
}
