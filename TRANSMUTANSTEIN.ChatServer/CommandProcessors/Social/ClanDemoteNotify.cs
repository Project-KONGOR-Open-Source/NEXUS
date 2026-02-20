namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Social;

/// <summary>
///     Handles clan demotion notifications.
///     Demotes a clan officer to member rank and broadcasts the rank change to all clan members.
/// </summary>
[ChatCommand(ChatProtocol.Command.CHAT_CMD_CLAN_DEMOTE_NOTIFY)]
public class ClanDemoteNotify : ISynchronousCommandProcessor<ClientChatSession>
{
    public void Process(ClientChatSession session, ChatBuffer buffer)
    {
        ClanDemoteNotifyRequestData requestData = new (buffer);

        // Only Clan Leaders Can Demote Members
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
        broadcast.WriteInt32(requestData.TargetAccountID);
        broadcast.WriteInt8(Convert.ToByte(ClanTier.Member));
        broadcast.WriteInt32(session.Account.ID);

        foreach (Account clanMember in session.Account.Clan.Members)
        {
            ClientChatSession? memberSession = Context.ClientChatSessions.Values
                .SingleOrDefault(chatSession => chatSession.Account.ID == clanMember.ID);

            memberSession?.Send(broadcast);
        }
    }
}

file class ClanDemoteNotifyRequestData
{
    public byte[] CommandBytes { get; init; }

    public int TargetAccountID { get; init; }

    public ClanDemoteNotifyRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        TargetAccountID = buffer.ReadInt32();
    }
}
