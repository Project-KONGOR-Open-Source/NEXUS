namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Social;

/// <summary>
///     Handles clan invitation acceptance and clan creation founding member acceptance.
///     The same command ID is used for both flows: checks pending invites first, then pending creations.
/// </summary>
[ChatCommand(ChatProtocol.Command.CHAT_CMD_CLAN_ADD_ACCEPTED)]
public class ClanInviteAccepted(MerrickContext merrick) : IAsynchronousCommandProcessor<ClientChatSession>
{
    public async Task Process(ClientChatSession session, ChatBuffer buffer)
    {
        ClanInviteAcceptedRequestData requestData = new (buffer);

        // Check Pending Clan Invites First
        if (PendingClan.Invites.TryRemove(session.Account.ID, out PendingClanInvite? invite))
        {
            await HandleClanInviteAcceptance(session, invite);

            return;
        }

        // Check Pending Clan Creations
        await HandleClanCreationAcceptance(session);
    }

    /// <summary>
    ///     Handles acceptance of a standard clan invite (joining an existing clan).
    /// </summary>
    private async Task HandleClanInviteAcceptance(ClientChatSession session, PendingClanInvite invite)
    {
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

    /// <summary>
    ///     Handles acceptance of a clan creation founding member invite.
    ///     If all 4 additional founding members have accepted, creates the clan.
    /// </summary>
    private async Task HandleClanCreationAcceptance(ClientChatSession session)
    {
        // Find The Pending Creation Where This Player Is A Founding Member
        PendingClanCreation? creation = PendingClan.Creations.Values
            .SingleOrDefault(creation => creation.IsFoundingMember(session.Account.ID));

        if (creation is null)
            return;

        bool allAccepted = creation.AcceptMember(session.Account.ID);

        if (allAccepted)
        {
            // All 4 Additional Founding Members Accepted: Create The Clan
            await CreateClan(creation);
        }

        else
        {
            // Notify The Founder That This Member Accepted
            ClientChatSession? founderSession = Context.ClientChatSessions.Values
                .SingleOrDefault(chatSession => chatSession.Account.ID == creation.FounderAccountID);

            if (founderSession is not null)
            {
                ChatBuffer accepted = new ();

                accepted.WriteCommand(ChatProtocol.Command.CHAT_CMD_CLAN_CREATE_ACCEPT);
                accepted.WriteString(session.Account.Name);

                founderSession.Send(accepted);
            }
        }
    }

    /// <summary>
    ///     Creates the clan in the database and notifies all founding members.
    /// </summary>
    private async Task CreateClan(PendingClanCreation creation)
    {
        // Remove The Pending Creation
        PendingClan.Creations.TryRemove(creation.FounderAccountID, out _);

        // Check If Clan Name/Tag Already Exists
        bool clanNameExists = await merrick.Clans.AnyAsync(clan => clan.Name == creation.ClanName);

        if (clanNameExists)
        {
            SendClanCreateFailure(creation.FounderAccountID, ChatProtocol.Command.CHAT_CMD_CLAN_CREATE_FAIL_NAME);

            return;
        }

        bool clanTagExists = await merrick.Clans.AnyAsync(clan => clan.Tag == creation.ClanTag);

        if (clanTagExists)
        {
            SendClanCreateFailure(creation.FounderAccountID, ChatProtocol.Command.CHAT_CMD_CLAN_CREATE_FAIL_TAG);

            return;
        }

        // Create The Clan In The Database
        Clan newClan = new ()
        {
            Name = creation.ClanName,
            Tag = creation.ClanTag
        };

        merrick.Clans.Add(newClan);
        await merrick.SaveChangesAsync();

        // Add The Founder As Leader
        Account founderAccount = await merrick.Accounts
            .SingleAsync(account => account.ID == creation.FounderAccountID);

        founderAccount.Clan = newClan;
        founderAccount.ClanTier = ClanTier.Leader;
        founderAccount.TimestampJoinedClan = DateTimeOffset.UtcNow;

        newClan.Members.Add(founderAccount);

        // Add The 4 Additional Founding Members As Officers
        for (int index = 0; index < creation.TargetAccountIDs.Length; index++)
        {
            Account memberAccount = await merrick.Accounts
                .SingleAsync(account => account.ID == creation.TargetAccountIDs[index]);

            memberAccount.Clan = newClan;
            memberAccount.ClanTier = ClanTier.Officer;
            memberAccount.TimestampJoinedClan = DateTimeOffset.UtcNow;

            newClan.Members.Add(memberAccount);
        }

        await merrick.SaveChangesAsync();

        // Update In-Memory Sessions And Send Notifications

        // Founder
        ClientChatSession? founderSession = Context.ClientChatSessions.Values
            .SingleOrDefault(chatSession => chatSession.Account.ID == creation.FounderAccountID);

        if (founderSession is not null)
        {
            founderSession.Account.Clan = newClan;
            founderSession.Account.ClanTier = ClanTier.Leader;

            ChatBuffer complete = new ();

            complete.WriteCommand(ChatProtocol.Command.CHAT_CMD_CLAN_CREATE_COMPLETE);
            complete.WriteCommand(ChatProtocol.Command.CHAT_CMD_NEW_CLAN_MEMBER);
            complete.WriteInt32(founderAccount.ID);
            complete.WriteInt32(newClan.ID);
            complete.WriteString(newClan.Name);
            complete.WriteString(newClan.Tag);

            founderSession.Send(complete);
        }

        // Founding Members
        for (int index = 0; index < creation.TargetAccountIDs.Length; index++)
        {
            ClientChatSession? memberSession = Context.ClientChatSessions.Values
                .SingleOrDefault(chatSession => chatSession.Account.ID == creation.TargetAccountIDs[index]);

            if (memberSession is null)
                continue;

            memberSession.Account.Clan = newClan;
            memberSession.Account.ClanTier = ClanTier.Officer;

            ChatBuffer memberNotification = new ();

            memberNotification.WriteCommand(ChatProtocol.Command.CHAT_CMD_NEW_CLAN_MEMBER);
            memberNotification.WriteInt32(creation.TargetAccountIDs[index]);
            memberNotification.WriteInt32(newClan.ID);
            memberNotification.WriteString(newClan.Name);
            memberNotification.WriteString(newClan.Tag);

            memberSession.Send(memberNotification);
        }

        // Join The Clan Channel
        ChatChannel clanChannel = ChatChannel.GetOrCreate(
            founderSession ?? Context.ClientChatSessions.Values.Single(chatSession => chatSession.Account.ID == creation.TargetAccountIDs[0]),
            newClan.GetChatChannelName());

        if (founderSession is not null)
            clanChannel.Join(founderSession);

        for (int index = 0; index < creation.TargetAccountIDs.Length; index++)
        {
            ClientChatSession? memberSession = Context.ClientChatSessions.Values
                .SingleOrDefault(chatSession => chatSession.Account.ID == creation.TargetAccountIDs[index]);

            if (memberSession is not null)
                clanChannel.Join(memberSession);
        }
    }

    private static void SendClanCreateFailure(int founderAccountID, ushort failureCommand)
    {
        ClientChatSession? founderSession = Context.ClientChatSessions.Values
            .SingleOrDefault(chatSession => chatSession.Account.ID == founderAccountID);

        if (founderSession is null)
            return;

        ChatBuffer failure = new ();

        failure.WriteCommand(failureCommand);

        founderSession.Send(failure);
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
