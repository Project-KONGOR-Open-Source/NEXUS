namespace KINESIS.Matchmaking;

public class RefreshMatchmakingStatsRequest : ProtocolRequest
{
    public static RefreshMatchmakingStatsRequest Decode(byte[] data, int offset, out int updatedOffset)
    {
        updatedOffset = offset;
        return new RefreshMatchmakingStatsRequest();
    }

}

