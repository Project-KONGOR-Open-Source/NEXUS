namespace KINESIS.Matchmaking;

public class LeaveMatchmakingGroupRequest : ProtocolRequest
{
    public static LeaveMatchmakingGroupRequest Decode(byte[] data, int offset, out int updatedOffset)
    {
        updatedOffset = offset;
        return new();
    }

}


