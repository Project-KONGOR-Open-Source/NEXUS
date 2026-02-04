using TRANSMUTANSTEIN.ChatServer.Services;
using TRANSMUTANSTEIN.ChatServer.Domain.Matchmaking;
using TRANSMUTANSTEIN.ChatServer.Contracts;

namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

[ChatCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_CHANGE_GROUP_TYPE)]
public class GroupChangeType(IMatchmakingService matchmakingService) : ISynchronousCommandProcessor<ChatSession>
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        // Read Command Bytes first (2 bytes) - Assuming the buffer includes the command bytes if passed from ChatSession
        // Wait, ChatSession.ProcessDataSegment passes a NEW buffer made from the segment.
        // The segment INCLUDES the command bytes (0,1).
        // Let's verify GroupGameOptionUpdateRequestData usage.
        // GroupGameOptionUpdateRequestData reads Int16 at start.
        // So we must consume the command bytes.
        
        buffer.ReadInt16(); // Command ID

        ChatProtocol.TMMType newType = (ChatProtocol.TMMType)buffer.ReadInt8();

        MatchmakingGroup? group;

        try
        {
            group = matchmakingService.GetMatchmakingGroupByMemberID(session.Account.ID)
                    ?? throw new NullReferenceException(
                        $@"No Matchmaking Group Found For Account ID ""{session.Account.ID}""");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "GroupChangeType: Failed to get group for Account {AccountID}", session.Account.ID);
            return;
        }

        if (group.Leader.Session != session)
        {
             Log.Warning("GroupChangeType: Non-leader {AccountID} attempted to change group type", session.Account.ID);
             return;
        }

        Log.Information("GroupChangeType: Updating Group Type to {NewType} for Group {GroupGUID}", newType, group.GUID);
        group.UpdateGroupType(newType);
    }
}
