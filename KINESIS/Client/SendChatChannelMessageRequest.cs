namespace KINESIS.Client;

public class SendChatChannelMessageRequest : ProtocolRequest
{
    private readonly string _message;
    public string Message => _message;
    private readonly int _channelId;
    public int ChannelId => _channelId;

    public SendChatChannelMessageRequest(string message, int channelId)
    {
        _message = message;
        _channelId = channelId;
    }

    public static SendChatChannelMessageRequest Decode(byte[] data, int offset, out int updatedOffset)
    {
        SendChatChannelMessageRequest message = new(
            message: ReadString(data, offset, out offset),
            channelId: ReadInt(data, offset, out offset)
        );

        updatedOffset = offset;
        return message;
    }

}

