namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Social;

/// <summary>
///     Handles clan invitation acceptance.
///     Adds the target to the clan via direct database access (C++ uses HTTP to master server).
///     C++ reference: <c>c_clientmanager.cpp:1895</c> — <c>HandleClanInviteAccepted</c>.
/// </summary>
[ChatCommand(ChatProtocol.Command.CHAT_CMD_CLAN_ADD_ACCEPTED)]
public class ClanInviteAccepted(MerrickContext merrick) : IAsynchronousCommandProcessor<ClientChatSession>
{
    public async Task Process(ClientChatSession session, ChatBuffer buffer)
    {
        _ = new ClanInviteAcceptedRequestData(buffer);

        // Look Up The Pending Invite For This Player
        if (PendingClanInvites.Invites.TryRemove(session.Account.ID, out PendingClanInvite? invite) is false)
            return;

        // Find The Inviter
        ClientChatSession? originSession = Context.ClientChatSessions.Values
            .SingleOrDefault(chatSession => chatSession.Account.ID == invite.OriginAccountID);

        if (originSession is null)
            return;

        // Load The Clan With Members
        Clan? clan = await merrick.Clans
            .Include(clan => clan.Members)
            .SingleOrDefaultAsync(clan => clan.ID == invite.ClanID);

        if (clan is null)
            return;

        // Load The Target Account
        Account targetAccount = await merrick.Accounts
            .SingleAsync(account => account.ID == session.Account.ID);

        // Add The Target To The Clan
        targetAccount.Clan = clan;
        targetAccount.ClanTier = ClanTier.Member;
        targetAccount.TimestampJoinedClan = DateTimeOffset.UtcNow;

        clan.Members.Add(targetAccount);

        await merrick.SaveChangesAsync();

        // Update The In-Memory Session Account
        session.Account.Clan = clan;
        session.Account.ClanTier = ClanTier.Member;

        // Broadcast New Member To All Online Clan Members
        ChatBuffer broadcast = new ();

        broadcast.WriteCommand(ChatProtocol.Command.CHAT_CMD_NEW_CLAN_MEMBER);
        broadcast.WriteInt32(session.Account.ID);
        broadcast.WriteString(session.Account.Name);

        foreach (Account clanMember in clan.Members)
        {
            ClientChatSession? memberSession = Context.ClientChatSessions.Values
                .SingleOrDefault(chatSession => chatSession.Account.ID == clanMember.ID);

            memberSession?.Send(broadcast);
        }
    }
}

file class ClanInviteAcceptedRequestData
{
    public byte[] CommandBytes { get; init; }

    public ClanInviteAcceptedRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
    }
}
