namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

public class GroupLeaveRequestData
{
    public GroupLeaveRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
    }

    public byte[] CommandBytes { get; init; }
}
