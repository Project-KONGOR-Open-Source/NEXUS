namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Channels;

public class DemoteChannelMemberRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes { get; init; } = buffer.ReadCommandBytes();

    public int ChannelID { get; } = buffer.ReadInt32();

    public int TargetAccountID { get; } = buffer.ReadInt32();
}
