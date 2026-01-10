namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

public class GroupPlayerLoadingStatusRequestData
{
    public GroupPlayerLoadingStatusRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        LoadingPercent = buffer.ReadInt8();
    }

    public byte[] CommandBytes { get; init; }

    public byte LoadingPercent { get; }
}
