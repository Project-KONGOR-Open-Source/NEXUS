namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.MatchState;

[ChatCommand(ChatProtocol.GameServerToChatServer.NET_CHAT_GS_ABANDON_MATCH)]
public class AbandonMatch : ISynchronousCommandProcessor<MatchServerChatSession>
{
    public void Process(MatchServerChatSession session, ChatBuffer buffer)
    {
        // 0x0504
        try
        {
            byte[] commandBytes = buffer.ReadCommandBytes();
            
            // Expected payload: MatchID (int)
            if (buffer.HasRemainingData())
            {
               int matchId = buffer.ReadInt32();
               Log.Information("Match Server {ServerID} Abandoning Match {MatchID}", session.Metadata.ServerID, matchId);
               
               // TODO: Update cache/state
            }
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Error parsing AbandonMatch packet");
        }
    }
}
