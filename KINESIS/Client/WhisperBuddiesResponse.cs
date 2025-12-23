namespace KINESIS.Client;

/// <summary>
///     Whisper Buddies is triggered when using `/b m` or `/f m` to message all of
///     your friends.
/// </summary>
public class WhisperBuddiesResponse : ProtocolResponse
{
    // The username when encoding the message. For `<username Whispered To Buddies:> message`.
    private readonly string _nickname;
    public string Nickname => _nickname;
    private readonly string _message;
    public string Message => _message;

    public WhisperBuddiesResponse(string nickname, string message)
    {
        _nickname = nickname;
        _message = message;
    }

    public override CommandBuffer Encode()
    {
        CommandBuffer response = new();
        response.WriteInt16(ChatServerResponse.WhisperedToBuddies);
        response.WriteString(_nickname);
        response.WriteString(_message);
        return response;
    }
}
