namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

[ChatCommand(ChatProtocol.GameServerToChatServer.NET_CHAT_GS_MATCH_ID_RESULT)]
public class AnnounceMatchID(IDatabase distributedCacheStore) : IAsynchronousCommandProcessor<MatchServerChatSession>
{
    public async Task Process(MatchServerChatSession session, ChatBuffer buffer)
    {
        MatchIDResultData resultData = new (buffer);

        if (resultData.Result is not ChatProtocol.MatchIDResult.MIDR_SUCCESS)
        {
            MatchStartData? data = await distributedCacheStore.GetMatchStartDataByMatchServerID(session.Metadata.ServerID);

            if (data is null)
            {
                Log.Error(@"[BUG] Unable To Retrieve Match Start Data For Server Session ""{SessionID}"" With Server ID ""{ServerID}""",
                    session.ID, session.Metadata.ServerID);
            }

            else
            {
                Log.Error(@"Publishing Match ID ""{MatchID}"" Has Failed In {RequestTime}ms With Result ""{Result}""",
                    data.MatchID, resultData.RequestTimeMilliseconds, resultData.Result);
            }
        }
    }
}

file class MatchIDResultData
{
    public byte[] CommandBytes { get; init; }

    public ChatProtocol.MatchIDResult Result { get; init; }

    public int RequestTimeMilliseconds { get; init; }

    public MatchIDResultData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        Result = (ChatProtocol.MatchIDResult) buffer.ReadInt8();
        RequestTimeMilliseconds = buffer.ReadInt32();
    }
}
