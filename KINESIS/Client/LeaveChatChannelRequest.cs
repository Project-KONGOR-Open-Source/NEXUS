namespace KINESIS.Client;

public class LeaveChatChannelRequest : ProtocolRequest
{
    private readonly string _channelName;
    public string ChannelName => _channelName;

    public LeaveChatChannelRequest(string channelName)
    {
        _channelName = channelName;
    }

    public static LeaveChatChannelRequest Decode(byte[] data, int offset, out int updatedOffset)
    {
        LeaveChatChannelRequest message = new(
            channelName: ReadString(data, offset, out offset)
        );

        updatedOffset = offset;
        return message;
    }

}

