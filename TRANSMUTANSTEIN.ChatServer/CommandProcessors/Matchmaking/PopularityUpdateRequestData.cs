namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

public class PopularityUpdateRequestData
{
    public PopularityUpdateRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
    }

    public byte[] CommandBytes { get; init; }
}
