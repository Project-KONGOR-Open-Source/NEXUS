namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Social;

/// <summary>
///     Handles clan invitation requests.
///     Validates permissions and target eligibility, then sends the invite to the target player.
/// </summary>
[ChatCommand(ChatProtocol.Command.CHAT_CMD_CLAN_ADD_MEMBER)]
public class ClanInvite : ISynchronousCommandProcessor<ClientChatSession>
{
    public void Process(ClientChatSession session, ChatBuffer buffer)
    {
        ClanInviteRequestData requestData = new (buffer);

        if (string.IsNullOrEmpty(requestData.TargetName))
        {
            SendFailure(session, ChatProtocol.Command.CHAT_CMD_CLAN_ADD_FAIL_ONLINE);

            return;
        }

        // Find Target Session
        ClientChatSession? targetSession = Context.ClientChatSessions.Values
            .SingleOrDefault(chatSession => chatSession.Account.Name.Equals(requestData.TargetName, StringComparison.OrdinalIgnoreCase));

        // Target Not Found, Disconnected, Or Invisible
        if (targetSession is null || targetSession.Metadata.ClientChatModeState is ChatProtocol.ChatModeType.CHAT_MODE_INVISIBLE)
        {
            SendFailure(session, ChatProtocol.Command.CHAT_CMD_CLAN_ADD_FAIL_ONLINE);

            return;
        }

        // Target Is Already In A Clan
        if (targetSession.Account.Clan is not null)
        {
            SendFailure(session, ChatProtocol.Command.CHAT_CMD_CLAN_ADD_FAIL_CLAN);

            return;
        }

        // Target Is DND — Send Auto-Response And Do Not Deliver The Invite
        if (targetSession.Metadata.ClientChatModeState is ChatProtocol.ChatModeType.CHAT_MODE_DND)
        {
            ChatBuffer autoResponse = new ();

            autoResponse.WriteCommand(ChatProtocol.Command.CHAT_CMD_CHAT_MODE_AUTO_RESPONSE);
            autoResponse.WriteInt8(Convert.ToByte(ChatProtocol.ChatModeType.CHAT_MODE_DND));
            autoResponse.WriteString(targetSession.Account.Name);
            autoResponse.WriteString(targetSession.Metadata.ClientChatModeReason);

            session.Send(autoResponse);

            return;
        }

        // Requester Must Have A Clan
        if (session.Account.Clan is null)
        {
            SendFailure(session, ChatProtocol.Command.CHAT_CMD_CLAN_ADD_FAIL_UNKNOWN);

            return;
        }

        // Requester Must Be An Officer Or Leader
        if (session.Account.ClanTier is not ClanTier.Officer and not ClanTier.Leader)
        {
            SendFailure(session, ChatProtocol.Command.CHAT_CMD_CLAN_ADD_FAIL_PERMS);

            return;
        }

        // Target Already Has A Pending Clan Invite
        if (PendingClan.Invites.ContainsKey(targetSession.Account.ID))
        {
            SendFailure(session, ChatProtocol.Command.CHAT_CMD_CLAN_ADD_FAIL_INVITED);

            return;
        }

        // Store The Pending Invite
        PendingClanInvite invite = new ()
        {
            TargetAccountID = targetSession.Account.ID,
            OriginAccountID = session.Account.ID,
            ClanID          = session.Account.Clan.ID,
            ClanName        = session.Account.Clan.Name,
            ClanTag         = session.Account.Clan.Tag
        };

        PendingClan.Invites[targetSession.Account.ID] = invite;

        // Send The Invite To The Target
        ChatBuffer invitePacket = new ();

        invitePacket.WriteCommand(ChatProtocol.Command.CHAT_CMD_CLAN_ADD_MEMBER);
        invitePacket.WriteString(session.Account.Name);
        invitePacket.WriteString(session.Account.Clan.Name);

        targetSession.Send(invitePacket);
    }

    private static void SendFailure(ClientChatSession session, ushort failureCommand)
    {
        ChatBuffer failure = new ();

        failure.WriteCommand(failureCommand);

        session.Send(failure);
    }
}

file class ClanInviteRequestData
{
    public byte[] CommandBytes { get; init; }

    public string TargetName { get; init; }

    public ClanInviteRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        TargetName = buffer.ReadString();
    }
}
