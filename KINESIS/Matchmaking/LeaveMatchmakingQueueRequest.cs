namespace KINESIS.Matchmaking;

public class LeaveMatchmakingQueueRequest : ProtocolRequest
{
    public static LeaveMatchmakingQueueRequest Decode(byte[] data, int offset, out int updatedOffset)
    {
        updatedOffset = offset;
        return new();
    }

}

