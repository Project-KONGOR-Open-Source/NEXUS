namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.MatchState;

[ChatCommand(ChatProtocol.GameServerToChatServer.NET_CHAT_GS_MATCH_ONGOING)]
public class MatchStatus : ISynchronousCommandProcessor<ChatSession>
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        MatchStatusRequestData requestData = new(buffer);

        // TODO: Update Match Phase In Distributed Cache (In Progress, Paused, Resumed)
        // TODO: Update Player Availability States (Mark Players As InGame When Match Starts)
        // TODO: Track Match Duration For Statistics And Timeout Detection
    }
}