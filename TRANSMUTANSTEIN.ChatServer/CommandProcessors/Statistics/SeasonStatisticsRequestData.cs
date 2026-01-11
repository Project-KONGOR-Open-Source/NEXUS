namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Statistics;

public class SeasonStatisticsRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes { get; init; } = buffer.ReadCommandBytes();
}