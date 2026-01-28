namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.MatchState;

[ChatCommand(ChatProtocol.GameServerToChatServer.NET_CHAT_GS_MATCH_STARTED)]
public class MatchStarted : ISynchronousCommandProcessor<ChatSession>
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        MatchStartedRequestData requestData = new(buffer);

        Log.Information("Match Started - MatchupID: {MatchupID}", requestData.MatchupID);

        // TODO: Update database state to 'In Game' if not already done.
        // TODO: Start stats tracking logic?
    }
}
