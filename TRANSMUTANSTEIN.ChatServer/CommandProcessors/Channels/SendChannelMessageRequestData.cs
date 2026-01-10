namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Channels;

public class SendChannelMessageRequestData
{
    public SendChannelMessageRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        Message = buffer.ReadString();
        ChannelID = buffer.ReadInt32();
    }

    public byte[] CommandBytes { get; init; }

    public string Message { get; }

    public int ChannelID { get; }
}
