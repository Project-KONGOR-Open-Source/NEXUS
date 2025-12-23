namespace KINESIS.Matchmaking;

public class MatchmakingStartLoadingResponse : ProtocolResponse
{
    public override CommandBuffer Encode()
    {
        CommandBuffer buffer = new();
        buffer.WriteInt16(ChatServerResponse.StartedLoadingResources);
        return buffer;
    }
}
