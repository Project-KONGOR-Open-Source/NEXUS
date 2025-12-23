namespace KINESIS.Manager;

public class PingReceivedResponse : ProtocolResponse
{
    public override CommandBuffer Encode()
    {
        CommandBuffer buffer = new();
        buffer.WriteInt16(ChatServerResponse.PingReceived);
        return buffer;
    }
}
