namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Statistics;

public class SeasonStatisticsRequestData
{
    public SeasonStatisticsRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
    }

    public byte[] CommandBytes { get; init; }
}
