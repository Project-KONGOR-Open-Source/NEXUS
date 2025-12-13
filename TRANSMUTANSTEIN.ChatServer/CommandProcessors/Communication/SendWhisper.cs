namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Communication;

[ChatCommand(ChatProtocol.Command.CHAT_CMD_WHISPER)]
public class SendWhisper : ISynchronousCommandProcessor<ClientChatSession>
{
    public void Process(ClientChatSession session, ChatBuffer buffer)
    {
        WhisperRequestData requestData = new (buffer);

        Whisper
            .Create(requestData.Message)
            .Send(session, requestData.TargetName);
    }
}

file class WhisperRequestData
{
    public byte[] CommandBytes { get; init; }

    public string TargetName { get; init; }

    public string Message { get; init; }

    public WhisperRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        TargetName = buffer.ReadString();
        Message = buffer.ReadString();
    }
}
