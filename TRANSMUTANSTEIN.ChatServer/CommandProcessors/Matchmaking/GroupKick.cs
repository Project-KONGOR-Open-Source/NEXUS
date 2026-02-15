namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

[ChatCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_KICK)]
public class GroupKick : ISynchronousCommandProcessor<ClientChatSession>
{
    public void Process(ClientChatSession session, ChatBuffer buffer)
    {
        GroupKickRequestData requestData = new (buffer);

        MatchmakingGroup group = MatchmakingGroup.GetByMemberAccountID(session.Account.ID);

        // Only The Leader Can Kick Members
        if (group.Leader.Account.ID != session.Account.ID)
        {
            Log.Warning(@"Non-Leader ""{AccountName}"" Tried To Kick A Member From Matchmaking Group", session.Account.Name);

            return;
        }

        group.KickMember(requestData.KickTargetAccountID);
    }
}

file class GroupKickRequestData
{
    public byte[] CommandBytes { get; init; }

    public int KickTargetAccountID { get; init; }

    public GroupKickRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        KickTargetAccountID = buffer.ReadInt32();
    }
}
