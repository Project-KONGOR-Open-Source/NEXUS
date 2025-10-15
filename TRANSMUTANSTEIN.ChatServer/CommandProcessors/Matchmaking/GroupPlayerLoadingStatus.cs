namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

[ChatCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_PLAYER_LOADING_STATUS)]
public class GroupPlayerLoadingStatus : ISynchronousCommandProcessor
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        GroupPlayerLoadingStatusRequestData requestData = new (buffer);

        MatchmakingGroup group = MatchmakingService.GetMatchmakingGroup(session.Account.ID)
            ?? throw new NullReferenceException($@"No Matchmaking Group Found For Invite Issuer ID ""{session.Account.ID}""");

        MatchmakingGroupMember groupMember = group.Members.Single(member => member.Account.ID == session.Account.ID);

        groupMember.LoadingPercent = requestData.LoadingPercent;

        bool loaded = group.Members.All(member => member.LoadingPercent is 100);

        if (loaded)
        {
            ChatBuffer queue = new ();

            queue.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_JOIN_QUEUE);

            Parallel.ForEach(group.Members, member => member.Session.Send(queue));

            group.QueueStartTime = DateTimeOffset.UtcNow;

            ChatBuffer load = new ();

            load.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_QUEUE_UPDATE);
            load.WriteInt8(Convert.ToByte(ChatProtocol.TMMUpdateType.TMM_GROUP_QUEUE_UPDATE));
            // TODO: Get Actual Average Time In Queue (In Seconds)
            load.WriteInt32(83);

            Parallel.ForEach(group.Members, member => member.Session.Send(load));
        }

        group.MulticastUpdate(session.Account.ID, ChatProtocol.TMMUpdateType.TMM_PARTIAL_GROUP_UPDATE);
    }
}

public class GroupPlayerLoadingStatusRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes = buffer.ReadCommandBytes();

    public byte LoadingPercent = buffer.ReadInt8();
}
