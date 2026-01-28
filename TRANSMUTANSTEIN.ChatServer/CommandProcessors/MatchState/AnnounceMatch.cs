namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.MatchState;

[ChatCommand(ChatProtocol.GameServerToChatServer.NET_CHAT_GS_ANNOUNCE_MATCH)]
public class AnnounceMatch : ISynchronousCommandProcessor<ChatSession>
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        AnnounceMatchRequestData requestData = new(buffer);

        Log.Information("Match Announced - MatchupID: {MatchupID}, MatchID: {MatchID}, Groups: {GroupCount}",
            requestData.MatchupID, requestData.MatchID, requestData.GroupIDs.Count);

        // TODO: Retrieve Matchmaking Groups and Notify Clients
        // This likely involves finding the groups in memory and sending NET_CHAT_CL_TMM_MATCH_FOUND_UPDATE (0x0D09)
        // or similar to them.
    }
}
