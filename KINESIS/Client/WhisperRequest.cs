namespace KINESIS.Client;

public class WhisperRequest : ProtocolRequest
{
    private readonly string _nickname;
    public string Nickname => _nickname;
    private readonly string _message;
    public string Message => _message;

    public WhisperRequest(string nickname, string message)
    {
        _nickname = nickname;
        _message = message;
    }

    public static WhisperRequest Decode(byte[] data, int offset, out int updatedOffset)
    {
        string nickname = ReadString(data, offset, out offset);
        string message = ReadString(data, offset, out offset);

        updatedOffset = offset;
        return new WhisperRequest(nickname, message);
    }


}

