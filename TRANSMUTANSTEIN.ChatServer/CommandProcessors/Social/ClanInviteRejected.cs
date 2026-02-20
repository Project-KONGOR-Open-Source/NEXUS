namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Social;

/// <summary>
///     Handles clan invitation rejection and clan creation founding member rejection.
///     The same command ID is used for both flows — checks pending invites first, then pending creations.
/// </summary>
[ChatCommand(ChatProtocol.Command.CHAT_CMD_CLAN_ADD_REJECTED)]
public class ClanInviteRejected : ISynchronousCommandProcessor<ClientChatSession>
{
    public void Process(ClientChatSession session, ChatBuffer buffer)
    {
        ClanInviteRejectedRequestData requestData = new (buffer);

        // Check Pending Clan Invites First
        if (PendingClan.Invites.TryRemove(session.Account.ID, out PendingClanInvite? invite))
        {
            // Notify The Inviter That The Invite Was Rejected
            ClientChatSession? originSession = Context.ClientChatSessions.Values
                .SingleOrDefault(chatSession => chatSession.Account.ID == invite.OriginAccountID);

            if (originSession is not null)
            {
                ChatBuffer rejection = new ();

                rejection.WriteCommand(ChatProtocol.Command.CHAT_CMD_CLAN_ADD_REJECTED);
                rejection.WriteString(session.Account.Name);

                originSession.Send(rejection);
            }

            return;
        }

        // Check Pending Clan Creations
        HandleClanCreationRejection(session);
    }

    /// <summary>
    ///     Handles rejection of a clan creation founding member invite.
    ///     Removes the entire pending creation and notifies the founder.
    /// </summary>
    private static void HandleClanCreationRejection(ClientChatSession session)
    {
        // Find The Pending Creation Where This Player Is A Founding Member
        PendingClanCreation? creation = PendingClan.Creations.Values
            .SingleOrDefault(creation => creation.IsFoundingMember(session.Account.ID));

        if (creation is null)
            return;

        // Remove The Entire Pending Creation (One Rejection Cancels The Whole Thing)
        PendingClan.Creations.TryRemove(creation.FounderAccountID, out _);

        // Notify The Founder
        ClientChatSession? founderSession = Context.ClientChatSessions.Values
            .SingleOrDefault(chatSession => chatSession.Account.ID == creation.FounderAccountID);

        if (founderSession is not null)
        {
            ChatBuffer rejection = new ();

            rejection.WriteCommand(ChatProtocol.Command.CHAT_CMD_CLAN_CREATE_REJECT);
            rejection.WriteString(session.Account.Name);

            founderSession.Send(rejection);
        }
    }
}

file class ClanInviteRejectedRequestData
{
    public byte[] CommandBytes { get; init; }

    public ClanInviteRejectedRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
    }
}
