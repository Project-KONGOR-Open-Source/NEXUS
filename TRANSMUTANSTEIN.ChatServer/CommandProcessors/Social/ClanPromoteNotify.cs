namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Social;

/// <summary>
///     Handles clan promotion notifications.
///     Promotes a clan member to officer rank and broadcasts the rank change to all clan members.
/// </summary>
[ChatCommand(ChatProtocol.Command.CHAT_CMD_CLAN_PROMOTE_NOTIFY)]
public class ClanPromoteNotify : ISynchronousCommandProcessor<ClientChatSession>
{
    public void Process(ClientChatSession session, ChatBuffer buffer)
    {
        ClanPromoteNotifyRequestData requestData = new (buffer);

        // Only Clan Leaders Can Promote Members
        if (session.Account.Clan is null || session.Account.ClanTier is not ClanTier.Leader)
            return;

        ClientChatSession? targetSession = Context.ClientChatSessions.Values
            .SingleOrDefault(chatSession => chatSession.Account.ID == requestData.TargetAccountID);

        // Target Must Be Online And In The Same Clan
        if (targetSession is null || targetSession.Account.Clan?.ID != session.Account.Clan.ID)
            return;

        // Broadcast Rank Change To All Clan Members
        ChatBuffer broadcast = new ();

        broadcast.WriteCommand(ChatProtocol.Command.CHAT_CMD_CLAN_RANK_CHANGE);
        broadcast.WriteInt32(requestData.TargetAccountID);     // Target Account ID
        broadcast.WriteInt8(Convert.ToByte(ClanTier.Officer)); // Clan Tier
        broadcast.WriteInt32(session.Account.ID);              // Promoter Account ID

        foreach (Account clanMember in session.Account.Clan.Members)
        {
            ClientChatSession? memberSession = Context.ClientChatSessions.Values
                .SingleOrDefault(chatSession => chatSession.Account.ID == clanMember.ID);

            memberSession?.Send(broadcast);
        }
    }
}

file class ClanPromoteNotifyRequestData
{
    public byte[] CommandBytes { get; init; }

    public int TargetAccountID { get; init; }

    public ClanPromoteNotifyRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        TargetAccountID = buffer.ReadInt32();
    }
}
