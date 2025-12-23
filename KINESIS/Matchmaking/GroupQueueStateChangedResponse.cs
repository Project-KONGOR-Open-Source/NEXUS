namespace KINESIS.Matchmaking;

public class GroupQueueStateChangedResponse : ProtocolResponse
{
    private readonly byte _updateType;
    public byte UpdateType => _updateType;
    private readonly int _averageTimeInQueueInSeconds;
    public int AverageTimeInQueueInSeconds => _averageTimeInQueueInSeconds;

    public GroupQueueStateChangedResponse(byte updateType, int averageTimeInQueueInSeconds)
    {
        _updateType = updateType;
        _averageTimeInQueueInSeconds = averageTimeInQueueInSeconds;
    }

    public override CommandBuffer Encode()
    {
        CommandBuffer buffer = new();
        buffer.WriteInt16(ChatServerResponse.GroupQueueStateChanged);
        buffer.WriteInt8(_updateType);
        if (_updateType == 11)
        {
            buffer.WriteInt32(_averageTimeInQueueInSeconds);
        }
        return buffer;
    }
}
