namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.MatchState;

[ChatCommand(ChatProtocol.GameServerToChatServer.NET_CHAT_GS_ANNOUNCE_MATCH)]
public class AnnounceMatch(Services.IMatchmakingService matchmakingService) : ISynchronousCommandProcessor<ChatSession>
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        AnnounceMatchRequestData requestData = new(buffer);

        Log.Information("Match Announced - MatchupID: {MatchupID}, MatchID: {MatchID}, Groups: {GroupCount}",
            requestData.MatchupID, requestData.MatchID, requestData.GroupIDs.Count);

        matchmakingService.ConfirmMatch(requestData.MatchupID, requestData.MatchID, 
            session.ServerMetadata.Address, session.ServerMetadata.Port);
    }
}
