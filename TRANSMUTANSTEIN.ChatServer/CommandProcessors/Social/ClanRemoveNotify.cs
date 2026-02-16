namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Social;

/// <summary>
///     Handles clan removal notifications.
///     Removes a member from the clan (or self-removal) and broadcasts the rank change.
///     C++ reference: <c>c_client.cpp:1638</c> — <c>HandleClanRemoveNotification</c>.
/// </summary>
[ChatCommand(ChatProtocol.Command.CHAT_CMD_CLAN_REMOVE_NOTIFY)]
public class ClanRemoveNotify : ISynchronousCommandProcessor<ClientChatSession>
{
    public void Process(ClientChatSession session, ChatBuffer buffer)
    {
        ClanRemoveNotifyRequestData requestData = new (buffer);

        if (session.Account.Clan is null)
            return;

        // C++ Reference: A Player Can Remove Themselves, But Only Leaders Can Remove Others
        if (session.Account.ID != requestData.TargetAccountID && session.Account.ClanTier is not ClanTier.Leader)
            return;

        ClientChatSession? targetSession = Context.ClientChatSessions.Values
            .SingleOrDefault(chatSession => chatSession.Account.ID == requestData.TargetAccountID);

        // Target Must Be Online And In The Same Clan
        if (targetSession is null || targetSession.Account.Clan?.ID != session.Account.Clan.ID)
            return;

        // Broadcast Rank Change (NONE = Removed) To All Clan Members Before Removal
        ChatBuffer rankChange = new ();

        rankChange.WriteCommand(ChatProtocol.Command.CHAT_CMD_CLAN_RANK_CHANGE);
        rankChange.WriteInt32(requestData.TargetAccountID);
        rankChange.WriteInt8(Convert.ToByte(ClanTier.None));
        rankChange.WriteInt32(session.Account.ID);

        foreach (Account clanMember in session.Account.Clan.Members)
        {
            ClientChatSession? memberSession = Context.ClientChatSessions.Values
                .SingleOrDefault(chatSession => chatSession.Account.ID == clanMember.ID);

            memberSession?.Send(rankChange);
        }

        // Broadcast Name Change To Peers (Clan Tag Is Removed From Display Name)
        ChatBuffer nameChange = new ();

        nameChange.WriteCommand(ChatProtocol.Command.CHAT_CMD_NAME_CHANGE);
        nameChange.WriteInt32(targetSession.Account.ID);
        nameChange.WriteString(targetSession.Account.Name);

        // Send To All Peers (Friends, Clan Members, Channel Mates)
        List<int> friendIDs = [.. targetSession.Account.FriendedPeers.Select(friend => friend.ID)];

        foreach (ClientChatSession peerSession in Context.ClientChatSessions.Values)
        {
            if (peerSession.Account.ID == targetSession.Account.ID)
                continue;

            if (friendIDs.Contains(peerSession.Account.ID))
                peerSession.Send(nameChange);
        }
    }
}

file class ClanRemoveNotifyRequestData
{
    public byte[] CommandBytes { get; init; }

    public int TargetAccountID { get; init; }

    public ClanRemoveNotifyRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        TargetAccountID = buffer.ReadInt32();
    }
}
