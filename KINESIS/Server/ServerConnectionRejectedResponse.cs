namespace KINESIS.Server;

public class ServerConnectionRejectedResponse : ProtocolResponse
{
    private readonly ConnectionRejectedReason _reason;
    public ConnectionRejectedReason Reason => _reason;

    public ServerConnectionRejectedResponse(ConnectionRejectedReason reason)
    {
        _reason = reason;
    }

    public override CommandBuffer Encode()
    {
        CommandBuffer buffer = new();
        buffer.WriteInt16(ChatServerResponse.ServerConnectionRejected);
        buffer.WriteInt8(Convert.ToByte(_reason));
        return buffer;
    }
}
