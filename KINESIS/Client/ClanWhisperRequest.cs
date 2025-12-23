namespace KINESIS.Client;

public class ClanWhisperRequest : ProtocolRequest
{
    private readonly string _message;
    public string Message => _message;

    public ClanWhisperRequest(string message)
    {
        _message = message;
    }

    public static ClanWhisperRequest Decode(byte[] data, int offset, out int updatedOffset)
    {
        string message = ReadString(data, offset, out offset);
        updatedOffset = offset;
        return new ClanWhisperRequest(message);
    }

}

