namespace KINESIS.Client;

public class NotifyJoinedGameRequest : ProtocolRequest
{
    private readonly string _gameName;
    public string GameName => _gameName;
    private readonly int _matchId;
    public int MatchId => _matchId;
    private readonly bool _joinChannel;
    public bool JoinChannel => _joinChannel;

    public NotifyJoinedGameRequest(string gameName, int matchId, bool joinChannel)
    {
        _gameName = gameName;
        _matchId = matchId;
        _joinChannel = joinChannel;
    }

    public static NotifyJoinedGameRequest Decode(byte[] data, int offset, out int updatedOffset)
    {
        string gameName = ReadString(data, offset, out offset);
        int matchId = ReadInt(data, offset, out offset);
        bool joinChannel = ReadByte(data, offset, out offset) != 0;
        updatedOffset = offset;

        return new NotifyJoinedGameRequest(gameName, matchId, joinChannel);
    }

}

