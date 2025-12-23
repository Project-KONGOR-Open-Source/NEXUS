namespace KINESIS.Client;

public class ConnectionRejectedResponse : ProtocolResponse
{
    private readonly ConnectionRejectedReason _reason;
    public ConnectionRejectedReason Reason => _reason;

    public ConnectionRejectedResponse(ConnectionRejectedReason reason)
    {
        _reason = reason;
    }

    public override CommandBuffer Encode()
    {
        CommandBuffer buffer = new();
        buffer.WriteInt16(ChatServerResponse.ConnectionRejected);
        buffer.WriteInt8(Convert.ToByte(_reason));
        return buffer;
    }
}
