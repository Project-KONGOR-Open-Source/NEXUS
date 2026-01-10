namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Communication;

public class SendWhisperRequestData
{
    public SendWhisperRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        TargetName = buffer.ReadString();
        Message = buffer.ReadString();
    }

    public byte[] CommandBytes { get; init; }

    public string TargetName { get; }

    public string Message { get; }
}
