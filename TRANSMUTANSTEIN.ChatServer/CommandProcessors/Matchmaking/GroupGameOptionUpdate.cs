using TRANSMUTANSTEIN.ChatServer.Attributes;
using TRANSMUTANSTEIN.ChatServer.Domain.Core;
using TRANSMUTANSTEIN.ChatServer.Domain.Matchmaking;

namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

[ChatCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GAME_OPTION_UPDATE)]
public class GroupGameOptionUpdate : ISynchronousCommandProcessor<ChatSession>
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        GroupGameOptionUpdateRequestData requestData = new(buffer);

        MatchmakingGroup? group;

        try
        {
            group = MatchmakingGroup.GetByMemberAccountID(session.Account.ID);
        }
        catch
        {
            return;
        }

        if (group is null)
        {
            return;
        }

        if (group.Leader.Session != session)
        {
            return;
        }

        group.UpdateInformation(requestData.ToGroupInformation());
    }
}
