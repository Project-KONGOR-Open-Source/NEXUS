using Log = Serilog.Log;

namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

[ChatCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_PLAYER_READY_STATUS)]
public class GroupPlayerReadyStatus : ISynchronousCommandProcessor<ChatSession>
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        GroupPlayerReadyStatusRequestData requestData = new(buffer);
        Log.Information(
            "[DEBUG] GroupPlayerReadyStatus Parsed: Ready={ReadyStatus}, GameType={GameType} ({(int)requestData.GameType})",
            requestData.ReadyStatus, requestData.GameType, (int) requestData.GameType);

        MatchmakingGroup
            .GetByMemberAccountID(session.Account.ID)
            .SendPlayerReadinessStatusUpdate(session, requestData.GameType);
    }
}