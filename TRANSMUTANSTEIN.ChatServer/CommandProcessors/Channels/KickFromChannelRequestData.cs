namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Channels;

public class KickFromChannelRequestData
{
    public KickFromChannelRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        ChannelID = buffer.ReadInt32();
        TargetAccountID = buffer.ReadInt32();
    }

    public byte[] CommandBytes { get; init; }

    public int ChannelID { get; }

    public int TargetAccountID { get; }
}
