namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Channels;

public class SendChannelMessageRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes { get; init; } = buffer.ReadCommandBytes();

    public string Message { get; } = buffer.ReadString();

    public int ChannelID { get; } = buffer.ReadInt32();
}