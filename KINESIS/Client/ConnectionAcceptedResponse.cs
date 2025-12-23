namespace KINESIS.Client;

public class ConnectionAcceptedResponse : ProtocolResponse
{
    public override CommandBuffer Encode()
    {
        CommandBuffer buffer = new();
        buffer.WriteInt16(ChatServerResponse.ConnectionAccepted);
        return buffer;
    }
}

