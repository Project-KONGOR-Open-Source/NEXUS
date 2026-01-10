namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

public class GroupJoinQueueRequestData
{
    public GroupJoinQueueRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
    }

    public byte[] CommandBytes { get; init; }
}
