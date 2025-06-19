namespace TRANSMUTANSTEIN.ChatServer.Matchmaking.Responses;

public record MatchmakingStartLoadingResponse : IMatchmakingResponse
{
    public byte[] Serialize()
    {
        ChatBuffer buffer = new();
        buffer.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_UPDATE);
        // TODO: Add specific loading state data
        buffer.PrependBufferSize();
        return buffer.Data;
    }
}
