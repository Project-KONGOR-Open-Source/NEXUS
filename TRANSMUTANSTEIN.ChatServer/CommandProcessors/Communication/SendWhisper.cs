namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Communication;

[ChatCommand(ChatProtocol.Command.CHAT_CMD_WHISPER)]
public class SendWhisper : ISynchronousCommandProcessor<ChatSession>
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        SendWhisperRequestData requestData = new (buffer);

        Whisper
            .Create(requestData.Message)
            .Send(session, requestData.TargetName);
    }
}

file class SendWhisperRequestData
{
    public byte[] CommandBytes { get; init; }

    public string TargetName { get; init; }

    public string Message { get; init; }

    public SendWhisperRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        TargetName = buffer.ReadString();
        Message = buffer.ReadString();
    }
}

