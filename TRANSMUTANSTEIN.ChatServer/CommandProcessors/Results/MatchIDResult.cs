namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Results;

[ChatCommand(ChatProtocol.GameServerToChatServer.NET_CHAT_GS_MATCH_ID_RESULT)]
public class MatchIDResult(IDatabase distributedCacheStore) : IAsynchronousCommandProcessor<MatchServerChatSession>
{
    public async Task Process(MatchServerChatSession session, ChatBuffer buffer)
    {
        MatchIDResultRequestData requestData = new (buffer);

        if (requestData.Result is not ChatProtocol.MatchIDResult.MIDR_SUCCESS)
        {
            MatchInformation? data = await distributedCacheStore.GetMatchInformationByMatchServerID(session.Metadata.ServerID);

            if (data is null)
            {
                Log.Error(@"[BUG] Unable To Retrieve Match Information For Server Session ""{SessionID}"" With Server ID ""{ServerID}""",
                    session.ID, session.Metadata.ServerID);
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
    public byte[] CommandBytes { get; init; }

    public ChatProtocol.MatchIDResult Result { get; init; }

    public int RequestTimeMilliseconds { get; init; }

    public MatchIDResultRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        Result = (ChatProtocol.MatchIDResult) buffer.ReadInt8();
        RequestTimeMilliseconds = buffer.ReadInt32();
    }
}
