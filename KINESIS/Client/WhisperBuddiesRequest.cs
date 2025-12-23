namespace KINESIS.Client;

/// <summary>
///     Whisper Buddies is triggered when using `/b m` or `/f m` to message all of
///     your friends.
/// </summary>
public class WhisperBuddiesRequest : ProtocolRequest
{
    // The username when encoding the message. For `<username Whispered To Buddies:> message`.
    private readonly string _message;
    public string Message => _message;

    public WhisperBuddiesRequest(string message)
    {
        _message = message;
    }

    public static WhisperBuddiesRequest Decode(byte[] data, int offset, out int updatedOffset)
    {
        string message = ReadString(data, offset, out offset);

        updatedOffset = offset;
        return new WhisperBuddiesRequest(message);
    }

}

