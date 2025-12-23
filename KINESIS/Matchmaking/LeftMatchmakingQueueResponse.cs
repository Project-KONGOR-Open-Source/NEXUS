namespace KINESIS.Matchmaking;

public class LeftMatchmakingQueueResponse : ProtocolResponse
{
    public override CommandBuffer Encode()
    {
        CommandBuffer buffer = new();
        buffer.WriteInt16(ChatServerResponse.LeftMatchmakingQueue);
        return buffer;
    }
}
