namespace KINESIS.Matchmaking;

public class EnteredMatchmakingQueueResponse : ProtocolResponse
{
    public override CommandBuffer Encode()
    {
        CommandBuffer buffer = new();
        buffer.WriteInt16(ChatServerResponse.EnteredMatchmakingQueue);
        return buffer;
    }
}
