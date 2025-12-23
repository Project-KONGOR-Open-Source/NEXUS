namespace KINESIS.Server;

public class ConnectServerRequest : ProtocolRequest
{
    private readonly int _serverId;
    public int ServerId => _serverId;
    private readonly string _sessionCookie;
    public string SessionCookie => _sessionCookie;
#pragma warning disable IDE0052
    private readonly int _chatProtocolVersion;
    public int ChatProtocolVersion => _chatProtocolVersion;
#pragma warning restore IDE0052

    public ConnectServerRequest(int serverId, string sessionCookie, int chatProtocolVersion)
    {
        _serverId = serverId;
        _sessionCookie = sessionCookie;
        _chatProtocolVersion = chatProtocolVersion;
    }

    public static ConnectServerRequest Decode(byte[] data, int offset, out int updatedOffset)
    {
        ConnectServerRequest message = new ConnectServerRequest(
            serverId: ReadInt(data, offset, out offset),
            sessionCookie: ReadString(data, offset, out offset),
            chatProtocolVersion: ReadInt(data, offset, out offset)
        );

        updatedOffset = offset;
        return message;
    }

}

