namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Results;

[ChatCommand(ChatProtocol.GameServerToChatServer.NET_CHAT_GS_MATCH_ID_RESULT)]
public class MatchIdResult(IDatabase distributedCacheStore) : IAsynchronousCommandProcessor<ChatSession>
{
    public async Task Process(ChatSession session, ChatBuffer buffer)
    {
        MatchIdResultRequestData requestData = new(buffer);

        if (requestData.Result is not ChatProtocol.MatchIDResult.MIDR_SUCCESS)
        {
            MatchStartData? data =
                await distributedCacheStore.GetMatchStartDataByMatchServerID(session.ServerMetadata.ServerID);

            if (data is null)
            {
                Log.Error(
                    @"[BUG] Unable To Retrieve Match Start Data For Server Session ""{SessionID}"" With Server ID ""{ServerID}""",
                    session.ID, session.ServerMetadata.ServerID);
            }

            else
            {
                Log.Error(@"Publishing Match ID ""{MatchID}"" Has Failed In {RequestTime}ms With Result ""{Result}""",
                    data.MatchID, requestData.RequestTimeMilliseconds, requestData.Result);
            }
        }
    }
}