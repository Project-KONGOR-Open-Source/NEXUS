namespace KINESIS.Server;

public class ServerConnectionAcceptedResponse : ProtocolResponse
{
    public override CommandBuffer Encode()
    {
        CommandBuffer buffer = new();
        buffer.WriteInt16(ChatServerResponse.ServerConnectionAccepted);
        return buffer;
    }
}
