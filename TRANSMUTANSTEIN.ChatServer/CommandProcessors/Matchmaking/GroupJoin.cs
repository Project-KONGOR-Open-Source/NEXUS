using TRANSMUTANSTEIN.ChatServer.Services;
using TRANSMUTANSTEIN.ChatServer.Domain.Matchmaking;

namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

[ChatCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_JOIN)]
public class GroupJoin(IMatchmakingService matchmakingService) : ISynchronousCommandProcessor<ChatSession>
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        GroupJoinRequestData requestData = new(buffer);

        MatchmakingGroup
            .GetByMemberAccountName(matchmakingService, requestData.InviteIssuerName)
            .Join(session);
    }
}