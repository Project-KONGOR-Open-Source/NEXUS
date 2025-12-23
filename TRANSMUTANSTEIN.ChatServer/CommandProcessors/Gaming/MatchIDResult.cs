
using TRANSMUTANSTEIN.ChatServer.Domain.Core;

namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Gaming;

[ChatCommand(ChatProtocol.GameServerToChatServer.NET_CHAT_GS_MATCH_ID_RESULT)]
public class MatchIDResult : IAsynchronousCommandProcessor<MatchServerChatSession>
{
    public Task Process(MatchServerChatSession session, ChatBuffer buffer)
    {
        MatchIDResultData resultData = new (buffer);

        if (session.Metadata is not null)
        {
            if (resultData.Result != ChatProtocol.MatchIDResult.MIDR_SUCCESS)
            {
                Log.Warning(@"Match ID Assignment Failed For Match Server ID ""{ServerID}"" - Result: {Result}", session.Metadata.ServerID, resultData.Result);
            }
            else
            {
                Log.Information(@"Match ID ""{MatchID}"" Assigned To Match Server ID ""{ServerID}""", resultData.MatchID, session.Metadata.ServerID);
            }
        }
        else
        {
             Log.Warning(@"Received Match ID Result For Unauthenticated Match Server - Match ID: {MatchID}, Result: {Result}", resultData.MatchID, resultData.Result);
        }

        return Task.CompletedTask;
    }
}

file class MatchIDResultData
{
    public byte[] CommandBytes { get; init; }

    public int MatchID { get; init; }

    public ChatProtocol.MatchIDResult Result { get; init; }

    public MatchIDResultData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        MatchID = buffer.ReadInt32();
        Result = (ChatProtocol.MatchIDResult)buffer.ReadInt8();
    }
}
