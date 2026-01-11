namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

public class PopularityUpdateRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes { get; init; } = buffer.ReadCommandBytes();
}