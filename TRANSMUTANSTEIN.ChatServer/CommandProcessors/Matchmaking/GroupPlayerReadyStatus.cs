using Log = Serilog.Log;

using TRANSMUTANSTEIN.ChatServer.Services;
using TRANSMUTANSTEIN.ChatServer.Domain.Matchmaking;

namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

[ChatCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_PLAYER_READY_STATUS)]
public class GroupPlayerReadyStatus(IMatchmakingService matchmakingService) : ISynchronousCommandProcessor<ChatSession>
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        GroupPlayerReadyStatusRequestData requestData = new(buffer);

        MatchmakingGroup
            .GetByMemberAccountID(matchmakingService, session.Account.ID)
            .SendPlayerReadinessStatusUpdate(session, requestData.GameType);
    }
}