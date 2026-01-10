namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.MatchState;

[ChatCommand(ChatProtocol.Command.CHAT_CMD_JOINED_GAME)]
public class JoinedMatch(IDatabase distributedCacheStore) : IAsynchronousCommandProcessor<ChatSession>
{
    public async Task Process(ChatSession session, ChatBuffer buffer)
    {
        JoinedMatchRequestData requestData = new(buffer);

        await session
            .JoinMatch(distributedCacheStore, requestData.MatchID);
    }
}