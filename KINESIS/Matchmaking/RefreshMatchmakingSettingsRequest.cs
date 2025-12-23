namespace KINESIS.Matchmaking;

public class RefreshMatchmakingSettingsRequest : ProtocolRequest
{
    public static RefreshMatchmakingSettingsRequest Decode(byte[] data, int offset, out int updatedOffset)
    {
        updatedOffset = offset;
        return new();
    }

}

