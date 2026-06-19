namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

[ChatCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_LEAVE)]
public class GroupLeave : ISynchronousCommandProcessor<ClientChatSession>
{
    public void Process(ClientChatSession session, ChatBuffer buffer)
    {
        GroupLeaveRequestData requestData = new (buffer);

        /*
            A Matchmaking Group Leave Request Can Legitimately Arrive With No Matching Server-Side Group

            EXAMPLE:
                A Solo Player's Group Is Removed The Moment Their Match Starts, Yet The Client Still Sends A Leave As It Connects To The Match Server
                In This Case, The Lookup Returning NULL Is Therefore The Expected No-Op Here Rather Than An Error
        */

        MatchmakingService
            .GetMatchmakingGroup(session.Account.ID)?
            .RemoveMember(session.Account.ID);
    }
}

file class GroupLeaveRequestData
{
    public byte[] CommandBytes { get; init; }

    public GroupLeaveRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
    }
}
