namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.MatchState;

[ChatCommand(ChatProtocol.GameServerToChatServer.NET_CHAT_GS_MATCH_ABORTED)]
public class MatchAborted : ISynchronousCommandProcessor<ChatSession>
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        MatchAbortedRequestData requestData = new(buffer);

        Log.Warning("Match Aborted - MatchupID: {MatchupID}, Reason: {Reason}", requestData.MatchupID, requestData.Reason);

        // TODO: Release players back to lobby/queue?
    }
}
