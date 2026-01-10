namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Results;

[ChatCommand(ChatProtocol.GameServerToChatServer.NET_CHAT_GS_MATCH_ID_RESULT)]
public class MatchIDResult(IDatabase distributedCacheStore) : IAsynchronousCommandProcessor<ChatSession>
{
    public async Task Process(ChatSession session, ChatBuffer buffer)
    {
        MatchIDResultRequestData requestData = new(buffer);

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

file class MatchIDResultRequestData
{
    public MatchIDResultRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        Result = (ChatProtocol.MatchIDResult) buffer.ReadInt8();
        RequestTimeMilliseconds = buffer.ReadInt32();
    }

    public byte[] CommandBytes { get; init; }

    public ChatProtocol.MatchIDResult Result { get; }

    public int RequestTimeMilliseconds { get; }
}