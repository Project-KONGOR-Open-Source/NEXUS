namespace KINESIS.Matchmaking;

public class MatchmakingFailedToJoinResponse : ProtocolResponse
{
    private readonly MatchmakingFailedToJoinReason _reason;
    public MatchmakingFailedToJoinReason Reason => _reason;
    private readonly int _banDurationMillis;
    public int BanDurationMillis => _banDurationMillis;

    public MatchmakingFailedToJoinResponse(MatchmakingFailedToJoinReason reason, int banDurationMillis = 0)
    {
        _reason = reason;
        _banDurationMillis = banDurationMillis;
    }

    public override CommandBuffer Encode()
    {
        CommandBuffer buffer = new();
        buffer.WriteInt16(ChatServerResponse.FailedToJoinMatchmakingGroup);
        buffer.WriteInt8(Convert.ToByte(_reason));

        if (_reason == MatchmakingFailedToJoinReason.Banned)
        {
            buffer.WriteInt32(_banDurationMillis);
        }
        return buffer;
    }
}
