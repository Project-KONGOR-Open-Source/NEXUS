namespace KINESIS.Matchmaking;

public class LoadingStatusChangedRequest : ProtocolRequest
{
    private readonly byte _loadingStatus;
    public byte LoadingStatus => _loadingStatus;

    public LoadingStatusChangedRequest(byte loadingStatus)
    {
        _loadingStatus = loadingStatus;
    }

    public static LoadingStatusChangedRequest Decode(byte[] data, int offset, out int updatedOffset)
    {
        LoadingStatusChangedRequest matchmakingGroupPlayerReadyStatus = new LoadingStatusChangedRequest(
            loadingStatus: ReadByte(data, offset, out offset)
        );
        updatedOffset = offset;
        return matchmakingGroupPlayerReadyStatus;
    }

}

