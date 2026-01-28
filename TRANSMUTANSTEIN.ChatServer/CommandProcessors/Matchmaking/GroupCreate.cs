using TRANSMUTANSTEIN.ChatServer.Services;
using TRANSMUTANSTEIN.ChatServer.Domain.Matchmaking;

namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

[ChatCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_CREATE)]
public class GroupCreate(IMatchmakingService matchmakingService) : ISynchronousCommandProcessor<ChatSession>
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        GroupCreateRequestData requestData = new(buffer);

        MatchmakingGroup
            .Create(matchmakingService, session, requestData.ToGroupInformation());
    }
}