namespace KINESIS.Manager;

public class ConnectManagerRequest : ProtocolRequest
{
    private readonly int _accountId;
    public int AccountId => _accountId;
    private readonly string _sessionCookie;
    public string SessionCookie => _sessionCookie;
    private readonly int _chatProtocolVersion;
    public int ChatProtocolVersion => _chatProtocolVersion;

    public ConnectManagerRequest(int accountId, string sessionCookie, int chatProtocolVersion)
    {
        _accountId = accountId;
        _sessionCookie = sessionCookie;
        _chatProtocolVersion = chatProtocolVersion;
    }

    public static ConnectManagerRequest Decode(byte[] data, int offset, out int updatedOffset)
    {
        ConnectManagerRequest message = new ConnectManagerRequest(
            accountId: ReadInt(data, offset, out offset),
            sessionCookie: ReadString(data, offset, out offset),
            chatProtocolVersion: ReadInt(data, offset, out offset)
        );

        updatedOffset = offset;
        return message;
    }

}

