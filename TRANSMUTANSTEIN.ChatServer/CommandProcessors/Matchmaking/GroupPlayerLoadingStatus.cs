using TRANSMUTANSTEIN.ChatServer.Services;
using TRANSMUTANSTEIN.ChatServer.Domain.Matchmaking;

namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

/// <summary>
///     Processes loading status updates for a matchmaking group member.
///     When all members reach 100% loading status it automatically joins the queue, complementing
///     <see cref="GroupJoinQueue" /> which handles explicit queue join requests from the group leader.
///     Both paths validusing TRANSMUTANSTEIN.ChatServer.Services;
[ChatCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_PLAYER_LOADING_STATUS)]
public class GroupPlayerLoadingStatus(IMatchmakingService matchmakingService) : ISynchronousCommandProcessor<ChatSession>
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        GroupPlayerLoadingStatusRequestData requestData = new(buffer);

        MatchmakingGroup
            .GetByMemberAccountID(matchmakingService, session.Account.ID)
            .SendLoadingStatusUpdate(session, requestData.LoadingPercent);
    }
}