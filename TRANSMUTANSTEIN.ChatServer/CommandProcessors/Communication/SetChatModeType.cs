namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Communication;

/// <summary>
///     Handles chat mode type changes (available, AFK, DND, invisible).
/// </summary>
[ChatCommand(ChatProtocol.Command.CHAT_CMD_SET_CHAT_MODE_TYPE)]
public class SetChatModeType : ISynchronousCommandProcessor<ClientChatSession>
{
    public void Process(ClientChatSession session, ChatBuffer buffer)
    {
        SetChatModeTypeRequestData requestData = new (buffer);

        session.Metadata.ClientChatModeState = requestData.ChatModeType;
        session.Metadata.ClientChatModeReason = requestData.Reason;

        // Echo The New Mode Back To The Client
        ChatBuffer response = new ();

        response.WriteCommand(ChatProtocol.Command.CHAT_CMD_SET_CHAT_MODE_TYPE);
        response.WriteInt8(Convert.ToByte(session.Metadata.ClientChatModeState)); // Client Chat Mode State
        response.WriteString(session.Metadata.ClientChatModeReason);              // Client Chat Mode Reason

        session.Send(response);
    }
}

file class SetChatModeTypeRequestData
{
    public byte[] CommandBytes { get; init; }

    public ChatProtocol.ChatModeType ChatModeType { get; init; }

    public string Reason { get; init; }

    public SetChatModeTypeRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        ChatModeType = (ChatProtocol.ChatModeType)buffer.ReadInt8();
        Reason = buffer.ReadString();
    }
}
