namespace TRANSMUTANSTEIN.ChatServer.Matchmaking.Responses;

public record LeftMatchmakingQueueResponse : IMatchmakingResponse
{
    public byte[] Serialize()
    {
        ChatBuffer buffer = new();
        buffer.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_LEAVE_QUEUE);
        buffer.PrependBufferSize();
        return buffer.Data;
    }
}
