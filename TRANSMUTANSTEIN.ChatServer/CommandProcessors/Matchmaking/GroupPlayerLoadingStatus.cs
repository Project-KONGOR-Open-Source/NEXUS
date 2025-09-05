namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

[ChatCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_PLAYER_LOADING_STATUS)]
public class GroupPlayerLoadingStatus(ILogger<GroupPlayerLoadingStatus> logger) : CommandProcessorsBase, ICommandProcessor
{
    private ILogger<GroupPlayerLoadingStatus> Logger { get; } = logger;

    public async Task Process(ChatSession session, ChatBuffer buffer)
    {
        GroupPlayerLoadingStatusRequestData requestData = new (buffer);

        MatchmakingGroup group = MatchmakingService.GetMatchmakingGroup(session.ClientInformation.Account.ID)
            ?? throw new NullReferenceException($@"No Matchmaking Group Found For Invite Issuer ID ""{session.ClientInformation.Account.ID}""");

        MatchmakingGroupMember groupMember = group.Members.Single(member => member.Account.ID == session.ClientInformation.Account.ID);

        groupMember.LoadingPercent = requestData.LoadingPercent;

        bool loaded = group.Members.All(member => member.LoadingPercent is 100);

        if (loaded)
        {
            ChatBuffer queue = new ();

            queue.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_JOIN_QUEUE);

            queue.PrependBufferSize();

            Parallel.ForEach(group.Members, member => member.Session.SendAsync(queue.Data));

            ChatBuffer load = new ();

            load.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_QUEUE_UPDATE);
            load.WriteInt8(Convert.ToByte(ChatProtocol.TMMUpdateType.TMM_GROUP_QUEUE_UPDATE));
            // TODO: Get Actual Average Time In Queue (In Seconds)
            load.WriteInt32(83);

            load.PrependBufferSize();

            Parallel.ForEach(group.Members, member => member.Session.SendAsync(load.Data));
        }

        // TODO: Figure Out Why Load Updates Are Not Being Sent Anymore (Probably A Group Update Is Needed)
    }
}

public class GroupPlayerLoadingStatusRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes = buffer.ReadCommandBytes();
    public byte LoadingPercent = buffer.ReadInt8();
}
