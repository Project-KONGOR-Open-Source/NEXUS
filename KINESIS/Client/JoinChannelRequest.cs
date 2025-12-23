namespace KINESIS.Client;

public class JoinChannelRequest : ProtocolRequest
{
    private readonly string _channelName;
    public string ChannelName => _channelName;

    public JoinChannelRequest(string channelName)
    {
        _channelName = channelName;
    }

    public static JoinChannelRequest Decode(byte[] data, int offset, out int updatedOffset)
    {
        JoinChannelRequest message = new(
            channelName: ReadString(data, offset, out offset)
        );

        updatedOffset = offset;
        return message;
    }


}

