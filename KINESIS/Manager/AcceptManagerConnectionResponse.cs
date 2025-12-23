namespace KINESIS.Manager;

public class AcceptManagerConnectionResponse : ProtocolResponse
{
    public override CommandBuffer Encode()
    {
        CommandBuffer buffer = new();
        buffer.WriteInt16(ChatServerResponse.ManagerConnectionAccepted);
        return buffer;
    }
}

