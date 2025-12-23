namespace KINESIS.Matchmaking;

public class EnterMatchmakingQueueRequest : ProtocolRequest
{
    private readonly byte _readyStatus;
    public byte ReadyStatus => _readyStatus;
    private readonly byte _gameType;
    public byte GameType => _gameType;

    public EnterMatchmakingQueueRequest(byte readyStatus, byte gameType)
    {
        _readyStatus = readyStatus;
        _gameType = gameType;
    }

    public static EnterMatchmakingQueueRequest Decode(byte[] data, int offset, out int updatedOffset)
    {
        EnterMatchmakingQueueRequest enterMatchmakingQueueRequest = new EnterMatchmakingQueueRequest(
            readyStatus: ReadByte(data, offset, out offset),
            gameType: ReadByte(data, offset, out offset)
        );
        updatedOffset = offset;
        return enterMatchmakingQueueRequest;
    }

}

