namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Communication;

public class WhisperBuddiesRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes { get; init; } = buffer.ReadCommandBytes();

    public string Message { get; } = buffer.ReadString();
}
