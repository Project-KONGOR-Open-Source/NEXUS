namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

/// <summary>
///     Processes loading status updates for a matchmaking group member.
///     When all members reach 100% loading status it automatically joins the queue, complementing <see cref="GroupJoinQueue"/> which handles explicit queue join requests from the group leader.
///     Both paths validate the same conditions.
/// </summary>
[ChatCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_PLAYER_LOADING_STATUS)]
public class GroupPlayerLoadingStatus : ISynchronousCommandProcessor<ClientChatSession>
{
    public void Process(ClientChatSession session, ChatBuffer buffer)
    {
        GroupPlayerLoadingStatusRequestData requestData = new (buffer);

        MatchmakingGroup
            .GetByMemberAccountID(session.Account.ID)
            .SendLoadingStatusUpdate(session, requestData.LoadingPercent);
    }
}

public class GroupPlayerLoadingStatusRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes = buffer.ReadCommandBytes();

    public byte LoadingPercent = buffer.ReadInt8();
}
