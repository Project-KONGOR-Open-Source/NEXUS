using TRANSMUTANSTEIN.ChatServer.Services;
using TRANSMUTANSTEIN.ChatServer.Domain.Matchmaking;

namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

[ChatCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_JOIN)]
public class GroupJoin(IMatchmakingService matchmakingService, MerrickContext merrick) : ISynchronousCommandProcessor<ChatSession>
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        GroupJoinRequestData requestData = new(buffer);

        // 1. Try to find group where the user is ALREADY a member (Rejoin)
        MatchmakingGroup? group = matchmakingService.GetMatchmakingGroupByMemberName(requestData.InviteIssuerName);

        // 2. If not found, try to find group where user is INVITED
        // This handles cases where the client sends the Invitee's name (Self) in the JOIN packet instead of the Leader's name.
        if (group is null)
        {
            group = matchmakingService.GetMatchmakingGroupByInvitedUser(requestData.InviteIssuerName);
        }

        if (group is null)
        {
            // TODO: Send Failure Response (TMM_FAILED_TO_JOIN)
            // For now, logging warning to match strict parity with previous potential behavior
             Log.Warning(
                 @"No Matchmaking Group Found For Account Name ""{AccountName}""",
                 requestData.InviteIssuerName);
             return;
        }

        group.Join(session, merrick);
    }
}