namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Communication;

public class SendWhisperRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes { get; init; } = buffer.ReadCommandBytes();

    public string TargetName { get; } = buffer.ReadString();

    public string Message { get; } = buffer.ReadString();
}