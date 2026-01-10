namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Channels;

public class SilenceChannelMemberRequestData
{
    public SilenceChannelMemberRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        ChannelID = buffer.ReadInt32();
        TargetName = buffer.ReadString();
        DurationMilliseconds = buffer.ReadInt32();
    }

    public byte[] CommandBytes { get; init; }

    public int ChannelID { get; }

    public string TargetName { get; }

    public int DurationMilliseconds { get; }
}
