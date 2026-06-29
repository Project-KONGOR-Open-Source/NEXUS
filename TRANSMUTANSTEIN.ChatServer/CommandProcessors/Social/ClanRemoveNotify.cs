namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Social;

/// <summary>
///     Handles clan removal notifications, sent by the client after the master server's "set_rank" endpoint has removed the member from the clan.
///     Removes the target from the clan chat channel, broadcasts the rank change, and clears the target's in-memory clan membership.
///     When a leader removes themselves, it also broadcasts the promotion of the member that inherited ownership, unless the clan was disbanded.
/// </summary>
[ChatCommand(ChatProtocol.Command.CHAT_CMD_CLAN_REMOVE_NOTIFY)]
public class ClanRemoveNotify(MerrickContext merrick) : IAsynchronousCommandProcessor<ClientChatSession>
{
    public async Task Process(ClientChatSession session, ChatBuffer buffer)
    {
        ClanRemoveNotifyRequestData requestData = new (buffer);

        if (session.Account.Clan is null)
            return;

        // A Player Can Remove Themselves, But Only Leaders Can Remove Others
        if (session.Account.ID != requestData.TargetAccountID && session.Account.ClanTier is not ClanTier.Leader)
            return;

        ClientChatSession? targetSession = Context.ClientChatSessions.Values
            .SingleOrDefault(chatSession => chatSession.Account.ID == requestData.TargetAccountID);

        // Target Must Be Online And In The Same Clan
        if (targetSession is null || targetSession.Account.Clan?.ID != session.Account.Clan.ID)
            return;

        int clanID = session.Account.Clan.ID;

        // A Departing Leader Triggers The Transfer Of Clan Ownership, Which The Master Server's "set_rank" Endpoint Has Already Persisted
        bool leaderLeft = targetSession.Account.ClanTier is ClanTier.Leader;

        // Snapshot The Clan Members Before The Target's Membership Is Cleared, Since Self-Removal Clears The Initiator's Own Clan Reference
        List<Account> clanMembers = [.. session.Account.Clan.Members];

        // Remove The Target From The Clan Chat Channel
        string clanChannelName = session.Account.Clan.GetChatChannelName();

        if (Context.ChatChannels.TryGetValue(clanChannelName, out ChatChannel? clanChannel) && clanChannel.Members.ContainsKey(targetSession.Account.Name))
            clanChannel.Leave(targetSession);

        // Broadcast Rank Change (NONE = Removed) To All Clan Members Before Removal
        ChatBuffer rankChange = new ();

        rankChange.WriteCommand(ChatProtocol.Command.CHAT_CMD_CLAN_RANK_CHANGE);
        rankChange.WriteInt32(requestData.TargetAccountID);  // Target Account ID
        rankChange.WriteInt8(Convert.ToByte(ClanTier.None)); // Clan Tier (None = Removed)
        rankChange.WriteInt32(session.Account.ID);           // Initiator Account ID

        foreach (Account clanMember in clanMembers)
        {
            ClientChatSession? memberSession = Context.ClientChatSessions.Values
                .SingleOrDefault(chatSession => chatSession.Account.ID == clanMember.ID);

            memberSession?.Send(rankChange);
        }

        // Mirror The Clan Membership Reset That The Master Server's "set_rank" Endpoint Has Already Persisted To The Database
        targetSession.Account.Clan = null;
        targetSession.Account.ClanTier = ClanTier.None;
        targetSession.Account.TimestampJoinedClan = null;

        // Broadcast Name Change To Peers (Clan Tag Is Removed From Display Name)
        ChatBuffer nameChange = new ();

        nameChange.WriteCommand(ChatProtocol.Command.CHAT_CMD_NAME_CHANGE);
        nameChange.WriteInt32(targetSession.Account.ID);    // Target Account ID
        nameChange.WriteString(targetSession.Account.Name); // Target Account Name

        // Send To All Peers (Friends, Clan Members, Channel Mates)
        List<int> friendIDs = [.. targetSession.Account.FriendedPeers.Select(friend => friend.ID)];

        foreach (ClientChatSession peerSession in Context.ClientChatSessions.Values)
        {
            if (peerSession.Account.ID == targetSession.Account.ID)
                continue;

            if (friendIDs.Contains(peerSession.Account.ID))
                peerSession.Send(nameChange);
        }

        // When A Leader Leaves, Announce The New Leader That The Master Server Promoted: The Longest-Serving Officer, Or The Longest-Serving Member When There Are No Officers (If The Clan Was Disbanded Due To No Remaining Members Then There Is No New Leader)
        if (leaderLeft is false)
            return;

        Account? newLeader = await merrick.Accounts
            .SingleOrDefaultAsync(account => account.Clan != null && account.Clan.ID == clanID && account.ClanTier == ClanTier.Leader);

        if (newLeader is null)
            return;

        // Update The New Leader's In-Memory Session, If They Are Online
        ClientChatSession? newLeaderSession = Context.ClientChatSessions.Values
            .SingleOrDefault(chatSession => chatSession.Account.ID == newLeader.ID);

        if (newLeaderSession is not null)
            newLeaderSession.Account.ClanTier = ClanTier.Leader;

        // Broadcast The New Leader's Rank Change To The Remaining Clan Members
        ChatBuffer leadershipChange = new ();

        leadershipChange.WriteCommand(ChatProtocol.Command.CHAT_CMD_CLAN_RANK_CHANGE);
        leadershipChange.WriteInt32(newLeader.ID);                   // New Leader Account ID
        leadershipChange.WriteInt8(Convert.ToByte(ClanTier.Leader)); // Clan Tier
        leadershipChange.WriteInt32(requestData.TargetAccountID);    // Initiator Account ID (The Departing Leader)

        foreach (Account clanMember in clanMembers)
        {
            if (clanMember.ID == requestData.TargetAccountID)
                continue;

            ClientChatSession? memberSession = Context.ClientChatSessions.Values
                .SingleOrDefault(chatSession => chatSession.Account.ID == clanMember.ID);

            memberSession?.Send(leadershipChange);
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
